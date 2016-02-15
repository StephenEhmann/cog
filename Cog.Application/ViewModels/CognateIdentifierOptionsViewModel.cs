using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class CognateIdentifierOptionsViewModel : ComponentOptionsViewModel
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private double _initialAlignmentThreshold;

        public CognateIdentifierOptionsViewModel(SegmentPool segmentPool, IProjectService projectService, EMSoundChangeInducerOptionsViewModel em,
            LexStatOptionsViewModel lexStat)
			: base("Cognacy", "Method", em, lexStat)
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
		}

		public double InitialAlignmentThreshold
		{
			get { return _initialAlignmentThreshold; }
			set { SetChanged(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, value); }
		}

		public override void Setup()
		{
			IProcessor<VarietyPair> statisticalMethod = _projectService.Project.StatisticalMethods[ComponentIdentifiers.PrimaryStatisticalMethod];
			int index = -1;
            if (statisticalMethod is EMSoundChangeInducer)
				index = 0;
            else if (statisticalMethod is LexStat)
				index = 1;
			SelectedOption = Options[index];

			var wordPairGenerator = (CognacyWordPairGenerator)_projectService.Project.VarietyPairProcessors[ComponentIdentifiers.WordPairGenerator];
			Set(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, wordPairGenerator.InitialAlignmentThreshold);
			base.Setup();
		}

		public override object UpdateComponent()
		{
			var wordPairGenerator = new CognacyWordPairGenerator(_segmentPool, _projectService.Project, _initialAlignmentThreshold,
				ComponentIdentifiers.PrimaryWordAligner, ComponentIdentifiers.PrimaryCognateIdentifier);
			_projectService.Project.VarietyPairProcessors[ComponentIdentifiers.WordPairGenerator] = wordPairGenerator;
			return base.UpdateComponent();
		}
	}
}
