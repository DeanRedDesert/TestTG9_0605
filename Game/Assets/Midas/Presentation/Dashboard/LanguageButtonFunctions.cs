using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.Dashboard
{
	[ButtonFunctions("Language")]
	public static class LanguageButtonFunctions
	{
		public static bool IsLanguageButtonFunction(this ButtonFunction buttonFunction)
		{
			return buttonFunction.Equals(ChangeLanguage) || LanguageButtons.Contains(buttonFunction);
		}

		public static ButtonFunction ChangeLanguage { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 0);
		public static ButtonFunction Language1 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 1);
		public static ButtonFunction Language2 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 2);
		public static ButtonFunction Language3 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 3);
		public static ButtonFunction Language4 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 4);
		public static ButtonFunction Language5 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 5);
		public static ButtonFunction Language6 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 6);
		public static ButtonFunction Language7 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 7);
		public static ButtonFunction Language8 { get; } = ButtonFunction.Create(ButtonFunctions.LanguageBase + 8);

		public static IReadOnlyList<ButtonFunction> LanguageButtons => new ReadOnlyCollection<ButtonFunction>(new[] { Language1, Language2, Language3, Language4, Language5, Language6, Language7, Language8 });
	}
}