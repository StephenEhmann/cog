﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;
using SIL.Cog.Domain;
using SIL.Machine.Annotations;

namespace SIL.Cog.Application.CommandLine
{
	public abstract class CommonOptions
	{
		[Option('i', "input", Default = "-", HelpText = "Input filename (\"-\" for stdin)")]
		public string InputFilename { get; set; }

		[Option('o', "output", Default = "-", HelpText = "Output filename (\"-\" for stdout)")]
		public string OutputFilename { get; set; }

		protected SpanFactory<ShapeNode> _spanFactory;
		protected SegmentPool _segmentPool;
		protected CogProject _project;
		protected Variety _variety;
		protected Meaning _meaning;

		protected void SetUpProject()
		{
			_spanFactory = new ShapeSpanFactory();
			_segmentPool = new SegmentPool();
			_variety = new Variety("variety1");
			_meaning = new Meaning("gloss1", "cat1");
			_project = CommandLineHelpers.GetProject(_spanFactory, _segmentPool);
			_project.Meanings.Add(_meaning);
			_project.Varieties.Add(_variety);
		}

		public abstract int DoWork(TextReader input, TextWriter output);

		public int RunAsPipe()
		{
			using (StreamReader input = OpenInput())
			using (StreamWriter output = OpenOutput())
				return DoWork(input, output);
		}

		protected Word ParseWord(string wordText, Meaning meaning)
		{
			int stemStartIdx = wordText.IndexOf("|", StringComparison.Ordinal);
			int stemEndIdx = wordText.LastIndexOf("|", StringComparison.Ordinal) - 1; // -1 because we're going to remove the leading |
			var word = (stemStartIdx == -1) ?
				new Word(wordText, meaning) :
				new Word(wordText.Replace("|", ""), stemStartIdx, stemEndIdx - stemStartIdx, meaning);
			return word;
		}

		#region OpenInput and overloads

		protected StreamReader OpenInput()
		{
			return OpenInput(InputFilename);
		}

		protected StreamReader OpenInput(string filename)
		{
			return OpenInput(filename, new UTF8Encoding());
		}

		protected StreamReader OpenInput(Encoding encoding)
		{
			return OpenInput(InputFilename, encoding);
		}

		protected StreamReader OpenInput(string filename, Encoding encoding)
		{
			Stream source = filename == "-"
				? Console.OpenStandardInput()
				: new FileStream(filename, FileMode.Open, FileAccess.Read);
			return new StreamReader(source, encoding);
		}
		#endregion
		#region OpenOutput and overloads
		protected StreamWriter OpenOutput()
		{
			return OpenOutput(OutputFilename);
		}

		protected StreamWriter OpenOutput(string filename)
		{
			return OpenOutput(filename, new UTF8Encoding());
		}

		protected StreamWriter OpenOutput(Encoding encoding)
		{
			return OpenOutput(OutputFilename, encoding);
		}

		protected StreamWriter OpenOutput(string filename, Encoding encoding)
		{
			Stream source = filename == "-"
				? Console.OpenStandardOutput()
				: new FileStream(filename, FileMode.Create, FileAccess.Write);
			return new StreamWriter(source, encoding);
		}

		#endregion
	}
}