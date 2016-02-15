using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class AlignmentOptionsViewModel : ComponentOptionsViewModel
	{
        private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;

        public AlignmentOptionsViewModel(SegmentPool segmentPool, IProjectService projectService, AlineViewModel alineAlignment, SCAAlignViewModel SCAAlignment)
            : base("Alignment", "Method", alineAlignment, SCAAlignment)
		{
            _segmentPool = segmentPool;
			_projectService = projectService;
		}

		public override void Setup()
		{
            IWordAligner aligner = _projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
			int index = -1;
            if (aligner is Aline)
				index = 0;
            else if (aligner is SCAAlign)
				index = 1;
			SelectedOption = Options[index];

			base.Setup();
		}

		public override object UpdateComponent()
		{
            var oldWordPairGenerator = (CognacyWordPairGenerator) _projectService.Project.VarietyPairProcessors[ComponentIdentifiers.WordPairGenerator];
            var wordPairGenerator = new CognacyWordPairGenerator(_segmentPool, _projectService.Project, oldWordPairGenerator.InitialAlignmentThreshold,
                ComponentIdentifiers.PrimaryWordAligner, ComponentIdentifiers.PrimaryCognateIdentifier);
			_projectService.Project.VarietyPairProcessors[ComponentIdentifiers.WordPairGenerator] = wordPairGenerator;
			return base.UpdateComponent();
		}
	}
}
