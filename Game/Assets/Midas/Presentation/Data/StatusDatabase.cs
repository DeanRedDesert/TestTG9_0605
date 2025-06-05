using System;
using System.Linq;
using Midas.Presentation.Audio;
using Midas.Presentation.AutoPlay;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Denom;
using Midas.Presentation.Gaff;
using Midas.Presentation.Game;
using Midas.Presentation.History;
using Midas.Presentation.Info;
using Midas.Presentation.Lights;
using Midas.Presentation.StageHandling;
using Midas.Presentation.Stakes;
using Midas.Presentation.WinPresentation;

namespace Midas.Presentation.Data
{
	public static partial class StatusDatabase
	{
		/// <summary>
		/// Register to be notified when any status property changes.
		/// </summary>
		public static event PropertyChangedHandler AnyPropertyChanged
		{
			add => StatusBlocksInstance.AnyPropertyChanged += value;
			remove => StatusBlocksInstance.AnyPropertyChanged -= value;
		}

		#region Properties

		public static bool IsInitialised => StatusBlocksInstance != null;

		/// <summary>
		/// The root of the status database.
		/// </summary>
		public static StatusBlockCompound StatusBlocksInstance { get; private set; }

		// Below are the core status blocks in the database.

		public static ConfigurationStatus ConfigurationStatus { get; } = new ConfigurationStatus();
		public static BankStatus BankStatus { get; } = new BankStatus();
		public static PopupStatus PopupStatus { get; } = new PopupStatus();
		public static GameStatus GameStatus { get; } = new GameStatus();
		public static ProgressiveStatus ProgressiveStatus { get; } = new ProgressiveStatus();
		public static ExternalJackpotsStatus ExternalJackpotsStatus { get; } = new ExternalJackpotsStatus();
		public static PidStatus PidStatus { get; } = new PidStatus();
		public static GameSpeedStatus GameSpeedStatus { get; } = new GameSpeedStatus();
		public static PresentationDataStatus PresentationDataStatus { get; } = new PresentationDataStatus();
		public static DashboardStatus DashboardStatus { get; } = new DashboardStatus();
		public static DenomStatus DenomStatus { get; } = new DenomStatus();
		public static InfoStatus InfoStatus { get; } = new InfoStatus();
		public static AutoPlayStatus AutoPlayStatus { get; } = new AutoPlayStatus();
		public static GameFlowStatus GameFlowStatus { get; } = new GameFlowStatus();
		public static GameFunctionStatus GameFunctionStatus { get; } = new GameFunctionStatus();
		public static WinPresentationStatus WinPresentationStatus { get; } = new WinPresentationStatus();
		public static DetailedWinPresStatus DetailedWinPresStatus { get; } = new DetailedWinPresStatus();
		public static StakesStatus StakesStatus { get; } = new StakesStatus();
		public static HistoryStatus HistoryStatus { get; } = new HistoryStatus();
		public static UtilityStatus UtilityStatus { get; } = new UtilityStatus();
		public static GaffStatus GaffStatus { get; } = new GaffStatus();
		public static LightsStatus LightsStatus { get; } = new LightsStatus();
		public static StageStatus StageStatus { get; } = new StageStatus();
		public static ButtonEventDataQueueStatus ButtonEventDataQueueStatus { get; } = new ButtonEventDataQueueStatus();
		public static PlayerSessionStatus PlayerSessionStatus { get; } = new PlayerSessionStatus();
		public static VolumeStatus VolumeStatus { get; } = new VolumeStatus();

		#endregion

		#region Public Methods

		/// <summary>
		/// Initialize the database.
		/// </summary>
		public static void Init()
		{
			StatusBlocksInstance = new StatusBlockCompound(nameof(StatusDatabase));

			StatusBlocksInstance.AddStatusBlock(ConfigurationStatus);
			StatusBlocksInstance.AddStatusBlock(BankStatus);
			StatusBlocksInstance.AddStatusBlock(PopupStatus);
			StatusBlocksInstance.AddStatusBlock(GameStatus);
			StatusBlocksInstance.AddStatusBlock(ProgressiveStatus);
			StatusBlocksInstance.AddStatusBlock(ExternalJackpotsStatus);
			StatusBlocksInstance.AddStatusBlock(PidStatus);
			StatusBlocksInstance.AddStatusBlock(GameSpeedStatus);
			StatusBlocksInstance.AddStatusBlock(PresentationDataStatus);
			StatusBlocksInstance.AddStatusBlock(DashboardStatus);
			StatusBlocksInstance.AddStatusBlock(DenomStatus);
			StatusBlocksInstance.AddStatusBlock(InfoStatus);
			StatusBlocksInstance.AddStatusBlock(AutoPlayStatus);
			StatusBlocksInstance.AddStatusBlock(GameFlowStatus);
			StatusBlocksInstance.AddStatusBlock(GameFunctionStatus);
			StatusBlocksInstance.AddStatusBlock(WinPresentationStatus);
			StatusBlocksInstance.AddStatusBlock(DetailedWinPresStatus);
			StatusBlocksInstance.AddStatusBlock(StakesStatus);
			StatusBlocksInstance.AddStatusBlock(HistoryStatus);
			StatusBlocksInstance.AddStatusBlock(UtilityStatus);
			StatusBlocksInstance.AddStatusBlock(GaffStatus);
			StatusBlocksInstance.AddStatusBlock(LightsStatus);
			StatusBlocksInstance.AddStatusBlock(StageStatus);
			StatusBlocksInstance.AddStatusBlock(ButtonEventDataQueueStatus);
			StatusBlocksInstance.AddStatusBlock(PlayerSessionStatus);
			StatusBlocksInstance.AddStatusBlock(VolumeStatus);

			StatusBlocksInstance.Init();
		}

		/// <summary>
		/// Deinit the database.
		/// </summary>
		public static void DeInit()
		{
			StatusBlocksInstance.RemoveStatusBlock(VolumeStatus);
			StatusBlocksInstance.RemoveStatusBlock(PlayerSessionStatus);
			StatusBlocksInstance.RemoveStatusBlock(ButtonEventDataQueueStatus);
			StatusBlocksInstance.RemoveStatusBlock(StageStatus);
			StatusBlocksInstance.RemoveStatusBlock(LightsStatus);
			StatusBlocksInstance.RemoveStatusBlock(GaffStatus);
			StatusBlocksInstance.RemoveStatusBlock(UtilityStatus);
			StatusBlocksInstance.RemoveStatusBlock(HistoryStatus);
			StatusBlocksInstance.RemoveStatusBlock(StakesStatus);
			StatusBlocksInstance.RemoveStatusBlock(DetailedWinPresStatus);
			StatusBlocksInstance.RemoveStatusBlock(WinPresentationStatus);
			StatusBlocksInstance.RemoveStatusBlock(GameFunctionStatus);
			StatusBlocksInstance.RemoveStatusBlock(GameFlowStatus);
			StatusBlocksInstance.RemoveStatusBlock(AutoPlayStatus);
			StatusBlocksInstance.RemoveStatusBlock(InfoStatus);
			StatusBlocksInstance.RemoveStatusBlock(DenomStatus);
			StatusBlocksInstance.RemoveStatusBlock(DashboardStatus);
			StatusBlocksInstance.RemoveStatusBlock(PresentationDataStatus);
			StatusBlocksInstance.RemoveStatusBlock(GameSpeedStatus);
			StatusBlocksInstance.RemoveStatusBlock(PidStatus);
			StatusBlocksInstance.RemoveStatusBlock(ExternalJackpotsStatus);
			StatusBlocksInstance.RemoveStatusBlock(ProgressiveStatus);
			StatusBlocksInstance.RemoveStatusBlock(GameStatus);
			StatusBlocksInstance.RemoveStatusBlock(PopupStatus);
			StatusBlocksInstance.RemoveStatusBlock(BankStatus);
			StatusBlocksInstance.RemoveStatusBlock(ConfigurationStatus);

			StatusBlocksInstance.DeInit();
			StatusBlocksInstance = null;
			CheckHangingMultiplePropertyChangeHandlers();
			multiplePropertyChangedSubscriber.Clear();
		}

		/// <summary>
		/// Resets the entire status database. Done any time the game shuts down.
		/// </summary>
		public static void ResetDatabase()
		{
			StatusBlocksInstance.Reset();
		}

		public static void ResetForNewGame()
		{
			StatusBlocksInstance.ResetForNewGame();
		}

		/// <summary>
		/// Apply all modifications and send change notifications.
		/// </summary>
		/// <returns>True if any modification was made.</returns>
		public static bool ApplyAllModifications()
		{
			var result = StatusBlocksInstance.ApplyModifications();
			StatusBlocksInstance.SendChangedNotifications();

			foreach (var subscriber in multiplePropertyChangedSubscriber)
				subscriber.InvokeIfRequired();

			return result;
		}

		public static T QueryStatusBlock<T>(bool throwErrorWhenNotFound = true) where T : StatusBlock
		{
			var statusBlock = StatusBlocksInstance?.QueryStatusBlock<T>();

			if (statusBlock == null && throwErrorWhenNotFound)
				throw new InvalidOperationException($"Unable to find status block {typeof(T).Name}");

			return statusBlock;
		}

		public static T AddStatusBlock<T>(T statusBlock) where T : StatusBlock
		{
			StatusBlocksInstance.AddStatusBlock(statusBlock);
			return statusBlock;
		}

		public static void RemoveStatusBlock<T>(T statusBlock) where T : StatusBlock
		{
			StatusBlocksInstance.RemoveStatusBlock(statusBlock);
		}

		#endregion
	}
}