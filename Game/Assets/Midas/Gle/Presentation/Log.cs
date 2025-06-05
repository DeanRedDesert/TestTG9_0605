using Midas.Logging;

namespace Midas.Gle.Presentation
{
	internal static class Log
	{
		public static Logger Instance { get; } = Factory.GetLogger(typeof(Log));
	}
}