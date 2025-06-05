using System.Collections.Generic;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class GameServiceUpdateBatchMessage : GameServiceUpdateMessage
	{
		private readonly IReadOnlyList<(IGameService service, object value)> batch;

		public GameServiceUpdateBatchMessage(IReadOnlyList<(IGameService, object)> batch)
		{
			this.batch = batch;
		}

		public override void DeliverChanges()
		{
			foreach (var entry in batch)
				entry.service.DeliverChange(entry.value);
		}
	}
}