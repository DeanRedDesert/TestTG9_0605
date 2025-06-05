using System.Linq;
using System.Text.RegularExpressions;

namespace Midas.Tools.Humanize
{
	/// <summary>
	/// Contains extension methods for humanizing string values.
	/// </summary>
	public static class StringHumanizeExtensions
	{
		private static readonly Regex pascalCaseWordPartsRegex;
		private static readonly Regex freestandingSpacingCharRegex;

		private const string OptionallyCapitalizedWord = @"\p{Lu}?\p{Ll}+";
		private const string IntegerAndOptionalLowercaseLetters = @"[0-9]+\p{Ll}*";
		private const string Acronym = @"\p{Lu}+(?=\p{Lu}|[0-9]|\b)";
		private const string SequenceOfOtherLetters = @"\p{Lo}+";
		private const string MidSentencePunctuation = "[,;$]?";

		static StringHumanizeExtensions()
		{
			pascalCaseWordPartsRegex = new Regex(
				$"({OptionallyCapitalizedWord}|{IntegerAndOptionalLowercaseLetters}|{Acronym}|{SequenceOfOtherLetters}){MidSentencePunctuation}",
				RegexOptions.IgnorePatternWhitespace | RegexOptions.ExplicitCapture | RegexOptions.Compiled);
			freestandingSpacingCharRegex = new Regex(@"\s[-_]|[-_]\s", RegexOptions.Compiled);
		}

		private static string FromUnderscoreDashSeparatedWords(string input) =>
			string.Join(" ", input.Split('_', '-'));

		private static string FromPascalCase(string input)
		{
			var result = string.Join(" ", pascalCaseWordPartsRegex
				.Matches(input)
				// ReSharper disable once RedundantEnumerableCastCall
				.Cast<Match>()
				.Select(match =>
				{
					var value = match.Value;
					return value.All(char.IsUpper) &&
						(value.Length > 1 || match.Index > 0 && input[match.Index - 1] == ' ' || value == "I")
							? value
							: value.ToLower();
				}));

			if (result
					.Replace(" ", "")
					.All(char.IsUpper) &&
				result.Contains(" "))
				result = result.ToLower();

			return result.Length > 0
				? char.ToUpper(result[0]) +
				result.Substring(1, result.Length - 1)
				: result;
		}

		/// <summary>
		/// Humanizes the input string; e.g. Underscored_input_String_is_turned_INTO_sentence -> 'Underscored input String is turned INTO sentence'
		/// </summary>
		/// <param name="input">The string to be humanized</param>
		public static string Humanize(this string input)
		{
			// if input is all capitals (e.g. an acronym) then return it without change
			if (input.All(char.IsUpper))
				return input;

			// if input contains a dash or underscore which precedes or follows a space (or both, e.g. free-standing)
			// remove the dash/underscore and run it through FromPascalCase
			if (freestandingSpacingCharRegex.IsMatch(input))
				return FromPascalCase(FromUnderscoreDashSeparatedWords(input));

			if (input.Contains("_") || input.Contains("-"))
				return FromUnderscoreDashSeparatedWords(input);

			return FromPascalCase(input);
		}
	}
}