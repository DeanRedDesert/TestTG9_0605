using Midas.Core.General;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class MetersService : CompositeGameService
	{
		internal readonly GameService<Money> CreditsService = new GameService<Money>(HistorySnapshotType.GameCycle);
		internal readonly GameService<Money> WagerableService = new GameService<Money>(HistorySnapshotType.GameCycle);
		internal readonly GameService<Money> PaidService = new GameService<Money>(HistorySnapshotType.GameCycle);
		internal readonly GameService<Money> CycleAwardService = new GameService<Money>(HistorySnapshotType.GameCycle);
		internal readonly GameService<Money> TotalAwardService = new GameService<Money>(HistorySnapshotType.GameCycle);

		public IGameServiceConsumer<Money> CreditMeter => CreditsService.Variable;
		public IGameServiceConsumer<Money> WagerableMeter => WagerableService.Variable;
		public IGameServiceConsumer<Money> PaidMeter => PaidService.Variable;
		public IGameServiceConsumer<Money> CycleAward => CycleAwardService.Variable;
		public IGameServiceConsumer<Money> TotalAward => TotalAwardService.Variable;

		protected override void CreateServices()
		{
			AddService(CreditsService, nameof(CreditMeter));
			AddService(WagerableService, nameof(WagerableService));
			AddService(PaidService, nameof(PaidMeter));
			AddService(CycleAwardService, nameof(CycleAward));
			AddService(TotalAwardService, nameof(TotalAward));
		}
	}
}