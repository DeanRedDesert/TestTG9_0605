using System.Text.RegularExpressions;

namespace Midas.Presentation.General
{
	public enum MonoSpacingMode
	{
		None,
		All,
		DigitsOnly,
	}

	public static class TextHelper
	{
		private const RegexOptions RegexOptions = System.Text.RegularExpressions.RegexOptions.Compiled;
		private static readonly Regex digitMonoSpace = new Regex(@"\d+", RegexOptions);

		public static string MonoSpace(string input, float size, MonoSpacingMode mode)
		{
			switch (mode)
			{
				default:
				case MonoSpacingMode.None:
					return input;
				case MonoSpacingMode.All:
					return MonoSpace(input, size);
				case MonoSpacingMode.DigitsOnly:
					return MonoSpaceDigits(input, size);
			}
		}

		public static string MonoSpaceDigits(string input, float size)
		{
			return digitMonoSpace.Replace(input, $"<mspace={size}>$&</mspace>");
		}

		public static string MonoSpace(string input, float size)
		{
			return $"<mspace={size}>{input}</mspace>";
		}
	}
}