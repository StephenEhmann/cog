using System.Diagnostics;
using System.Xml.Linq;
using SIL.Cog.Domain.Components;
using SIL.Machine.Annotations;

namespace SIL.Cog.Domain.Config.Components
{
    public class StatisticalProcessorConfig : IComponentConfig<IProcessor<VarietyPair>>
	{
		public IProcessor<VarietyPair> Load(SpanFactory<ShapeNode> spanFactory, SegmentPool segmentPool, CogProject project, XElement elem)
		{
			XElement statisticalMethodElem = elem.Element(ConfigManager.Cog + "ApplicableStatisticalMethod");
			Debug.Assert(statisticalMethodElem != null);
			var statisticalMethodID = (string) statisticalMethodElem.Attribute("ref");
			XElement alignerElem = elem.Element(ConfigManager.Cog + "ApplicableWordAligner");
			Debug.Assert(alignerElem != null);
			var alignerID = (string)alignerElem.Attribute("ref");
			XElement cognateIdentifierElem = elem.Element(ConfigManager.Cog + "ApplicableCognateIdentifier");
			Debug.Assert(cognateIdentifierElem != null);
			var cognateIdentifierID = (string)cognateIdentifierElem.Attribute("ref");
			return new StatisticalProcessor(segmentPool, project, statisticalMethodID, alignerID, cognateIdentifierID);
		}

		public void Save(IProcessor<VarietyPair> component, XElement elem)
		{
            var statisticalProcessor = (StatisticalProcessor)component;
            elem.Add(new XElement(ConfigManager.Cog + "ApplicableStatisticalProcessor", new XAttribute("ref", statisticalProcessor.StatisticalMethodID)));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableWordAligner", new XAttribute("ref", statisticalProcessor.AlignerID)));
			elem.Add(new XElement(ConfigManager.Cog + "ApplicableCognateIdentifier", new XAttribute("ref", statisticalProcessor.CognateIdentifierID)));
		}
	}
}
