using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.ExtensionMethods;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

namespace Midas.Presentation.Dashboard
{
	public sealed partial class DashboardController
	{
		private static readonly TimeSpan languageMenuPopupTimeout = TimeSpan.FromSeconds(5);
		private static readonly TimeSpan languageMenuPopupTimeoutAfterChange = TimeSpan.FromSeconds(3);

		private TimeSpan languageMenuTimeout;
		private Coroutine languageMenuCoroutine;

		private void InitLanguage()
		{
			languageMenuCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(LanguageMenuCoroutine(), "LanguageMenu");
			StatusDatabase.ConfigurationStatus.AddPropertyChangedHandler(nameof(ConfigurationStatus.CurrentLanguage), OnCurrentLanguageChanged);
			SetLanguage();
		}

		private void DeInitLanguage()
		{
			StatusDatabase.ConfigurationStatus.RemovePropertyChangedHandler(nameof(ConfigurationStatus.CurrentLanguage), OnCurrentLanguageChanged);
			languageMenuCoroutine?.Stop();
			languageMenuCoroutine = null;
		}

		private static void OnCurrentLanguageChanged(StatusBlock sender, string propertyname)
		{
			SetLanguage();
		}

		private static void SetLanguage()
		{
			var currentLanguage = StatusDatabase.ConfigurationStatus?.CurrentLanguage;
			if (currentLanguage == null)
				return;

			var locale = LocalizationSettings.AvailableLocales.GetLocale(new LocaleIdentifier(currentLanguage));
			LocalizationSettings.SelectedLocale = locale;
		}

		private void LanguageOnMoneyIn()
		{
			StatusDatabase.PopupStatus.Open(Popup.Language);
		}

		private void LanguageButtonCheck(ButtonEventData eventData)
		{
			if (!PopupStatus.IsLanguageOpen)
				return;

			if (!eventData.ButtonFunction.IsLanguageButtonFunction())
				StatusDatabase.PopupStatus.Close(Popup.Language);
		}

		public void ChangeLanguageRequest()
		{
			if (!PopupStatus.IsLanguageOpen)
			{
				StatusDatabase.PopupStatus.Open(Popup.Language);
				languageMenuTimeout = FrameTime.CurrentTime + languageMenuPopupTimeout;
			}
			else
			{
				Communication.ToLogicSender.Send(new ChangeLanguageMessage(GetNextLanguage()));
				languageMenuTimeout = FrameTime.CurrentTime + languageMenuPopupTimeoutAfterChange;
			}
		}

		public void SelectLanguage(string languageCode)
		{
			if (!StatusDatabase.ConfigurationStatus.LanguageConfig.AvailableLanguages.Contains(languageCode))
				return;

			Communication.ToLogicSender.Send(new ChangeLanguageMessage(languageCode));
			languageMenuTimeout = FrameTime.CurrentTime + languageMenuPopupTimeoutAfterChange;
		}

		private IEnumerator<CoroutineInstruction> LanguageMenuCoroutine()
		{
			while (true)
			{
				while (!PopupStatus.IsLanguageOpen)
					yield return null;

				while (FrameTime.CurrentTime < languageMenuTimeout)
				{
					var currentState = StatusDatabase.GameStatus.CurrentGameState;
					if (currentState != GameState.OfferGamble && currentState != GameState.Idle)
						break;

					yield return null;
				}

				StatusDatabase.PopupStatus.Close(Popup.Language);
			}
		}

		private static string GetNextLanguage()
		{
			var languages = StatusDatabase.ConfigurationStatus.LanguageConfig.AvailableLanguages;
			var nextLanguageIndex = languages.FindIndex(StatusDatabase.ConfigurationStatus.CurrentLanguage);
			nextLanguageIndex = (nextLanguageIndex + 1) % languages.Count;
			return languages[nextLanguageIndex];
		}
	}
}