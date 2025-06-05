using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Game
{
	public sealed class GameSpecificController : IPresentationController
	{
		public static GameSpecificStatus Data { get; private set; }

		public GameSpecificController()
		{
			Data = new GameSpecificStatus();
			StatusDatabase.AddStatusBlock(Data);
		}

		public void Init()
		{
		}

		public void DeInit()
		{
		}

		public void Destroy()
		{
			StatusDatabase.RemoveStatusBlock(Data);
		}
	}
}