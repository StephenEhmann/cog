using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class LexStatCognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private int _initialEquivalenceThreshold;
		private readonly SoundClassesViewModel _soundClasses;

		public LexStatCognateIdentifierViewModel(SegmentPool segmentPool, IProjectService projectService, SoundClassesViewModel soundClassesViewModel)
			: base("LexStat")
		{
			_segmentPool = segmentPool;
			_projectService = projectService;
			_soundClasses = soundClassesViewModel;
			_soundClasses.PropertyChanged += ChildPropertyChanged;
		}

		public int InitialEquivalenceThreshold
		{
			get { return _initialEquivalenceThreshold; }
			set { SetChanged(() => InitialEquivalenceThreshold, ref _initialEquivalenceThreshold, value); }
		}

		public override void AcceptChanges()
		{
			base.AcceptChanges();
			_soundClasses.AcceptChanges();
		}

		public SoundClassesViewModel SoundClasses
		{
			get { return _soundClasses; }
		}

		public override void Setup()
		{
			CogProject project = _projectService.Project;
			ICognateIdentifier cognateIdentifier = project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier];
			var lexStat = cognateIdentifier as LexStatCognateIdentifier;

			IEnumerable<SoundClass> soundClasses;
			if (lexStat == null)
			{
				Set(() => InitialEquivalenceThreshold, ref _initialEquivalenceThreshold, 2);
				soundClasses = new SoundClass[]
					{
						new UnnaturalClass("K", new[] {"t͡s", "d͡z", "t͡ɕ", "d͡ʑ", "t͡ʃ", "d͡ʒ", "c", "ɟ", "t͡θ", "t͡ʂ", "d͡ʐ", "k", "g", "q", "ɢ", "ɡ", "ɠ", "x", "ɣ", "χ"}, true, project.Segmenter),
						new UnnaturalClass("P", new[] {"ɸ", "β", "f", "p͡f", "p", "b", "ɓ"}, true, project.Segmenter),
						new UnnaturalClass("Ø", new[] {"ʔ", "ħ", "ʕ", "h", "ɦ", "-", "#ŋ"}, true, project.Segmenter),
						new UnnaturalClass("J", new[] {"j", "ɥ", "ɰ"}, true, project.Segmenter),
						new UnnaturalClass("M", new[] {"m", "ɱ", "ʍ"}, true, project.Segmenter),
 						new UnnaturalClass("N", new[] {"n", "ɳ", "ŋ", "ɴ", "ɲ"}, true, project.Segmenter),
 						new UnnaturalClass("S", new[] {"s", "z", "ʃ", "ʒ", "ʂ", "ʐ", "ç", "ʝ", "ɕ", "ʑ", "ɧ"}, true, project.Segmenter),
						new UnnaturalClass("R", new[] {"ɹ", "ɻ", "ʀ", "ɾ", "r", "ʁ", "ɽ", "l", "ɭ", "ʎ", "ʟ", "ɬ", "ɮ", "ɫ", "ł"}, true, project.Segmenter),
						new UnnaturalClass("T", new[] {"t", "d", "ɗ", "ʈ", "ɖ", "θ", "ð"}, true, project.Segmenter),
 						new UnnaturalClass("W", new[] {"w", "ʋ", "v", "ʙ"}, true, project.Segmenter)
					};
			}
			else
			{
				Set(() => InitialEquivalenceThreshold, ref _initialEquivalenceThreshold, lexStat.InitialEquivalenceThreshold);
				soundClasses = lexStat.SoundClasses;
			}

			_soundClasses.SelectedSoundClass = null;
			_soundClasses.SoundClasses.Clear();
			foreach (SoundClass soundClass in soundClasses)
				_soundClasses.SoundClasses.Add(new SoundClassViewModel(soundClass));
			
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new LexStatCognateIdentifier(_segmentPool, _soundClasses.SoundClasses.Select(nc => nc.DomainSoundClass),
				_initialEquivalenceThreshold);
			_projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
