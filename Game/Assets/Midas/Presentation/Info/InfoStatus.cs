using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;

namespace Midas.Presentation.Info
{
	public enum InfoMode
	{
		None,
		Lobby,
		StartSession,
		ViewSession,
		ViewGameInfo,
		Rules,
		ToggleService,
	}

	public sealed class InfoStatus : StatusBlock
	{
		private StatusProperty<InfoMode> activeMode;
		private StatusProperty<int> currentRulesPage;
		private StatusProperty<string> gameInformation;
		private StatusProperty<string> chanceOfWinningText;
		private StatusProperty<string> topFivePrizes;
		private StatusProperty<string> topFiveOdds;
		private StatusProperty<string> bottomFivePrizes;
		private StatusProperty<string> bottomFiveOdds;
		private StatusProperty<string> sessionData;
		private StatusProperty<string> progressiveCeilingText;
		private StatusProperty<string> ancillaryText;

		public InfoMode ActiveMode => activeMode.Value;

		public int CurrentRulesPage
		{
			get => currentRulesPage.Value;
			set => currentRulesPage.Value = value;
		}

		public string GameInformation
		{
			get => gameInformation.Value;
			set => gameInformation.Value = value;
		}

		public string ChanceOfWinningText
		{
			get => chanceOfWinningText.Value;
			set => chanceOfWinningText.Value = value;
		}

		public string TopFivePrizes
		{
			get => topFivePrizes.Value;
			set => topFivePrizes.Value = value;
		}

		public string TopFiveOdds
		{
			get => topFiveOdds.Value;
			set => topFiveOdds.Value = value;
		}

		public string BottomFivePrizes
		{
			get => bottomFivePrizes.Value;
			set => bottomFivePrizes.Value = value;
		}

		public string BottomFiveOdds
		{
			get => bottomFiveOdds.Value;
			set => bottomFiveOdds.Value = value;
		}

		public string SessionData
		{
			get => sessionData.Value;
			set => sessionData.Value = value;
		}

		public string ProgressiveCeilingText
		{
			get => progressiveCeilingText.Value;
			set => progressiveCeilingText.Value = value;
		}

		public string AncillaryText
		{
			get => ancillaryText.Value;
			set => ancillaryText.Value = value;
		}

		public void SetActiveMode(InfoMode mode)
		{
			activeMode.Value = mode;
			StatusDatabase.PopupStatus.Set(Popup.Info, mode != InfoMode.None);
		}

		public InfoStatus() : base(nameof(InfoStatus))
		{
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			activeMode = AddProperty(nameof(ActiveMode), InfoMode.None);
			currentRulesPage = AddProperty(nameof(CurrentRulesPage), 0);
			gameInformation = AddProperty(nameof(GameInformation), string.Empty);
			chanceOfWinningText = AddProperty(nameof(ChanceOfWinningText), string.Empty);
			topFivePrizes = AddProperty(nameof(TopFivePrizes), string.Empty);
			topFiveOdds = AddProperty(nameof(TopFiveOdds), string.Empty);
			bottomFivePrizes = AddProperty(nameof(BottomFivePrizes), string.Empty);
			bottomFiveOdds = AddProperty(nameof(BottomFiveOdds), string.Empty);
			sessionData = AddProperty(nameof(SessionData), string.Empty);
			progressiveCeilingText = AddProperty(nameof(ProgressiveCeilingText), string.Empty);
			ancillaryText = AddProperty(nameof(AncillaryText), string.Empty);
		}
	}
}