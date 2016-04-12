using System.Collections.Generic;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Domain.Components
{
	public class SCAAlign : WordAlignerBase
	{
		private readonly SCAAlignScorer _scorer;
		private readonly SCAAlignSettings _settings;

		public SCAAlign(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics)
			: this(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, new SCAAlignSettings())
		{
		}

		public SCAAlign(SegmentPool segmentPool, IEnumerable<SymbolicFeature> relevantVowelFeatures, IEnumerable<SymbolicFeature> relevantConsFeatures,
			IDictionary<SymbolicFeature, int> featureWeights, IDictionary<FeatureSymbol, int> valueMetrics, SCAAlignSettings settings)
			: base(settings)
		{
			_settings = settings;
			_scorer = new SCAAlignScorer(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				settings.ContextualSoundClasses);
		}

		public IReadOnlySet<SymbolicFeature> RelevantVowelFeatures
		{
			get { return _scorer.RelevantVowelFeatures; }
		}

		public IReadOnlySet<SymbolicFeature> RelevantConsonantFeatures
		{
			get { return _scorer.RelevantConsonantFeatures; }
		} 

		public IReadOnlyDictionary<SymbolicFeature, int> FeatureWeights
		{
			get { return _scorer.FeatureWeights; }
		}

		public IReadOnlyDictionary<FeatureSymbol, int> ValueMetrics
		{
			get { return _scorer.ValueMetrics; }
		}

		public SCAAlignSettings Settings
		{
			get { return _settings; }
		}

		protected override IPairwiseAlignmentScorer<Word, ShapeNode> Scorer
		{
			get { return _scorer; }
		}

		public override int Delta(FeatureStruct fs1, FeatureStruct fs2)
		{
			return _scorer.Delta(fs1, fs2);
		}
	}
}
