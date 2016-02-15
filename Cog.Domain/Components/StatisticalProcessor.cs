using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SIL.Collections;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
	public class StatisticalProcessor : IProcessor<VarietyPair>
	{
		private readonly CogProject _project;
		private readonly SegmentPool _segmentPool;
		private readonly string _statisticalMethodID;
		private readonly string _alignerID;
		private readonly string _cognateIdentifierID;

		public StatisticalProcessor(SegmentPool segmentPool, CogProject project, string statisticalMethodID, string alignerID, string cognateIdentifierID)
		{
			_project = project;
			_segmentPool = segmentPool;
			_statisticalMethodID = statisticalMethodID;
			_alignerID = alignerID;
			_cognateIdentifierID = cognateIdentifierID;
		}

		public string StatisticalMethodID
		{
			get { return _statisticalMethodID; }
		}
		
		public string AlignerID
		{
			get { return _alignerID; }
		}

		public string CognateIdentifierID
		{
			get { return _cognateIdentifierID; }
		}

        public void Process(VarietyPair varietyPair)
        {
			IProcessor<VarietyPair> statisticalProcessor = _project.StatisticalMethods[_statisticalMethodID];
			statisticalProcessor.Process(varietyPair);
        }
	}
}
