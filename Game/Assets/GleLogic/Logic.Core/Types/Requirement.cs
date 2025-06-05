using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Types
{
	/// <summary>
	/// A requirement is a list of values, of which ONE must be matched for the SelectorItem to be chosen.
	/// </summary>
	public sealed class Requirement : IToCode
	{
		private readonly HashSet<object> valuesHash;

		/// <summary>
		/// A hint to show in the GLE UI so a user will know what to wire up to the Select input.
		/// Keep it SHORT and WITHOUT whitespace.
		/// </summary>
		public string Hint { get; }

		/// <summary>
		/// The valid values for the Requirement
		/// </summary>
		public IReadOnlyList<object> Values { get; }

		public Requirement(string hint, IReadOnlyList<object> values)
		{
			Hint = hint;
			Values = values;
			valuesHash = values == null ? new HashSet<object>() : new HashSet<object>(values);
		}

		/// <summary>
		/// Checks if the object is in the Values collection.
		/// This is optimised to be an O(1) call.
		///
		/// Note: Use this instead of Values.Contains() which is O(n).
		/// </summary>
		public bool Contains(object value) => valuesHash.Contains(value);

		/// <summary>
		/// Helper method for use in the IToCode implementation.
		/// </summary>
		public static Requirement Create(string hint, params object[] values)
			=> new Requirement(hint, values);

		/// <inheritdoc cref="IToCode.ToCode(CodeGenArgs?)" />
		public IResult ToCode(CodeGenArgs args)
		{
			var values = Values.Select(r => CodeConverter.ToCodeOrThrow(args, r));
			return $"{CodeConverter.ToCode<Requirement>(args)}.{nameof(Create)}(\"{Hint}\", {string.Join(", ", values)})".ToSuccess();
		}
	}
}