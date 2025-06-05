namespace Midas.Logging
{
	internal static class Log
	{
		public static Logger Instance { get; } = Factory.GetLogger(typeof(Log));
	}
}