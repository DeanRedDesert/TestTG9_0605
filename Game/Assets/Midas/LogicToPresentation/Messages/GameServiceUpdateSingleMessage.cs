using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Messages
{
	public sealed class GameServiceUpdateSingleMessage : GameServiceUpdateMessage
	{
		private IGameService gameService;
		private object value;

		public GameServiceUpdateSingleMessage(IGameService gameService, object value)
		{
			this.gameService = gameService;
			this.value = value;
		}

		public override void DeliverChanges()
		{
			gameService.DeliverChange(value);
		}
	}
}