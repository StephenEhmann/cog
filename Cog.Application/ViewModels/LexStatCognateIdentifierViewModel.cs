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
                        new UnnaturalClass("A", new[] {"a", "ᴀ", "ã", "ɑ", "á", "à", "ā", "ǎ", "â"}, true, project.Segmenter),
                        new UnnaturalClass("C", new[] {"t͡s", "d͡z", "ʦ", "ʣ", "t͡ɕ", "d͡ʑ", "ʨ", "ʥ", "t͡ʃ", "d͡ʒ", "ʧ", "ʤ", "c", "ɟ", "t͡ʂ", "d͡ʐ", "č", "ž", "t͡θ", "ʄ"}, true, project.Segmenter),
                        new UnnaturalClass("B", new[] {"ɸ", "β", "f", "p͡f", "ƀ", "v", "ʙ"}, true, project.Segmenter),
                        new UnnaturalClass("E", new[] {"ɛ", "æ", "ɜ", "ɐ", "ʌ", "e", "ᴇ", "ə", "ɘ", "ɤ", "è", "é", "ē", "ě", "ê", "ɚ", "ǝ", "ẽ"}, true, project.Segmenter),
                        new UnnaturalClass("D", new[] {"θ", "ð", "ŧ", "þ", "đ"}, true, project.Segmenter),
                        new UnnaturalClass("G", new[] {"x", "ɣ", "χ"}, true, project.Segmenter),
                        new UnnaturalClass("I", new[] {"i", "ɪ", "ɨ", "ɿ", "ʅ", "ɯ", "ĩ", "í", "ǐ", "ì", "î", "ī", "ı"}, true, project.Segmenter),
                        new UnnaturalClass("H", new[] {"ʔ", "ħ", "ʕ", "h", "ɦ"}, true, project.Segmenter),
                        new UnnaturalClass("K", new[] {"k", "g", "q", "ɢ", "ɡ"}, true, project.Segmenter),
                        new UnnaturalClass("J", new[] {"j", "ɥ", "ɰ"}, true, project.Segmenter),
                        new UnnaturalClass("M", new[] {"m", "ɱ", "ʍ"}, true, project.Segmenter),
                        new UnnaturalClass("L", new[] {"l", "ȴ", "l", "ɭ", "ʎ", "ʟ", "ɬ", "ɮ", "ł", "ɫ"}, true, project.Segmenter),
                        new UnnaturalClass("O", new[] {"Œ", "ɒ"}, true, project.Segmenter),
                        new UnnaturalClass("N", new[] {"n", "ȵ", "ɳ", "ŋ", "ɴ", "ň", "ń", "ɲ"}, true, project.Segmenter),
                        new UnnaturalClass("P", new[] {"p", "b", "ɓ"}, true, project.Segmenter),
                        new UnnaturalClass("S", new[] {"s", "z", "ʃ", "ʒ", "ʂ", "ʐ", "ç", "ʝ", "š", "ž", "ɕ", "ɧ", "ʑ"}, true, project.Segmenter),
                        new UnnaturalClass("R", new[] {"ɹ", "ɻ", "ʀ", "ɾ", "r", "ʁ", "ɽ", "ɐ̯"}, true, project.Segmenter),
                        new UnnaturalClass("U", new[] {"œ", "ɞ", "ɔ", "ø", "ɵ", "o", "õ", "ó", "ò", "ō", "ɶ", "ô", "ɷ"}, true, project.Segmenter),
                        new UnnaturalClass("T", new[] {"t", "d", "ȶ", "ȡ", "ɗ", "ʈ", "ɖ"}, true, project.Segmenter),
                        new UnnaturalClass("W", new[] {"w", "ʋ", "ⱱ"}, true, project.Segmenter),
                        new UnnaturalClass("Y", new[] {"y", "ʏ", "ʉ", "u", "ᴜ", "ʊ", "ú", "ù"}, true, project.Segmenter),
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
