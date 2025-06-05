using Midas.Core.General;
using Midas.Core.LogicServices;
using Midas.LogicToPresentation.Data;

namespace Midas.CreditPlayoff.LogicToPresentation
{
	public sealed class CreditPlayoffService : CompositeGameService
	{
		internal readonly GameService<CreditPlayoffState> StateService = new GameService<CreditPlayoffState>(HistorySnapshotType.None);
		internal readonly GameService<Money> BetService = new GameService<Money>(HistorySnapshotType.None);
		internal readonly GameService<Money> CashService = new GameService<Money>(HistorySnapshotType.None);
		internal readonly GameService<int> ResultNumberService = new GameService<int>(HistorySnapshotType.None);

		public IGameServiceConsumer<CreditPlayoffState> State => StateService.Variable;
		public IGameServiceConsumer<Money> Bet => BetService.Variable;
		public IGameServiceConsumer<Money> Cash => CashService.Variable;
		public IGameServiceConsumer<int> ResultNumber => ResultNumberService.Variable;

		public static CreditPlayoffService Instance { get; private set; }

		public static void Create()
		{
			GameServices.AddService(Instance = new CreditPlayoffService(), nameof(CreditPlayoffService));
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
			AddService(StateService, nameof(State));
			AddService(BetService, nameof(Bet));
			AddService(CashService, nameof(Cash));
			AddService(ResultNumberService, nameof(ResultNumber));
		}
	}
}