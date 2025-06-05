using log4net.Appender;
using log4net.Core;
using Midas.Logging;
using UnityEngine;

namespace Midas.Presentation.Logging
{
	public sealed class UnityConsoleAppender : AppenderSkeleton
	{
		#region Protected

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (!(loggingEvent is Log4NetLoggingEvent { Context: Object context }))
			{
				context = null;
			}

			var logMessage = RenderLoggingEvent(loggingEvent);

			switch (loggingEvent.Level.Name)
			{
				case "WARN":
					Debug.LogWarning(logMessage, context);
					break;

				case "ERROR":
				case "FATAL":
					Debug.LogError(logMessage, context);
					Debug.Break();
					break;

				default:
					Debug.Log(logMessage, context);
					break;
			}
		}

		#endregion
	}
}