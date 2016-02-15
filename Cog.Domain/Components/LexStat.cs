using System;
using System.Linq;
using SIL.Machine.Annotations;
using SIL.Machine.NgramModeling;
using SIL.Machine.SequenceAlignment;
using SIL.Machine.Statistics;

namespace SIL.Cog.Domain.Components
{
    public class LexStat : IProcessor<VarietyPair>
	{
		private readonly SegmentPool _segmentPool;
		private readonly CogProject _project;
		private readonly string _alignerID;
		private readonly string _cognateIdentifierID;

		public LexStat(SegmentPool segmentPool, CogProject project, string alignerID, string cognateIdentifierID)
		{
			_segmentPool = segmentPool;
			_project = project;
			_alignerID = alignerID;
			_cognateIdentifierID = cognateIdentifierID;
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
            // TODO: port lexstat code into here
		}
	}
}
