using System;
using System.Collections.Generic;
using Midas.Core;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class GameFunctionStatusService : CompositeGameService
	{
		internal readonly GameService<IReadOnlyList<GameButtonBehaviour>> GameButtonBehavioursService = new GameService<IReadOnlyList<GameButtonBehaviour>>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<DenominationPlayableStatus>> DenominationPlayableStatusService = new GameService<IReadOnlyList<DenominationPlayableStatus>>(HistorySnapshotType.None);
		internal readonly GameService<TimeSpan> TimeoutService = new GameService<TimeSpan>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsTimeoutActiveService = new GameService<bool>(HistorySnapshotType.None);

		public IGameServiceConsumer<IReadOnlyList<GameButtonBehaviour>> GameButtonBehaviours => GameButtonBehavioursService.Variable;
		public IGameServiceConsumer<IReadOnlyList<DenominationPlayableStatus>> DenominationPlayableStatus => DenominationPlayableStatusService.Variable;
		public IGameServiceConsumer<TimeSpan> Timeout => TimeoutService.Variable;
		public IGameServiceConsumer<bool> IsTimeoutActive => IsTimeoutActiveService.Variable;

		protected override void CreateServices()
		{
			AddService(GameButtonBehavioursService, nameof(GameButtonBehaviours));
			AddService(DenominationPlayableStatusService, nameof(DenominationPlayableStatus));
			AddService(TimeoutService, nameof(Timeout));
			AddService(IsTimeoutActiveService, nameof(IsTimeoutActive));
		}
	}
}