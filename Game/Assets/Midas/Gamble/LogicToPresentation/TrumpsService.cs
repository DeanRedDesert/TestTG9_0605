using System.Collections.Generic;
using Midas.Core.LogicServices;
using Midas.LogicToPresentation.Data;

namespace Midas.Gamble.LogicToPresentation
{
	public sealed class TrumpsService : CompositeGameService
	{
		internal readonly GameService<IReadOnlyList<TrumpsSuit>> HistoryService = new GameService<IReadOnlyList<TrumpsSuit>>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<TrumpsCycleData>> ResultsService = new GameService<IReadOnlyList<TrumpsCycleData>>(HistorySnapshotType.None);
		internal readonly GameService<int> CurrentResultIndexService = new GameService<int>(HistorySnapshotType.None);

		public IGameServiceConsumer<IReadOnlyList<TrumpsSuit>> History => HistoryService.Variable;
		public IGameServiceConsumer<IReadOnlyList<TrumpsCycleData>> Results => ResultsService.Variable;
		public IGameServiceConsumer<int> CurrentResultIndex => CurrentResultIndexService.Variable;

		public static TrumpsService Instance { get; private set; }

		public static void Create()
		{
			GameServices.AddService(Instance = new TrumpsService(), nameof(TrumpsService));
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
			AddService(HistoryService, nameof(History));
			AddService(ResultsService, nameof(Results));
			AddService(CurrentResultIndexService, nameof(CurrentResultIndex));
		}
	}
}