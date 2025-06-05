using System.Collections.Generic;
using Midas.Core.LogicServices;
using Midas.LogicToPresentation.Data;

namespace Midas.Gle.LogicToPresentation
{
	public sealed class GleService : CompositeGameService
	{
		internal readonly GameService<GleResults> PreviousGameResultsService = new GameService<GleResults>(HistorySnapshotType.None);
		internal readonly GameService<GleResults> CurrentGameResultsService = new GameService<GleResults>(HistorySnapshotType.None);
		internal readonly GameService<int> CurrentResultIndexService = new GameService<int>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<GleUserSelection>> PlayerDecisionsService = new GameService<IReadOnlyList<GleUserSelection>>(HistorySnapshotType.None);

		public IGameServiceConsumer<GleResults> PreviousGameResults => PreviousGameResultsService.Variable;
		public IGameServiceConsumer<GleResults> CurrentGameResults => CurrentGameResultsService.Variable;
		public IGameServiceConsumer<int> CurrentResultIndex => CurrentResultIndexService.Variable;
		public IGameServiceConsumer<IReadOnlyList<GleUserSelection>> PlayerDecisions => PlayerDecisionsService.Variable;

		public static GleService Instance { get; private set; }

		public static void Create()
		{
			GameServices.AddService(Instance = new GleService(), nameof(GleService));
		}

		public static void Destroy()
		{
			if (Instance != null)
			{
				GameServices.RemoveService(Instance);
				Instance = null;
			}
		}

		protected override void CreateServices()
		{
			AddService(PreviousGameResultsService, nameof(PreviousGameResults));
			AddService(CurrentGameResultsService, nameof(CurrentGameResults));
			AddService(CurrentResultIndexService, nameof(CurrentResultIndex));
			AddService(PlayerDecisionsService, nameof(PlayerDecisions));
		}
	}
}