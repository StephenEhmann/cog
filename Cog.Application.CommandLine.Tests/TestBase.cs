﻿using System.IO;
using NUnit.Framework;

namespace SIL.Cog.Application.CommandLine.Tests
{
	public class TestBase
	{
		public void CheckVerbOutput(string input, string expectedOutput, CommonOptions verbUnderTest)
		{
			CheckVerbOutput(input, expectedOutput, verbUnderTest, true);
		}

		public void CheckVerbOutput(string input, string expectedOutput, CommonOptions verbUnderTest, bool stripNewlines)
		{
			var inputStream = new StringReader(input);
			var outputStream = new StringWriter();
			var retcode = verbUnderTest.DoWork(inputStream, outputStream);
			string TextResult = outputStream.ToString().Replace("\r\n", "\n");
			if (stripNewlines)
				TextResult = TextResult.Replace("\n", "");
			Assert.That(TextResult, Is.EqualTo(expectedOutput));
			Assert.That(retcode, Is.EqualTo(0));
		}

	}
}
