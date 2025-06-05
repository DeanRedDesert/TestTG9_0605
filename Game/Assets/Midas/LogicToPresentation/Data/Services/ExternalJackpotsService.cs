using System.Collections.Generic;
using Midas.Core;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class ExternalJackpotsService : CompositeGameService
	{
		internal readonly GameService<bool> IsVisibleService = new GameService<bool>(HistorySnapshotType.None);
		internal readonly GameService<int> IconIdService = new GameService<int>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<ExternalJackpot>> JackpotsService = new GameService<IReadOnlyList<ExternalJackpot>>(HistorySnapshotType.None);

		public IGameServiceConsumer<bool> IsVisible => IsVisibleService.Variable;
		public IGameServiceConsumer<int> IconId => IconIdService.Variable;
		public IGameServiceConsumer<IReadOnlyList<ExternalJackpot>> ExternalJackpots => JackpotsService.Variable;

		protected override void CreateServices()
		{
			AddService(IsVisibleService, nameof(IsVisible));
			AddService(IconIdService, nameof(IconId));
			AddService(JackpotsService, nameof(ExternalJackpots));
		}
	}
}