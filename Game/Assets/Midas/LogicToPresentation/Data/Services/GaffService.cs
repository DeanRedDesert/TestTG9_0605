using System.Collections.Generic;
using Midas.Core;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class GaffService : CompositeGameService
	{
		internal readonly GameService<IReadOnlyList<IGaffSequence>> GaffSequencesService = new GameService<IReadOnlyList<IGaffSequence>>(HistorySnapshotType.None);
		internal readonly GameService<bool> AreGaffCyclesPendingService = new GameService<bool>(HistorySnapshotType.None);

		public IGameServiceConsumer<IReadOnlyList<IGaffSequence>> GaffSequences => GaffSequencesService.Variable;
		public IGameServiceConsumer<bool> AreGaffCyclesPending => AreGaffCyclesPendingService.Variable;

		protected override void CreateServices()
		{
			AddService(GaffSequencesService, nameof(GaffSequences));
			AddService(AreGaffCyclesPendingService, nameof(AreGaffCyclesPending));
		}
	}
}