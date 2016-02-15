using SIL.Cog.Application.Services;

namespace SIL.Cog.Application.ViewModels
{
	public class CompareSettingsViewModel : SettingsWorkspaceViewModelBase
	{
		public CompareSettingsViewModel(IProjectService projectService, IBusyService busyService, AlignmentOptionsViewModel alignmentOptionsViewModel,
			CognateIdentifierOptionsViewModel cognateIdentifierOptionsViewModel)
			: base("Comparison", projectService, busyService, alignmentOptionsViewModel, cognateIdentifierOptionsViewModel)
		{
		}
	}
}
