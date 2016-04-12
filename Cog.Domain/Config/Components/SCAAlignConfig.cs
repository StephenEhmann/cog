using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;
using SIL.Machine.FeatureModel;

namespace SIL.Cog.Domain.Config.Components
{
	public class SCAAlignConfig : WordAlignerConfigBase
	{
		public override IWordAligner Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			var settings = new SCAAlignSettings();
			LoadSettings(project.Segmenter, project.FeatureSystem, elem, settings);
			XElement relevantFeaturesElem = elem.Element(ConfigManager.Cog + "RelevantFeatures");
			Debug.Assert(relevantFeaturesElem != null);

			var relevantVowelFeatures = new List<SymbolicFeature>();
			var relevantConsFeatures = new List<SymbolicFeature>();
			var featureWeights = new Dictionary<SymbolicFeature, int>();
			var valueMetrics = new Dictionary<FeatureSymbol, int>();

			foreach (XElement featureElem in relevantFeaturesElem.Elements(ConfigManager.Cog + "RelevantFeature"))
			{
				var feature = project.FeatureSystem.GetFeature<SymbolicFeature>((string) featureElem.Attribute("ref"));
				if ((bool?) featureElem.Attribute("vowel") ?? false)
					relevantVowelFeatures.Add(feature);
				if ((bool?) featureElem.Attribute("consonant") ?? false)
					relevantConsFeatures.Add(feature);
				featureWeights[feature] = (int) featureElem.Attribute("weight");
				foreach (XElement valueElem in featureElem.Elements(ConfigManager.Cog + "RelevantValue"))
				{
					FeatureSymbol symbol = feature.PossibleSymbols[(string) valueElem.Attribute("ref")];
					valueMetrics[symbol] = (int) valueElem.Attribute("metric");
				}
			}

			return new SCAAlign(segmentPool, relevantVowelFeatures, relevantConsFeatures, featureWeights, valueMetrics, settings);
		}

		public override void Save(IWordAligner component, XElement elem)
		{
			var SCAAlign = (SCAAlign) component;
			SaveSettings(SCAAlign.Settings, elem);
			elem.Add(new XElement(ConfigManager.Cog + "RelevantFeatures", SCAAlign.FeatureWeights.Select(kvp =>
				new XElement(ConfigManager.Cog + "RelevantFeature", new XAttribute("ref", kvp.Key.ID), new XAttribute("weight", kvp.Value),
					new XAttribute("vowel", SCAAlign.RelevantVowelFeatures.Contains(kvp.Key)),
					new XAttribute("consonant", SCAAlign.RelevantConsonantFeatures.Contains(kvp.Key)),
					kvp.Key.PossibleSymbols.Select(fs =>
						new XElement(ConfigManager.Cog + "RelevantValue", new XAttribute("ref", fs.ID), new XAttribute("metric", SCAAlign.ValueMetrics[fs])))))));
		}
	}
}
