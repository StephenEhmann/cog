/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Cog"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Autofac;
using SIL.Cog.Applications.Services;
using SIL.Cog.Applications.ViewModels;
using SIL.Cog.Presentation.Services;
using SIL.Machine;

namespace SIL.Cog.Presentation
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
	    private readonly IContainer _container;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
			var builder = new ContainerBuilder();

			////if (ViewModelBase.IsInDesignModeStatic)
			////{
			////    // Create design time view services and models
			////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
			////}
			////else
			////{
			////    // Create run time view services and models
			////    SimpleIoc.Default.Register<IDataService, DataService>();
			////}
			builder.RegisterType<AnalysisService>().As<IAnalysisService>().SingleInstance();
			builder.RegisterType<ProjectService>().As<IProjectService>().SingleInstance();
			builder.RegisterType<WindowViewModelMappings>().As<IWindowViewModelMappings>().SingleInstance();
			builder.RegisterType<DialogService>().As<IDialogService>().SingleInstance();
			builder.RegisterType<BusyService>().As<IBusyService>().SingleInstance();
			builder.RegisterType<ShapeSpanFactory>().As<SpanFactory<ShapeNode>>().SingleInstance();
			builder.RegisterType<ExportService>().As<IExportService>().SingleInstance();
			builder.RegisterType<ImportService>().As<IImportService>().SingleInstance();
			builder.RegisterType<ImageExportService>().As<IImageExportService>().SingleInstance();
			builder.RegisterType<SettingsService>().As<ISettingsService>().SingleInstance();

			builder.RegisterType<MainWindowViewModel>().SingleInstance();
			builder.RegisterType<InputMasterViewModel>().SingleInstance();
			builder.RegisterType<CompareMasterViewModel>().SingleInstance();
			builder.RegisterType<AnalyzeMasterViewModel>().SingleInstance();
			builder.RegisterType<WordListsViewModel>().SingleInstance();
			builder.RegisterType<VarietiesViewModel>().SingleInstance();
			builder.RegisterType<SensesViewModel>().SingleInstance();
			builder.RegisterType<InputSettingsViewModel>().SingleInstance();
			builder.RegisterType<SimilarityMatrixViewModel>().SingleInstance();
			builder.RegisterType<VarietyPairsViewModel>().SingleInstance();
			builder.RegisterType<CompareSettingsViewModel>().SingleInstance();
			builder.RegisterType<HierarchicalGraphViewModel>().SingleInstance();
			builder.RegisterType<NetworkGraphViewModel>().SingleInstance();
			builder.RegisterType<GeographicalViewModel>().SingleInstance();
			builder.RegisterType<GlobalCorrespondencesViewModel>().SingleInstance();
			builder.RegisterType<MultipleWordAlignmentViewModel>().SingleInstance();

	        _container = builder.Build();
        }

        public MainWindowViewModel Main
        {
            get { return _container.Resolve<MainWindowViewModel>(); }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}