using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gaff.Conditions
{
	public enum Constraint
	{
		Exact,
		StartsWith,
		EndsWith,
		Contains
	}

	public sealed class StringCondition
	{
		private readonly IReadOnlyList<string> options;

		// ReSharper disable MemberCanBePrivate.Global

		public string Text { get; }
		public Constraint Type { get; }
		public bool Invert { get; }

		// ReSharper restore MemberCanBePrivate.Global

		public StringCondition(string text, Constraint type, bool invert)
		{
			Text = text;
			Type = type;
			Invert = invert;

			options = text.Split('|');
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			if (Invert)
				sb.Append('!');

			switch (Type)
			{
				case Constraint.Exact: sb.Append($"Exact {Text}"); break;
				case Constraint.StartsWith: sb.Append($"StartsWith {Text}"); break;
				case Constraint.EndsWith: sb.Append($"EndsWith {Text}"); break;
				case Constraint.Contains: sb.Append($"Contains {Text}"); break;
				default: throw new ArgumentOutOfRangeException();
			}

			return sb.ToString();
		}

		public static implicit operator StringCondition(string s) => new StringCondition(s, Constraint.Exact, false);

		public bool Check(string other)
		{
			bool baseCheck;

			switch (Type)
			{
				case Constraint.Exact: baseCheck = options.Contains(other); break;
				case Constraint.StartsWith: baseCheck = options.Any(other.StartsWith); break;
				case Constraint.EndsWith: baseCheck = options.Any(other.EndsWith); break;
				case Constraint.Contains: baseCheck = options.Any(other.Contains); break;
				default: throw new NotSupportedException();
			}

			return Invert ? !baseCheck : baseCheck;
		}
	}
}