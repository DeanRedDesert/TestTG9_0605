using System;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using TMPro;
using UnityEngine;

namespace Game.GameIdentity.Global.Dashboard
{
	public sealed class GlobalDashboardLanguageButtonPres : GlobalDashboardButtonPres
	{
		[Serializable]
		private class LanguageCodeToName
		{
			[Tooltip("The two letter language code")]
			public string languageCodePrefix;

			[Tooltip("The language name in its native language")]
			public string languageName;
		}

		[SerializeField]
		private TMP_Text languageName;

		[SerializeField]
		[Tooltip("Use this if your language is not in the default list")]
		private LanguageCodeToName[] customLanguageCodeToNameTable;

		public override bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData)
		{
			return true;
		}

		public override bool IsButtonSelected(Button button, ButtonStateData buttonStateData)
		{
			var languageData = buttonStateData.SpecificData as LanguageButtonSpecificData;
			return languageData?.IsSelected ?? false;
		}

		public override void RefreshVisualState(Button button, ButtonStateData buttonStateData)
		{
			base.RefreshVisualState(button, buttonStateData);

			if (!gameObject.activeSelf)
				return;

			var languageData = buttonStateData.SpecificData as LanguageButtonSpecificData;
			languageName.text = GetLanguageName(languageData);

			var lpl = GetComponentInParent<LanguagePanelLayout>();
			if (lpl)
				lpl.Refresh();
		}

		private string GetLanguageName(LanguageButtonSpecificData languageData)
		{
			if (languageData == null)
				return "NULL";

			var languagePrefix = languageData.LanguageCode.Substring(0, 2).ToLowerInvariant();

			if (customLanguageCodeToNameTable != null && customLanguageCodeToNameTable.Length > 0)
			{
				foreach (var language in customLanguageCodeToNameTable)
				{
					if (languagePrefix.Equals(language.languageCodePrefix, StringComparison.InvariantCultureIgnoreCase))
						return language.languageName;
				}
			}

			switch (languagePrefix)
			{
				case "en": return "English";
				case "es": return "Español";
				case "zh": return "中文";
				case "fr": return "Français";

				default: return $"Unknown: {languagePrefix}";
			}
		}
	}
}