using System;

namespace Midas.Core
{
	public static class ErrorHandler
	{
		public static event Action<string> OnError;

		public static void ReportError(string errorMessage)
		{
			OnError?.Invoke(errorMessage);
		}
	}
}