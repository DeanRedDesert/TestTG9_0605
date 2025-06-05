using Midas.Presentation.Data;
using Midas.Presentation.Game;

namespace Midas.Gle.Presentation
{
	public class GleGameController : IPresentationController
	{
		public static GleStatus GleStatus { get; private set; }

		public GleGameController()
		{
			GleStatus = StatusDatabase.AddStatusBlock(new GleStatus());
		}

		public void Init()
		{
		}

		public void DeInit()
		{
		}

		public void Destroy()
		{
			if (GleStatus != null)
			{
				StatusDatabase.RemoveStatusBlock(GleStatus);
				GleStatus = null;
			}
		}
	}
}