﻿using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	[TestFixture]
    public class MakePairsTests : TestBase
	{
		[Test, Sequential]
		public void MakePairsProducesAllCombinations(
			[Values("one", "one\r\ntwo", "one\ntwo\n", "1\r\n2\r\n3\r\n", "1\n2\n3\n4")]
			string input,
			[Values("",    "one two\n",  "one two\n",  "1 2\n1 3\n2 3\n", "1 2\n1 3\n1 4\n2 3\n2 4\n3 4\n")]
			string expectedOutput)
		{
			var makePairs = new MakePairsVerb();
			CheckVerbOutput(input, expectedOutput, makePairs, stripNewlines:false);
		}
	}
}
