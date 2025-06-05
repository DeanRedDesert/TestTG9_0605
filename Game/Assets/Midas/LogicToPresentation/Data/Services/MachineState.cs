using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class MachineState : CompositeGameService
	{
		internal readonly GameService<DisplayState> DisplayStateService = new GameService<DisplayState>(HistorySnapshotType.None);
		internal readonly GameService<FoundationGameMode> GameModeService = new GameService<FoundationGameMode>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsPlayerWagerAvailableService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsCashoutAvailableService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<string>> MessagesService = new GameService<IReadOnlyList<string>>(HistorySnapshotType.None);
		internal readonly GameService<bool> ShowGameOverMessageService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsChooserAvailableService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> HasWinCapBeenReachedService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsGambleOfferableService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<DateTime> GameTimeService = new GameService<DateTime>(HistorySnapshotType.None);

		public IGameServiceConsumer<DisplayState> DisplayState => DisplayStateService.Variable;
		public IGameServiceConsumer<FoundationGameMode> GameMode => GameModeService.Variable;
		public IGameServiceConsumer<bool> IsPlayerWagerAvailable => IsPlayerWagerAvailableService.Variable;
		public IGameServiceConsumer<bool> IsCashoutAvailable => IsCashoutAvailableService.Variable;
		public IGameServiceConsumer<IReadOnlyList<string>> Messages => MessagesService.Variable;
		public IGameServiceConsumer<bool> ShowGameOverMessage => ShowGameOverMessageService.Variable;
		public IGameServiceConsumer<bool> IsChooserAvailable => IsChooserAvailableService.Variable;
		public IGameServiceConsumer<bool> HasWinCapBeenReached => HasWinCapBeenReachedService.Variable;
		public IGameServiceConsumer<bool> IsGambleOfferable => IsGambleOfferableService.Variable;
		public IGameServiceConsumer<DateTime> GameTime => GameTimeService.Variable;

		protected override void CreateServices()
		{
			AddService(DisplayStateService, nameof(DisplayState));
			AddService(GameModeService, nameof(GameMode));
			AddService(IsPlayerWagerAvailableService, nameof(IsPlayerWagerAvailable));
			AddService(IsCashoutAvailableService, nameof(IsCashoutAvailable));
			AddService(MessagesService, nameof(Messages));
			AddService(ShowGameOverMessageService, nameof(ShowGameOverMessage));
			AddService(IsChooserAvailableService, nameof(IsChooserAvailable));
			AddService(HasWinCapBeenReachedService, nameof(HasWinCapBeenReached));
			AddService(IsGambleOfferableService, nameof(IsGambleOfferable));
			AddService(GameTimeService, nameof(GameTime));
		}
	}
}