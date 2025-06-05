using Midas.Logging;

namespace Game
{
	internal static class GameLog
	{
		public static Logger Instance { get; } = Factory.GetLogger(typeof(GameLog));
	}
}