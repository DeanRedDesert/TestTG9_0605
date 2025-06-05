using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// Holds resolved selector item.  The input set conditions that were extracted from the data and the data to use when they are satisfied.
	/// </summary>
	public sealed class SelectorItem : IToString
	{
		/// <summary>
		/// The requirements for the selector entry to be valid.
		/// All requirements must contain at least one matching requirement for the entire selector entry to be valid.
		/// e.g. If requirements are:
		///   [ 1cr, 2cr, 5cr ]
		///   [ "Base", "Free" ]
		/// 
		/// Then parameters of [1cr, "Base"] would be valid but [1cr, "Blah"] would not.
		/// </summary>
		public IReadOnlyList<Requirement> Requirements { get; }

		/// <summary>
		/// The data to use if selected.
		/// </summary>
		public object Data { get; }

		public SelectorItem(IReadOnlyList<Requirement> requirements, object data)
		{
			Requirements = requirements;
			Data = data;
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				var lines = new List<string>();
				lines.Add("Requirements:");
				var arr = new List<List<string>>();

				foreach (var requirement in Requirements)
				{
					var a = new List<string> { requirement.Hint };
					a.AddRange(requirement.Values.Select(v => StringConverter.TryToString(v, "SL", out var s) ? s : "<no converter>"));
					arr.Add(a);
				}

				lines.AddRange(arr.ToTableLines().Indent(2, ' '));
				lines.Add("");
				lines.Add($"Data: {Data.GetType().ToDisplayString()}");

				if (StringConverter.TryToString(Data, "ML", out var s2))
					lines.AddRange(s2.ToLines(false, false).Indent(2, ' '));
				else if (StringConverter.TryToString(Data, "SL", out var s1))
					lines.Add($"  {s1}");
				else
					lines.Add($"  No converter - {Data.GetType().ToDisplayString()}");

				return lines.Join().ToSuccess();
			}

			return new NotSupported();
		}
	}
}