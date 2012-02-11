﻿using System.Linq;
using System.Text;
using SIL.Machine;

namespace SIL.Cog
{
	public class Word : IData<ShapeNode>
	{
		private readonly Shape _shape;
		private readonly string _gloss;
		private readonly string _category;

		public Word(Shape shape, string gloss, string category)
		{
			_shape = shape;
			_gloss = gloss;
			_category = category;
		}

		public Shape Shape
		{
			get { return _shape; }
		}

		public string Gloss
		{
			get { return _gloss; }
		}

		public string Category
		{
			get { return _category; }
		}

		public Span<ShapeNode> Span
		{
			get { return _shape.Span; }
		}

		public AnnotationList<ShapeNode> Annotations
		{
			get { return _shape.Annotations; }
		}

		public override string ToString()
		{
			Annotation<ShapeNode> stemAnn = _shape.Annotations.SingleOrDefault(ann => ann.Type() == CogFeatureSystem.StemType);

			var sb = new StringBuilder();
			if (stemAnn != null)
			{
				string prefix = GetString(_shape.First, stemAnn.Span.Start.Prev);
				if (prefix.Length > 0)
				{
					sb.Append(prefix);
					sb.Append(" ");
				}
			}
			sb.Append("|");
			bool first = true;
			foreach (ShapeNode node in stemAnn == null ? _shape : _shape.GetNodes(stemAnn.Span))
			{
				if (!first)
					sb.Append(" ");
				sb.Append((string) node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep));
				first = false;
			}
			sb.Append("|");

			if (stemAnn != null)
			{
				string suffix = GetString(stemAnn.Span.End.Next, _shape.Last);
				if (suffix.Length > 0)
				{
					sb.Append(" ");
					sb.Append(suffix);
				}
			}

			return sb.ToString();
		}

		private string GetString(ShapeNode startNode, ShapeNode endNode)
		{
			if (startNode == null || endNode == null || startNode == _shape.End || endNode == _shape.Begin)
				return "";

			return string.Concat(startNode.GetNodes(endNode).Select(node => (string)node.Annotation.FeatureStruct.GetValue(CogFeatureSystem.StrRep)));
		}
	}
}
