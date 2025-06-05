using System.Linq;
using System.Text.RegularExpressions;

namespace Midas.Tools.Humanize
{
	/// <summary>
	/// Contains extension methods for dehumanizing strings.
	/// </summary>
	public static class StringDehumanizeExtensions
	{
		private static readonly Regex anyLetters = new Regex(@"\p{L}");

		/// <summary>
		/// Dehumanizes a string; e.g. 'some string', 'Some String', 'Some string' -> 'SomeString'
		/// If a string is already dehumanized then it leaves it alone 'SomeStringAndAnotherString' -> 'SomeStringAndAnotherString'
		/// </summary>
		/// <param name="input">The string to be dehumanized</param>
		public static string Dehumanize(this string input)
		{
			// If there are no letters, leave it alone

			if (!anyLetters.IsMatch(input))
				return input;

			var pascalizedWords = input
				.Split(' ')
				.Select(word => word
					.Humanize()
					.Pascalize());
			return string
				.Concat(pascalizedWords)
				.Replace(" ", "");
		}
	}
}