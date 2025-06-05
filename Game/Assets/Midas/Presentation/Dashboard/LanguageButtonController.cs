using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.ExtensionMethods;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Gamble;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;
using Midas.Presentation.Stakes;

namespace Midas.Presentation.Dashboard
{
	public sealed class LanguageButtonSpecificData
	{
		public string LanguageCode { get; }
		public bool IsSelected { get; }

		public LanguageButtonSpecificData(string languageCode, bool isSelected)
		{
			LanguageCode = languageCode;
			IsSelected = isSelected;
		}
	}

	public sealed class LanguageButtonController : ButtonController, IPlayerSessionReset
	{
		private GambleStatus gambleStatus;
		private DashboardController dashboardController;
		private InterruptController interruptController;

		public override void Init()
		{
			gambleStatus = StatusDatabase.QueryStatusBlock<GambleStatus>();
			dashboardController = GameBase.GameInstance.GetPresentationController<DashboardController>();
			interruptController = GameBase.GameInstance.GetPresentationController<InterruptController>();
			base.Init();
		}

		public void ResetForNewPlayerSession(IReadOnlyList<PlayerSessionParameterType> pendingResetParams, IList<PlayerSessionParameterType> resetDoneParams)
		{
			if (!pendingResetParams.Contains(PlayerSessionParameterType.Culture))
				return;

			resetDoneParams.Add(PlayerSessionParameterType.Culture);
			Communication.ToLogicSender.Send(new ChangeLanguageMessage(null));
		}

		protected override void RegisterEvents()
		{
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameLogicPaused));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.InActivePlay));
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.LanguageConfig));
			AddButtonConditionPropertyChanged(StatusDatabase.ConfigurationStatus, nameof(ConfigurationStatus.CurrentLanguage));
			AddButtonConditionPropertyChanged(StatusDatabase.AutoPlayStatus, nameof(AutoPlayStatus.State));
			AddButtonConditionPropertyChanged(ButtonEventDataQueueStatus, nameof(ButtonEventDataQueueStatus.ButtonFunction));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.CurrentGameState));
			AddButtonConditionPropertyChanged(StatusDatabase.GameStatus, nameof(GameStatus.GameIsActive));
			AddButtonConditionPropertyChanged(gambleStatus, nameof(GambleStatus.AwaitingSelection));
			RegisterButtonPressListener(LanguageButtonFunctions.ChangeLanguage, OnChangeLanguageButtonPressed);

			foreach (var button in LanguageButtonFunctions.LanguageButtons)
			{
				RegisterButtonPressListener(button, OnLanguageButtonPressed);
			}
		}

		private bool IsButtonEnabled()
		{
			return StatusDatabase.GameStatus.InActivePlay &&
				StatusDatabase.ConfigurationStatus.LanguageConfig.AvailableLanguages.Count > 1 &&
				(StatusDatabase.GameStatus.CurrentGameState == GameState.Idle || StatusDatabase.GameStatus.CurrentGameState == GameState.OfferGamble) &&
				(StatusDatabase.AutoPlayStatus.State == AutoPlayState.Idle || StatusDatabase.AutoPlayStatus.State == AutoPlayState.WaitPlayerConfirm) &&
				!StatusDatabase.GameStatus.GameLogicPaused &&
				!ButtonEventDataQueueStatus.ButtonFunction.IsPlayButtonFunction() &&
				!ButtonEventDataQueueStatus.ButtonFunction.IsStartAutoplayOrConfirmButtonFunction();
		}

		protected override void UpdateButtonStates()
		{
			if (StatusDatabase.ConfigurationStatus.LanguageConfig == null)
				return;

			if (StatusDatabase.ConfigurationStatus.LanguageConfig.AvailableLanguages.Count <= 1)
			{
				AddButtonState(LanguageButtonFunctions.ChangeLanguage, ButtonState.DisabledHide);
				foreach (var button in LanguageButtonFunctions.LanguageButtons)
					AddButtonState(button, ButtonState.DisabledHide);
			}
			else
			{
				var en = IsButtonEnabled();
				AddButtonState(LanguageButtonFunctions.ChangeLanguage, en);

				var i = 0;
				var availableLanguages = StatusDatabase.ConfigurationStatus.LanguageConfig.AvailableLanguages;
				var currentLanguage = StatusDatabase.ConfigurationStatus.CurrentLanguage;
				for (; i < availableLanguages.Count && i < LanguageButtonFunctions.LanguageButtons.Count; i++)
				{
					var language = availableLanguages[i];
					AddButtonState(LanguageButtonFunctions.LanguageButtons[i], en, new LanguageButtonSpecificData(language, language == currentLanguage));
				}

				for (; i < LanguageButtonFunctions.LanguageButtons.Count; i++)
					AddButtonState(LanguageButtonFunctions.LanguageButtons[i], ButtonState.DisabledHide);
			}
		}

		private void OnChangeLanguageButtonPressed(ButtonEventData buttonEventData)
		{
			if (IsButtonEnabled())
			{
				interruptController.Interrupt(false);
				dashboardController.ChangeLanguageRequest();
			}
		}

		private void OnLanguageButtonPressed(ButtonEventData buttonEventData)
		{
			if (!IsButtonEnabled())
				return;

			var languageIndex = LanguageButtonFunctions.LanguageButtons.FindIndex(buttonEventData.ButtonFunction);
			if (languageIndex >= StatusDatabase.ConfigurationStatus.LanguageConfig.AvailableLanguages.Count)
				return;

			var language = StatusDatabase.ConfigurationStatus.LanguageConfig.AvailableLanguages[languageIndex];
			dashboardController.SelectLanguage(language);
		}
	}
}