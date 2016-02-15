﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using GalaSoft.MvvmLight.Messaging;
using ProtoBuf;
using SIL.Cog.Application.ViewModels;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Config;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.Services
{
	public class ProjectService : IProjectService
	{
		private const int CacheVersion = 3;
		private const int ProjectVersion = 3;

		private static readonly FileType CogProjectFileType = new FileType("Cog Project", ".cogx");
		private static readonly List<IProjectMigration> ProjectMigrations;

		static ProjectService()
		{
			ProjectMigrations = Assembly.GetExecutingAssembly().GetTypes()
				.Where(t => t.GetInterfaces().Contains(typeof(IProjectMigration)))
				.Select(t => (IProjectMigration) Activator.CreateInstance(t)).OrderBy(cm => cm.Version).ToList();
		}

		public event EventHandler<EventArgs> ProjectOpened;

		private readonly SpanFactory<ShapeNode> _spanFactory;
		private readonly SegmentPool _segmentPool;
		private readonly IDialogService _dialogService;
		private readonly IBusyService _busyService;
		private readonly ISettingsService _settingsService;
		private readonly Lazy<IAnalysisService> _analysisService;

		private CogProject _project;
		private string _projectName;
		private bool _isChanged;
		private bool _isNew;
		private FileStream _projectFileStream;

		public ProjectService(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, IDialogService dialogService, IBusyService busyService,
			ISettingsService settingsService, Lazy<IAnalysisService> analysisService)
		{
			_spanFactory = spanFactory;
			_segmentPool = segmentPool;
			_dialogService = dialogService;
			_busyService = busyService;
			_settingsService = settingsService;
			_analysisService = analysisService;

			Messenger.Default.Register<DomainModelChangedMessage>(this, HandleDomainModelChanged);
			Messenger.Default.Register<ComparisonPerformedMessage>(this, HandleComparisonPerformed);
		}

		private void HandleDomainModelChanged(DomainModelChangedMessage msg)
		{
			_isChanged = true;
			if (msg.AffectsComparison)
				_project.VarietyPairs.Clear();
		}

		private void HandleComparisonPerformed(ComparisonPerformedMessage msg)
		{
			if (_projectFileStream != null && !_isChanged)
				SaveComparisonCache();
		}

		public bool Init()
		{
			string projectPath = _settingsService.LastProject;
			bool usingCmdLineArg = false;
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length > 1)
			{
				projectPath = args[1];
				usingCmdLineArg = true;
			}

			string errorMsg = null;
			var progressVM = new ProgressViewModel(vm =>
				{
					if (string.IsNullOrEmpty(projectPath) || projectPath == "<new>" || !File.Exists(projectPath))
					{
						NewProject(vm);
					}
					else
					{
						if (!OpenProject(vm, projectPath, out errorMsg))
						{
							if (usingCmdLineArg)
								vm.Canceled = true;
							else
								NewProject(vm);
						}
					}
				}, true, false);

			if (_dialogService.ShowModalDialog(null, progressVM) == false)
			{
				_dialogService.ShowError(null, errorMsg, "Cog");
				return false;
			}

			return true;
		}

		public bool New(object ownerViewModel)
		{
			if (_isNew && !_isChanged)
			{
				var progressVM = new ProgressViewModel(NewProject, true, false);
				_dialogService.ShowModalDialog(ownerViewModel, progressVM);
				return true;
			}

			StartNewProcess("<new>", 5000);
			return false;
		}

		private void StartNewProcess(string projectPath, int timeout)
		{
			_busyService.ShowBusyIndicator(() =>
				{
					Process process = Process.Start(Assembly.GetEntryAssembly().Location, string.Format("\"{0}\"", projectPath));
					Debug.Assert(process != null);
					var stopwatch = new Stopwatch();
					stopwatch.Start();
					while (process.MainWindowHandle == IntPtr.Zero && stopwatch.ElapsedMilliseconds < timeout)
					{
						Thread.Sleep(100);
						process.Refresh();
					}
					stopwatch.Stop();
				});
		}

		private void NewProject(ProgressViewModel vm)
		{
			vm.DisplayName = "Opening New Project - Cog";
			vm.Text = "Loading project file...";
			Stream stream = Assembly.GetAssembly(GetType()).GetManifestResourceStream("SIL.Cog.Application.NewProject.cogx");
			CogProject project = ConfigManager.Load(_spanFactory, _segmentPool, stream);
			SetupProject(vm, null, "New Project", project);
			_isNew = true;
		}

		public bool Open(object ownerViewModel)
		{
			if (_isNew && !_isChanged)
			{
				FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, CogProjectFileType);
				if (result.IsValid && result.FileName != _settingsService.LastProject)
				{
					string errorMsg = null;
					var progressVM = new ProgressViewModel(vm =>
						{
							if (!OpenProject(vm, result.FileName, out errorMsg))
								vm.Canceled = true;
						}, true, false);
					if (_dialogService.ShowModalDialog(ownerViewModel, progressVM) == false)
					{
						_dialogService.ShowError(ownerViewModel, errorMsg, "Cog");
						return false;
					}
					return true;
				}
			}
			else
			{
				FileDialogResult result = _dialogService.ShowOpenFileDialog(ownerViewModel, CogProjectFileType);
				if (result.IsValid && result.FileName != _settingsService.LastProject)
					StartNewProcess(result.FileName, 5000);
			}

			return false;
		}

		public bool Close(object ownerViewModel)
		{
			if (IsChanged)
			{
				bool? res = _dialogService.ShowQuestion(ownerViewModel, "Do you want to save the changes to this project?", "Cog");
				if (res == true)
					Save(ownerViewModel);
				else if (res == null)
					return false;
			}
			CloseProject();
			return true;
		}

		private void CloseProject()
		{
			if (_projectFileStream != null)
			{
				_projectFileStream.Close();
				_projectFileStream = null;
			}
			_project = null;
			_projectName = null;
			_isChanged = false;
		}

		private bool OpenProject(ProgressViewModel vm, string path, out string errorMsg)
		{
			string projectName = Path.GetFileNameWithoutExtension(path);
			vm.DisplayName = string.Format("Opening {0} - Cog", projectName);
			vm.Text = "Loading project file...";
			errorMsg = null;
			FileStream fileStream = null;
			CogProject project = null;
			try
			{
				fileStream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
				if (ConfigManager.Load(_spanFactory, _segmentPool, fileStream, out project))
					_isChanged = true;
			}
			catch (ConfigException)
			{
				errorMsg = "The specified file is not a valid Cog configuration file.";
			}
			catch (IOException ioe)
			{
				errorMsg = string.Format("Error opening project file:\n{0}", ioe.Message);
			}
			catch (UnauthorizedAccessException)
			{
				errorMsg = "The specified file is in use or you do not have access to it.";
			}
			if (errorMsg != null)
			{
				if (fileStream != null)
					fileStream.Close();
				return false;
			}
			Debug.Assert(project != null);

			_projectFileStream = fileStream;
			SetupProject(vm, path, projectName, project);
			_isNew = false;
			return true;
		}

		private bool MigrateProjectIfNeeded(ProgressViewModel vm, CogProject project)
		{
			if (project.Version == ProjectVersion)
				return false;

			vm.Text = "Updating project...";
			foreach (IProjectMigration pm in ProjectMigrations.SkipWhile(cm => project.Version >= cm.Version))
				pm.Migrate(_segmentPool, project);
			project.Version = ProjectVersion;
			return true;
		}

		public bool Save(object ownerViewModel)
		{
			if (_projectFileStream == null)
			{
				FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, CogProjectFileType);
				if (result.IsValid)
				{
					SaveAsProject(result.FileName);
					return true;
				}

				return false;
			}

			SaveProject();
			return true;
		}

		public bool SaveAs(object ownerViewModel)
		{
			FileDialogResult result = _dialogService.ShowSaveFileDialog(ownerViewModel, CogProjectFileType);
			if (result.IsValid)
			{
				SaveAsProject(result.FileName);
				return true;
			}

			return false;
		}

		private void SaveAsProject(string path)
		{
			if (_projectFileStream != null)
				_projectFileStream.Close();
			_projectFileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
			SaveProject();
			_settingsService.LastProject = path;
			_projectName = Path.GetFileNameWithoutExtension(path);
		}

		private void SaveProject()
		{
			_projectFileStream.Seek(0, SeekOrigin.Begin);
			ConfigManager.Save(_project, _projectFileStream);
			_projectFileStream.Flush();
			_projectFileStream.SetLength(_projectFileStream.Position);
			SaveComparisonCache();
			_isChanged = false;
			_isNew = false;
		}

		private void SetupProject(ProgressViewModel vm, string path, string name, CogProject project)
		{
            Console.WriteLine("Setup Project");
			_settingsService.LastProject = path;
			if (MigrateProjectIfNeeded(vm, project))
				_isChanged = true;
			_project = project;
			_projectName = name;

			if (vm != null)
				vm.Text = "Segmenting and syllabifying words...";
			_analysisService.Value.SegmentAll();

			if (path != null)
			{
				string cacheFileName = Path.Combine(Path.GetTempPath(), Path.GetFileName(path) + ".cache");
				if (File.Exists(cacheFileName))
				{
                    Console.WriteLine("using cached file " + cacheFileName);
					if (vm != null)
						vm.Text = "Loading cached results...";
					bool delete = true;
					if (!_isChanged)
					{
						try
						{
							using (FileStream fs = File.OpenRead(cacheFileName))
							{
								var version = Serializer.DeserializeWithLengthPrefix<int>(fs, PrefixStyle.Base128, 1);
								if (version == CacheVersion)
								{
									var hash = Serializer.DeserializeWithLengthPrefix<string>(fs, PrefixStyle.Base128, 1);
									if (hash == CalcProjectHash())
									{
										_project.VarietyPairs.AddRange(Serializer.DeserializeItems<VarietyPairSurrogate>(fs, PrefixStyle.Base128, 1)
											.Select(surrogate => surrogate.ToVarietyPair(_segmentPool, _project)));
										delete = false;
									}
								}
							}
						}
						catch (Exception)
						{
							// could not load the cache, so delete it
						}
					}
					if (delete)
						File.Delete(cacheFileName);
				}
			}

			if (vm != null)
				vm.Text = "Initializing views...";
			OnProjectOpened(new EventArgs());
		}

		private void SaveComparisonCache()
		{
			string cacheFileName = Path.Combine(Path.GetTempPath(), Path.GetFileName(_settingsService.LastProject) + ".cache");
			if (AreAllVarietiesCompared)
			{
				using (FileStream fs = File.Create(cacheFileName))
				{
					Serializer.SerializeWithLengthPrefix(fs, CacheVersion, PrefixStyle.Base128, 1);
					Serializer.SerializeWithLengthPrefix(fs, CalcProjectHash(), PrefixStyle.Base128, 1);
					foreach (VarietyPair vp in _project.VarietyPairs)
					{
						var surrogate = new VarietyPairSurrogate(vp);
						Serializer.SerializeWithLengthPrefix(fs, surrogate, PrefixStyle.Base128, 1);
					}
				}
			}
			else
			{
				if (File.Exists(cacheFileName))
					File.Delete(cacheFileName);
			}
		}

		private string CalcProjectHash()
		{
			using (var md5 = MD5.Create())
			{
				_projectFileStream.Seek(0, SeekOrigin.Begin);
				return BitConverter.ToString(md5.ComputeHash(_projectFileStream)).Replace("-","").ToLower();
			}
		}

		private void OnProjectOpened(EventArgs e)
		{
			if (ProjectOpened != null)
				ProjectOpened(this, e);
		}

		public bool IsChanged
		{
			get { return _isChanged; }
		}

		public CogProject Project
		{
			get { return _project; }
		}

		public string ProjectName
		{
			get { return _projectName; }
		}

		public bool AreAllVarietiesCompared
		{
			get { return _project.Varieties.Count > 0 && _project.VarietyPairs.Count == ((_project.Varieties.Count - 1) * _project.Varieties.Count) / 2; }
		}
	}
}
