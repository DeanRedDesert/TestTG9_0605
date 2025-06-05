using System.Diagnostics.CodeAnalysis;
using Midas.Core;
using Midas.Presentation.Data;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Denom;
using Midas.Presentation.Game;

namespace Midas.Presentation.Dashboard
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	public static class DashboardExpressions
	{
		[Expression("Dashboard")] public static bool IsAnyPopupOpen => StatusDatabase.PopupStatus.AreOpen(Popup.Info | Popup.Reserve | Popup.DenomMenu | Popup.CashoutConfirm);

		[Expression("Dashboard")]
		public static bool ShowGameContentOnMain => !StatusDatabase.PopupStatus.AreOpen(Popup.Info | Popup.DenomMenu) && !StatusDatabase.GameStatus.CurrentGameState.IsGambleState();

		[Expression("Dashboard")]
		public static bool ShowGameContentOnDpp
		{
			get
			{
				if (GameBase.GameInstance == null)
					return true;

				return !StatusDatabase.PopupStatus.AreOpen(Popup.Info | Popup.DenomMenu)
					&& StatusDatabase.StageStatus.CurrentStage == StatusDatabase.StageStatus.DesiredStage
					&& StatusDatabase.StageStatus.DesiredStage == GameBase.GameInstance.BaseGameStage;
			}
		}

		[Expression("Dashboard")]
		public static bool ShowGameContentOnGlobalDpp
		{
			get
			{
				if (GameBase.GameInstance == null)
					return true;

				return StatusDatabase.StageStatus.DesiredStage == GameBase.GameInstance.BaseGameStage;
			}
		}

		[Expression("Dashboard")]
		public static ChooseConfigState ChooseConfigState
		{
			get
			{
				if (StatusDatabase.GameStatus.GameMode != FoundationGameMode.Play)
					return ChooseConfigState.None;

				if (StatusDatabase.StageStatus.DesiredStage != GameBase.GameInstance.BaseGameStage)
					return ChooseConfigState.None;

				if (StatusDatabase.ConfigurationStatus.IsChooserAvailable)
					return ChooseConfigState.MoreGames;

				return DenomExpressions.VisibleDenoms.Count > 1 ? ChooseConfigState.Denom : ChooseConfigState.None;
			}
		}
	}
}