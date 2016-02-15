using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class LexStatOptionsViewModel : ComponentOptionsViewModel
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private double _initialAlignmentThreshold;

		public LexStatOptionsViewModel(SegmentPool segmentPool, IProjectService projectService, DolgopolskyCognateIdentifierViewModel dolgopolskyCognateIdentifier, SCACognateIdentifierViewModel scaCognateIdentifier)
            : base("LexStat", "Sound Class", dolgopolskyCognateIdentifier, scaCognateIdentifier)
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
		}

		public override void Setup()
		{
			ICognateIdentifier cognateIdentifier = _projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier];
			int index = -1;
		    if (cognateIdentifier is DolgopolskyCognateIdentifier)
				index = 0;
            else if (cognateIdentifier is SCACognateIdentifier)
                index = 1;
			else
				// For the case where the config has some cognate identifier associated with EMSoundChangeInducer which is not valid for LexStat
				index = 1;
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

            var statisticalProcessor = new StatisticalProcessor(_segmentPool, _projectService.Project,
				ComponentIdentifiers.PrimaryStatisticalMethod, ComponentIdentifiers.PrimaryWordAligner, ComponentIdentifiers.PrimaryCognateIdentifier);
            _projectService.Project.VarietyPairProcessors[ComponentIdentifiers.StatisticalProcessor] = statisticalProcessor;

			return base.UpdateComponent();
		}
	}
}
