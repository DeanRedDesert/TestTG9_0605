using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Messages;

namespace Midas.Ascent.Cabinet
{
	internal sealed class ParkController : ICabinetController
	{
		private bool parked;

		public void Init()
		{
			Communication.PresentationDispatcher.AddHandler<ParkMessage>(OnParkMessage);
		}

		public void OnBeforeLoadGame()
		{
		}

		public void Resume()
		{
		}

		public void Pause()
		{
		}

		public void OnAfterUnLoadGame()
		{
		}

		public void DeInit()
		{
			Communication.PresentationDispatcher.RemoveHandler<ParkMessage>(OnParkMessage);
		}

		private void OnParkMessage(ParkMessage message)
		{
			if (message.Park == parked)
				return;

			if (message.Park)
				AscentCabinet.Pause();
			else
				AscentCabinet.Resume();

			parked = message.Park;
		}
	}
}