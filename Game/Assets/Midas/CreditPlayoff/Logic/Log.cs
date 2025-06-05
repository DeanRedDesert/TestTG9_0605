using Midas.Logging;

namespace Midas.CreditPlayoff.Logic
{
	internal static class Log
	{
		#region Public

		public static Logger Instance { get; } = Factory.GetLogger(typeof(Log));

		#endregion
	}
}