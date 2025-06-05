using Midas.Core;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class PlayerSessionService : CompositeGameService
	{
		internal readonly GameService<PlayerSession> SessionService = new GameService<PlayerSession>(HistorySnapshotType.None);
		internal readonly GameService<PlayerSessionParameters> ParametersService = new GameService<PlayerSessionParameters>(HistorySnapshotType.None);
		internal readonly GameService<bool> IsSessionTimerDisplayEnabledService = new GameService<bool>(HistorySnapshotType.None);

		public IGameServiceConsumer<PlayerSession> Session => SessionService.Variable;
		public IGameServiceConsumer<PlayerSessionParameters> Parameters => ParametersService.Variable;
		public IGameServiceConsumer<bool> IsSessionTimerDisplayEnabled => IsSessionTimerDisplayEnabledService.Variable;

		protected override void CreateServices()
		{
			AddService(SessionService, nameof(PidConfiguration));
			AddService(ParametersService, nameof(PidSession));
			AddService(IsSessionTimerDisplayEnabledService, nameof(IsSessionTimerDisplayEnabled));
		}
	}
}