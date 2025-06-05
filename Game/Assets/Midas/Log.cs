using Midas.Logging;

namespace Midas
{
	internal static class Log
	{
		#region Public

		public static Logger Instance { get; } = Factory.GetLogger(typeof(Log));

		#endregion
	}
}