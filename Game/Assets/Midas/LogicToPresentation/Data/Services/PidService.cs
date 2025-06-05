using System.Collections.Generic;
using Midas.Core;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class PidService : CompositeGameService
	{
		internal readonly GameService<PidConfiguration> PidConfigurationService = new GameService<PidConfiguration>(HistorySnapshotType.None);
		internal readonly GameService<PidSession> PidSessionService = new GameService<PidSession>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsServiceRequestedService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<double> GamesPerWinService = new GameService<double>(HistorySnapshotType.None);
		internal readonly GameService<double> MinGameRtpService = new GameService<double>(HistorySnapshotType.None);
		internal readonly GameService<double> MaxGameRtpService = new GameService<double>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<(string Prize, int Odds)>> LargestPrizesService = new GameService<IReadOnlyList<(string Prize, int Odds)>>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<(string Prize, int Odds)>> SmallestPrizesService = new GameService<IReadOnlyList<(string Prize, int Odds)>>(HistorySnapshotType.None);

		public IGameServiceConsumer<PidConfiguration> PidConfiguration => PidConfigurationService.Variable;
		public IGameServiceConsumer<PidSession> PidSession => PidSessionService.Variable;
		public IGameServiceConsumer<bool> IsServiceRequested => IsServiceRequestedService.Variable;
		public IGameServiceConsumer<double> GamesPerWin => GamesPerWinService.Variable;
		public IGameServiceConsumer<double> MinGameRtp => MinGameRtpService.Variable;
		public IGameServiceConsumer<double> MaxGameRtp => MaxGameRtpService.Variable;
		public IGameServiceConsumer<IReadOnlyList<(string Prize, int Odds)>> LargestPrizes => LargestPrizesService.Variable;
		public IGameServiceConsumer<IReadOnlyList<(string Prize, int Odds)>> SmallestPrizes => SmallestPrizesService.Variable;

		protected override void CreateServices()
		{
			AddService(GamesPerWinService, nameof(GamesPerWin));
			AddService(MinGameRtpService, nameof(MinGameRtp));
			AddService(MinGameRtpService, nameof(MinGameRtp));
			AddService(LargestPrizesService, nameof(LargestPrizes));
			AddService(SmallestPrizesService, nameof(SmallestPrizes));
			AddService(PidConfigurationService, nameof(PidConfiguration));
			AddService(PidSessionService, nameof(PidSession));
			AddService(IsServiceRequestedService, nameof(IsServiceRequested));
		}
	}
}