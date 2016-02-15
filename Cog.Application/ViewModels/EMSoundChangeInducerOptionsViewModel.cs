using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class EMSoundChangeInducerOptionsViewModel : ComponentOptionsViewModel
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private double _initialAlignmentThreshold;

		public EMSoundChangeInducerOptionsViewModel(SegmentPool segmentPool, IProjectService projectService, BlairCognateIdentifierViewModel blairCognateIdentifier,
            ThresholdCognateIdentifierViewModel thresholdCognateIdentifier, DolgopolskyCognateIdentifierViewModel dolgopolskyCognateIdentifier, SCACognateIdentifierViewModel scaCognateIdentifier)
            : base("EM Sound Change Inducer", "Cognate Identifier Method", blairCognateIdentifier, thresholdCognateIdentifier, dolgopolskyCognateIdentifier, scaCognateIdentifier)
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
		}

		public override void Setup()
		{
			ICognateIdentifier cognateIdentifier = _projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier];
			int index = -1;
			if (cognateIdentifier is BlairCognateIdentifier)
				index = 0;
			else if (cognateIdentifier is ThresholdCognateIdentifier)
				index = 1;
			else if (cognateIdentifier is DolgopolskyCognateIdentifier)
				index = 2;
            else if (cognateIdentifier is SCACognateIdentifier)
                index = 3;
			SelectedOption = Options[index];

			var wordPairGenerator = (CognacyWordPairGenerator) _projectService.Project.VarietyPairProcessors[ComponentIdentifiers.WordPairGenerator];
			Set(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, wordPairGenerator.InitialAlignmentThreshold);

			base.Setup();
		}

		public double InitialAlignmentThreshold
		{
			get { return _initialAlignmentThreshold; }
			set { SetChanged(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, value); }
		}

		public override object UpdateComponent()
		{
			var wordPairGenerator = new CognacyWordPairGenerator(_segmentPool, _projectService.Project, _initialAlignmentThreshold,
				ComponentIdentifiers.PrimaryWordAligner, ComponentIdentifiers.PrimaryCognateIdentifier);
			_projectService.Project.VarietyPairProcessors[ComponentIdentifiers.WordPairGenerator] = wordPairGenerator;

            var statisticalProcessor = new EMSoundChangeInducer(_segmentPool, _projectService.Project,
                ComponentIdentifiers.PrimaryWordAligner, ComponentIdentifiers.PrimaryCognateIdentifier);
            _projectService.Project.VarietyPairProcessors[ComponentIdentifiers.StatisticalProcessor] = statisticalProcessor;

			return base.UpdateComponent();
		}
	}
}
