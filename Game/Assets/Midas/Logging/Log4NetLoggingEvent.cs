using System;
using System.Runtime.Serialization;
using log4net.Core;

namespace Midas.Logging
{
	[Serializable]
	public sealed class Log4NetLoggingEvent : LoggingEvent
	{
		public object Context { get; }

		public Log4NetLoggingEvent(
			string loggerName,
			Level level,
			object message,
			Exception exception,
			object context)
			: base(typeof(Logger), null, loggerName, level, message, exception)
		{
			Context = context;
		}

		private Log4NetLoggingEvent(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}