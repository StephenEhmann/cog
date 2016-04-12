using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class SCAAlignScorer : IPairwiseAlignmentScorer<Word, ShapeNode>
	{
		private const int MaxSubstitutionScore = 3500;
		private const int IndelCost = 1000;

		private readonly SegmentPool _segmentPool;
		private readonly IReadOnlySet<SymbolicFeature> _relevantConsFeatures;
		private readonly IReadOnlySet<SymbolicFeature> _relevantVowelFeatures; 
		private readonly IReadOnlyDictionary<SymbolicFeature, int> _featureWeights;
		private readonly IReadOnlyDictionary<FeatureSymbol, int> _valueMetrics;
		private readonly List<SoundClass> _soundClasses;
        private readonly int[][] _scoringMatrix;

        public SCAAlignScorer(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics, IEnumerable<SoundClass> contextualSoundClasses)
		{
			_segmentPool = segmentPool;
			_relevantVowelFeatures = new IDBearerSet<SymbolicFeature>(relevantVowelFeatures).ToReadOnlySet();
			_relevantConsFeatures = new IDBearerSet<SymbolicFeature>(relevantConsFeatures).ToReadOnlySet();
			_featureWeights = new Dictionary<SymbolicFeature, int>(featureWeights).ToReadOnlyDictionary();
			_valueMetrics = new Dictionary<FeatureSymbol, int>(valueMetrics).ToReadOnlyDictionary();

			IEnumerable<SoundClass> soundClasses;
			soundClasses = new SoundClass[]
					{
                        new UnnaturalClass("A", new[] {"a", "ᴀ", "ã", "ɑ", "á", "à", "ā", "ǎ", "â"}, true, project.Segmenter, 0),
                        new UnnaturalClass("B", new[] {"ɸ", "β", "f", "p͡f", "ƀ", "v", "ʙ"}, true, project.Segmenter, 1),
                        new UnnaturalClass("C", new[] {"t͡s", "d͡z", "ʦ", "ʣ", "t͡ɕ", "d͡ʑ", "ʨ", "ʥ", "t͡ʃ", "d͡ʒ", "ʧ", "ʤ", "c", "ɟ", "t͡ʂ", "d͡ʐ", "č", "ž", "t͡θ", "ʄ"}, true, project.Segmenter, 2),
                        new UnnaturalClass("D", new[] {"θ", "ð", "ŧ", "þ", "đ"}, true, project.Segmenter, 3),
                        new UnnaturalClass("E", new[] {"ɛ", "æ", "ɜ", "ɐ", "ʌ", "e", "ᴇ", "ə", "ɘ", "ɤ", "è", "é", "ē", "ě", "ê", "ɚ", "ǝ", "ẽ"}, true, project.Segmenter, 4),
                        new UnnaturalClass("G", new[] {"x", "ɣ", "χ"}, true, project.Segmenter, 5),
                        new UnnaturalClass("H", new[] {"ʔ", "ħ", "ʕ", "h", "ɦ"}, true, project.Segmenter, 6),
                        new UnnaturalClass("I", new[] {"i", "ɪ", "ɨ", "ɿ", "ʅ", "ɯ", "ĩ", "í", "ǐ", "ì", "î", "ī", "ı"}, true, project.Segmenter, 7),
                        new UnnaturalClass("J", new[] {"j", "ɥ", "ɰ"}, true, project.Segmenter, 8),
                        new UnnaturalClass("K", new[] {"k", "g", "q", "ɢ", "ɡ"}, true, project.Segmenter, 9),
                        new UnnaturalClass("L", new[] {"l", "ȴ", "l", "ɭ", "ʎ", "ʟ", "ɬ", "ɮ", "ł", "ɫ"}, true, project.Segmenter,10),
                        new UnnaturalClass("M", new[] {"m", "ɱ", "ʍ"}, true, project.Segmenter, 11),
                        new UnnaturalClass("N", new[] {"n", "ȵ", "ɳ", "ŋ", "ɴ", "ň", "ń", "ɲ"}, true, project.Segmenter, 12),
                        new UnnaturalClass("O", new[] {"Œ", "ɒ"}, true, project.Segmenter, 13),
                        new UnnaturalClass("P", new[] {"p", "b", "ɓ"}, true, project.Segmenter, 14),
                        new UnnaturalClass("R", new[] {"ɹ", "ɻ", "ʀ", "ɾ", "r", "ʁ", "ɽ", "ɐ̯"}, true, project.Segmenter, 15),
                        new UnnaturalClass("S", new[] {"s", "z", "ʃ", "ʒ", "ʂ", "ʐ", "ç", "ʝ", "š", "ž", "ɕ", "ɧ", "ʑ"}, true, project.Segmenter, 16),
                        new UnnaturalClass("T", new[] {"t", "d", "ȶ", "ȡ", "ɗ", "ʈ", "ɖ"}, true, project.Segmenter, 17),
                        new UnnaturalClass("U", new[] {"œ", "ɞ", "ɔ", "ø", "ɵ", "o", "õ", "ó", "ò", "ō", "ɶ", "ô", "ɷ"}, true, project.Segmenter, 18),
                        new UnnaturalClass("W", new[] {"w", "ʋ", "ⱱ"}, true, project.Segmenter, 19),
                        new UnnaturalClass("Y", new[] {"y", "ʏ", "ʉ", "u", "ᴜ", "ʊ", "ú", "ù"}, true, project.Segmenter, 20),
					};
			_soundClasses = soundClasses.ToList();

            // Create the scoring matrix
			_scoringMatrix = new int[][]
            {
                new int[] {500,-1000,-1000,-1000,400,-1000,-1000,400,-600,-1000,-1000,-1000,-1000,400,-1000,-1000,-1000,-1000,400,-600,400},
                new int[] {-1000,1000,0,0,-1000,0,0,-1000,0,0,0,0,0,-1000,600,0,0,0,-1000,600,-900},
                new int[] {-1000,0,1000,200,-1000,200,200,-1000,0,600,0,0,0,-1000,0,0,600,600,-1000,0,-1000},
                new int[] {-1000,0,200,1000,-1000,0,200,-1000,0,0,0,0,0,-1000,0,0,600,600,-1000,0,-1000},
                new int[] {400,-1000,-1000,-1000,500,-1000,-1000,400,-600,-1000,-1000,-1000,-1000,400,-1000,-1000,-1000,-1000,400,-600,400},
                new int[] {-1000,0,200,0,-1000,1000,200,-1000,0,600,0,0,0,-1000,0,0,600,0,-1000,0,-1000},
                new int[] {-1000,0,200,200,-1000,200,1000,-1000,0,0,0,0,0,-1000,0,0,600,0,-1000,0,-1000},
                new int[] {400,-1000,-1000,-1000,400,-1000,-1000,500,-500,-1000,-1000,-1000,-1000,400,-1000,-1000,-1000,-1000,300,-600,400},
                new int[] {-600,0,0,0,-600,0,0,-500,1000,0,0,0,0,-600,0,0,0,0,-600,0,-600},
                new int[] {-1000,0,600,0,-1000,600,0,-1000,0,1000,0,0,0,-1000,0,0,200,0,-1000,0,-1000},
                new int[] {-1000,0,0,0,-1000,0,0,-1000,0,0,1000,0,0,-1000,0,400,0,0,-1000,0,-1000},
                new int[] {-1000,0,0,0,-1000,0,0,-1000,0,0,0,1000,100,-1000,0,0,0,0,-1000,0,-1000},
                new int[] {-1000,0,0,0,-1000,0,0,-1000,0,0,0,100,1000,-1000,0,0,0,0,-1000,0,-1000},
                new int[] {400,-1000,-1000,-1000,400,-1000,-1000,400,-600,-1000,-1000,-1000,-1000,500,-1000,-1000,-1000,-1000,400,-600,400},
                new int[] {-1000,600,0,0,-1000,0,0,-1000,0,0,0,0,0,-1000,1000,0,0,0,-1000,200,-1000},
                new int[] {-1000,0,0,0,-1000,0,0,-1000,0,0,400,0,0,-1000,0,1000,0,0,-1000,0,-1000},
                new int[] {-1000,0,600,600,-1000,600,600,-1000,0,200,0,0,0,-1000,0,0,1000,200,-1000,0,-1000},
                new int[] {-1000,0,600,600,-1000,0,0,-1000,0,0,0,0,0,-1000,0,0,200,1000,-1000,0,-1000},
                new int[] {400,-1000,-1000,-1000,400,-1000,-1000,300,-600,-1000,-1000,-1000,-1000,400,-1000,-1000,-1000,-1000,500,-600,400},
                new int[] {-600,600,0,0,-600,0,0,-600,0,0,0,0,0,-600,200,0,0,0,-600,1000,-500},
                new int[] {400,-900,-1000,-1000,400,-1000,-1000,400,-600,-1000,-1000,-1000,-1000,400,-1000,-1000,-1000,-1000,400,-500,500}
			};
		}

		public IReadOnlySet<SymbolicFeature> RelevantVowelFeatures
		{
			get { return _relevantVowelFeatures; }
		}

		public IReadOnlySet<SymbolicFeature> RelevantConsonantFeatures
		{
			get { return _relevantConsFeatures; }
		} 

		public IReadOnlyDictionary<SymbolicFeature, int> FeatureWeights
		{
			get { return _featureWeights; }
		}

		public IReadOnlyDictionary<FeatureSymbol, int> ValueMetrics
		{
			get { return _valueMetrics; }
		}

		public int GetGapPenalty(Word sequence1, Word sequence2)
		{
			// Completed, lingpy has -10.00 in its SCA matrix for this (_ row), so scale lingpy numbers by 100X
			return -IndelCost;
		}

		public int GetInsertionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
		{
			// Unused by SCA alignment
			return 0;
		}

		public int GetDeletionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
		{
			// Unused by SCA alignment
			return 0;
		}

		//TODO
		public int GetSubstitutionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q)
		{
            // TODO: how to get number from shape node?
			//UnnaturalClass soundClass1 = FindSoundClass(p);
			//UnnaturalClass soundClass2 = FindSoundClass(q);
			//int index1 = soundClass1.Index();
			//int index2 = soundClass2.Index();
            //return _scoringMatrix[index1][index2];
			return -1;
		}

		public int GetExpansionScore(Word sequence1, ShapeNode p, Word sequence2, ShapeNode q1, ShapeNode q2)
		{
			// Unused by SCA alignment
			return 0;
		}

		public int GetCompressionScore(Word sequence1, ShapeNode p1, ShapeNode p2, Word sequence2, ShapeNode q)
		{
			// Unused by SCA alignment
			return 0;
		}

        // TODO: what should the max scores be here?
		public int GetMaxScore1(Word sequence1, ShapeNode p, Word sequence2)
		{
			return GetMaxScore(p);
		}

		// TODO
		public int GetMaxScore2(Word sequence1, Word sequence2, ShapeNode q)
		{
			return GetMaxScore(q);
		}

		// TODO
		private int GetMaxScore(ShapeNode node)
		{
			return MaxSubstitutionScore;
		}
	}
}
