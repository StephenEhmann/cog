using SIL.Cog.Applications.Services;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Applications.ViewModels
{
	public class EMSoundChangeInducerViewModel : ComponentSettingsViewModelBase
	{
		private readonly IProjectService _projectService;
		private double _initialAlignmentThreshold;

		public EMSoundChangeInducerViewModel(IProjectService projectService)
			: base("Sound correspondence induction")
		{
			_projectService = projectService;
		}

		public override void Setup()
		{
			var soundChangeInducer = (EMSoundChangeInducer) _projectService.Project.VarietyPairProcessors["soundChangeInducer"];
			Set(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, soundChangeInducer.InitialAlignmentThreshold);
		}

		public double InitialAlignmentThreshold
		{
			get { return _initialAlignmentThreshold; }
			set { SetChanged(() => InitialAlignmentThreshold, ref _initialAlignmentThreshold, value); }
		}

		public override object UpdateComponent()
		{
			var soundChangeInducer = new EMSoundChangeInducer(_projectService.Project, _initialAlignmentThreshold, "primary", "cognateIdentifier");
			_projectService.Project.VarietyPairProcessors["soundChangeInducer"] = soundChangeInducer;
			return soundChangeInducer;
		}
	}
}
