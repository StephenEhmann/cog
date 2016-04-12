using System.Collections.Generic;
using System.Linq;
using SIL.Cog.Application.Services;
using SIL.Cog.Domain;
using SIL.Cog.Domain.Components;

namespace SIL.Cog.Application.ViewModels
{
	public class SCACognateIdentifierViewModel : ComponentSettingsViewModelBase
	{
		private readonly SegmentPool _segmentPool;
		private readonly IProjectService _projectService;
		private int _initialEquivalenceThreshold;
		private readonly SoundClassesViewModel _soundClasses;

		public SCACognateIdentifierViewModel(SegmentPool segmentPool, IProjectService projectService, SoundClassesViewModel soundClassesViewModel)
			: base("SCA")
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
			var sca = cognateIdentifier as SCACognateIdentifier;

			if (sca == null)
			{
				Set(() => InitialEquivalenceThreshold, ref _initialEquivalenceThreshold, 2);
			}
			else
			{
				Set(() => InitialEquivalenceThreshold, ref _initialEquivalenceThreshold, sca.InitialEquivalenceThreshold);
			}

            // TODO: these are non-editable (1: reflect that in the UI) because there is a built-in matrix for these for SCA alignment
            IEnumerable<SoundClass> soundClasses;
            soundClasses = new SoundClass[]
					{
                        new UnnaturalClass("A", new[] {"a", "ᴀ", "ã", "ɑ", "á", "à", "ā", "ǎ", "â"}, true, project.Segmenter, 0),
                        new UnnaturalClass("B", new[] {"ɸ", "β", "f", "p͡f", "ƀ", "v", "ʙ"}, true, project.Segmenter, 1),
                        new UnnaturalClass("C", new[] {"t͡s", "d͡z", "ʦ", "ʣ", "t͡ɕ", "d͡ʑ", "ʨ", "ʥ", "t͡ʃ", "d͡ʒ", "ʧ", "ʤ", "c", "ɟ", "t͡ʂ", "d͡ʐ", "č", "ž", "t͡θ", "ʄ"}, true, project.Segmenter, 2),
                        new UnnaturalClass("D", new[] {"θ", "ð", "ŧ", "þ", "đ"}, true, project.Segmenter, 3),
                        new UnnaturalClass("E", new[] {"ɛ", "æ", "ɜ", "ɐ", "ʌ", "e", "ᴇ", "ə", "ɘ", "ɤ", "è", "é", "ē", "ě", "ê", "ɚ", "ǝ", "ẽ"}, true, project.Segmenter, 4),
                        new UnnaturalClass("G", new[] {"x", "ɣ", "χ"}, true, project.Segmenter, 5),
                        new UnnaturalClass("H", new[] {"ʔ", "ħ", "ʕ", "h", "ɦ"}, true, project.Segmenter, 6),
                        new UnnaturalClass("I", new[] {"i", "ɪ", "ɨ", "ɿ", "ʅ", "ɯ", "ĩ", "í", "ǐ", "ì", "î", "ī", "ı"}, true, project.Segmenter, 7),
                        new UnnaturalClass("J", new[] {"j", "ɥ", "ɰ"}, true, project.Segmenter, 8),
                        new UnnaturalClass("K", new[] {"k", "g", "q", "ɢ", "ɡ"}, true, project.Segmenter, 9),
                        new UnnaturalClass("L", new[] {"l", "ȴ", "l", "ɭ", "ʎ", "ʟ", "ɬ", "ɮ", "ł", "ɫ"}, true, project.Segmenter,10),
                        new UnnaturalClass("M", new[] {"m", "ɱ", "ʍ"}, true, project.Segmenter, 11),
                        new UnnaturalClass("N", new[] {"n", "ȵ", "ɳ", "ŋ", "ɴ", "ň", "ń", "ɲ"}, true, project.Segmenter, 12),
                        new UnnaturalClass("O", new[] {"Œ", "ɒ"}, true, project.Segmenter, 13),
                        new UnnaturalClass("P", new[] {"p", "b", "ɓ"}, true, project.Segmenter, 14),
                        new UnnaturalClass("R", new[] {"ɹ", "ɻ", "ʀ", "ɾ", "r", "ʁ", "ɽ", "ɐ̯"}, true, project.Segmenter, 15),
                        new UnnaturalClass("S", new[] {"s", "z", "ʃ", "ʒ", "ʂ", "ʐ", "ç", "ʝ", "š", "ž", "ɕ", "ɧ", "ʑ"}, true, project.Segmenter, 16),
                        new UnnaturalClass("T", new[] {"t", "d", "ȶ", "ȡ", "ɗ", "ʈ", "ɖ"}, true, project.Segmenter, 17),
                        new UnnaturalClass("U", new[] {"œ", "ɞ", "ɔ", "ø", "ɵ", "o", "õ", "ó", "ò", "ō", "ɶ", "ô", "ɷ"}, true, project.Segmenter, 18),
                        new UnnaturalClass("W", new[] {"w", "ʋ", "ⱱ"}, true, project.Segmenter, 19),
                        new UnnaturalClass("Y", new[] {"y", "ʏ", "ʉ", "u", "ᴜ", "ʊ", "ú", "ù"}, true, project.Segmenter, 20),
					};

			_soundClasses.SelectedSoundClass = null;
			_soundClasses.SoundClasses.Clear();
			foreach (SoundClass soundClass in soundClasses)
				_soundClasses.SoundClasses.Add(new SoundClassViewModel(soundClass));
		}

		public override object UpdateComponent()
		{
			var cognateIdentifier = new SCACognateIdentifier(_segmentPool, _soundClasses.SoundClasses.Select(nc => nc.DomainSoundClass),
				_initialEquivalenceThreshold);
			_projectService.Project.CognateIdentifiers[ComponentIdentifiers.PrimaryCognateIdentifier] = cognateIdentifier;
			return cognateIdentifier;
		}
	}
}
