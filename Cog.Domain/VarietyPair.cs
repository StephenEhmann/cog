using System.Collections.Generic;
using SIL.Collections;
using SIL.Machine.FeatureModel;
using SIL.Machine.NgramModeling;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain
{
	public class VarietyPair : ObservableObject
	{
		private readonly Variety _variety1;
		private readonly Variety _variety2;
		private readonly WordPairCollection _wordPairs; 
		private IConditionalProbabilityDistribution<SoundContext, Ngram<Segment>> _soundChangeProbabilityDistribution;
		private ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> _soundFreqDist;
		private double _defaultCorrProb;
		private double _phoneticSimilarityScore;
		private double _lexicalSimilarityScore;
		private double _significance;
		private double _precision;
		private double _recall;
		private readonly ReadOnlyDictionary<FeatureSymbol, SoundCorrespondenceCollection> _soundCorrespondenceCollections;

		public VarietyPair(Variety variety1, Variety variety2)
		{
			_variety1 = variety1;
			_variety2 = variety2;
			_wordPairs = new WordPairCollection(this);

			_soundCorrespondenceCollections = new ReadOnlyDictionary<FeatureSymbol, SoundCorrespondenceCollection>(new Dictionary<FeatureSymbol, SoundCorrespondenceCollection>
				{
					{CogFeatureSystem.Onset, new SoundCorrespondenceCollection()},
					{CogFeatureSystem.Nucleus, new SoundCorrespondenceCollection()},
					{CogFeatureSystem.Coda, new SoundCorrespondenceCollection()}
				});
		}

		public Variety Variety1
		{
			get { return _variety1; }
		}

		public Variety Variety2
		{
			get { return _variety2; }
		}

		public WordPairCollection WordPairs
		{
			get { return _wordPairs; }
		}

		public Variety GetOtherVariety(Variety variety)
		{
			Variety otherVariety = null;
			if (_variety1 == variety)
				otherVariety = _variety2;
			else if (_variety2 == variety)
				otherVariety = _variety1;
			return otherVariety;
		}

		public double PhoneticSimilarityScore
		{
			get { return _phoneticSimilarityScore; }
			set { Set(() => PhoneticSimilarityScore, ref _phoneticSimilarityScore, value); }
		}

		public double LexicalSimilarityScore
		{
			get { return _lexicalSimilarityScore; }
			set { Set(() => LexicalSimilarityScore, ref _lexicalSimilarityScore, value); }
		}

		public double Significance
		{
			get { return _significance; }
			set { Set(() => Significance, ref _significance, value); }
		}

		public double Precision
		{
			get { return _precision; }
			set { Set(() => Precision, ref _precision, value); }
		}

		public double Recall
		{
			get { return _recall; }
			set { Set(() => Recall, ref _recall, value); }
		}

		public ReadOnlyDictionary<FeatureSymbol, SoundCorrespondenceCollection> SoundCorrespondenceCollections
		{
			get { return _soundCorrespondenceCollections; }
		}

		public IConditionalProbabilityDistribution<SoundContext, Ngram<Segment>> SoundChangeProbabilityDistribution
		{
			get { return _soundChangeProbabilityDistribution; }
			set { Set(() => SoundChangeProbabilityDistribution, ref _soundChangeProbabilityDistribution, value); }
		}

		public ConditionalFrequencyDistribution<SoundContext, Ngram<Segment>> SoundChangeFrequencyDistribution
		{
			get { return _soundFreqDist; }
			set { Set(() => SoundChangeFrequencyDistribution, ref _soundFreqDist, value); }
		}

		public double DefaultCorrespondenceProbability
		{
			get { return _defaultCorrProb; }
			set { Set(() => DefaultCorrespondenceProbability, ref _defaultCorrProb, value); }
		}
	}
}
