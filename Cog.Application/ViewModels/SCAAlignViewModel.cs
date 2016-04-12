using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;
using SIL.Collections;
using SIL.Machine.FeatureModel;
using SIL.Machine.SequenceAlignment;

namespace SIL.Cog.Application.ViewModels
{
	public enum SCAAlignMode
	{
		[Description("Full (global)")]
		Global,
		[Description("Partial (local)")]
		Local,
        [Description("Beginning (half-local)")]
        HalfLocal,
        [Description("Hybrid (semi-global)")]
        SemiGlobal
	}

	public class SCAAlignViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private SCAAlignMode _mode;
		private ReadOnlyList<RelevantFeatureViewModel> _features;
		private readonly SoundClassesViewModel _soundClasses;

		public SCAAlignViewModel(SegmentPool segmentPool, IProjectService projectService, SoundClassesViewModel soundClasses)
			: base("SCA")
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
			_soundClasses = soundClasses;
			_soundClasses.PropertyChanged += ChildPropertyChanged;
		}

		public override void Setup()
		{
            _soundClasses.SelectedSoundClass = null;
            _soundClasses.SoundClasses.Clear();

            IWordAligner aligner = _projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner];
            var sca = aligner as SCAAlign;
            if (sca == null)
            {
                Set(() => Features, ref _features, null);
            }
            else
            {
                var features = new List<RelevantFeatureViewModel>();
			    foreach (KeyValuePair<SymbolicFeature, int> kvp in sca.FeatureWeights)
			    {
				    var vm = new RelevantFeatureViewModel(kvp.Key, kvp.Value, sca.RelevantVowelFeatures.Contains(kvp.Key), sca.RelevantConsonantFeatures.Contains(kvp.Key), sca.ValueMetrics);
				    vm.PropertyChanged += ChildPropertyChanged;
				    features.Add(vm);
			    }
			    Set(() => Features, ref _features, new ReadOnlyList<RelevantFeatureViewModel>(features));
			    switch (sca.Settings.Mode)
			    {
				    case AlignmentMode.Local:
					    Set(() => Mode, ref _mode, SCAAlignMode.Local);
					    break;
				    case AlignmentMode.Global:
					    Set(() => Mode, ref _mode, SCAAlignMode.Global);
					    break;
                    case AlignmentMode.SemiGlobal:
                        Set(() => Mode, ref _mode, SCAAlignMode.SemiGlobal);
					    break;
                    case AlignmentMode.HalfLocal:
                        Set(() => Mode, ref _mode, SCAAlignMode.HalfLocal);
					    break;
			    }

			    foreach (SoundClass soundClass in sca.ContextualSoundClasses)
				    _soundClasses.SoundClasses.Add(new SoundClassViewModel(soundClass));
            }
		}

		public SCAAlignMode Mode
		{
			get { return _mode; }
			set { SetChanged(() => Mode, ref _mode, value); }
		}

		public ReadOnlyList<RelevantFeatureViewModel> Features
		{
			get { return _features; }
		}

		public SoundClassesViewModel SoundClasses
		{
			get { return _soundClasses; }
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_soundClasses.AcceptChanges();
			ChildrenAcceptChanges(_features);
		}

		public override object UpdateComponent()
		{
			var mode = AlignmentMode.Local;
			switch (_mode)
			{
				case SCAAlignMode.Local:
					mode = AlignmentMode.Local;
					break;
				case SCAAlignMode.Global:
					mode = AlignmentMode.Global;
					break;
                case SCAAlignMode.SemiGlobal:
                    mode = AlignmentMode.SemiGlobal;
					break;
                case SCAAlignMode.HalfLocal:
                    mode = AlignmentMode.HalfLocal;
					break;
			}

            if (_features == null)
            {
                var features = new List<RelevantFeatureViewModel>();
                Set(() => Features, ref _features, new ReadOnlyList<RelevantFeatureViewModel>(features));
            }

			var relevantVowelFeatures = new List<SymbolicFeature>();
			var relevantConsFeatures = new List<SymbolicFeature>();
			var featureWeights = new Dictionary<SymbolicFeature, int>();
			var valueMetrics = new Dictionary<FeatureSymbol, int>();
			foreach (RelevantFeatureViewModel feature in _features)
			{
				if (feature.Vowel)
					relevantVowelFeatures.Add(feature.DomainFeature);
				if (feature.Consonant)
					relevantConsFeatures.Add(feature.DomainFeature);
				featureWeights[feature.DomainFeature] = feature.Weight;
				foreach (RelevantValueViewModel value in feature.Values)
					valueMetrics[value.DomainSymbol] = value.Metric;
			}

			var aligner = new SCAAlign(_segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics,
				new SCAAlignSettings
				{
					ExpansionCompressionEnabled = false,
					Mode = mode,
					ContextualSoundClasses = _soundClasses.SoundClasses.Select(nc => nc.DomainSoundClass),
				});
			_projectService.Project.WordAligners[ComponentIdentifiers.PrimaryWordAligner] = aligner;
			return aligner;
		}
	}
}
