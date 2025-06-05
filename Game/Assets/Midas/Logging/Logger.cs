using System;
using System.Diagnostics;
using System.Reflection;
using log4net.Core;
using ILogger = log4net.Core.ILogger;

namespace Midas.Logging
{
	/// <summary>
	/// Abstract class for logging data do the configured log target.
	/// This class is not an interface to support conditional compilation
	/// </summary>
	public sealed class Logger
	{
		#region Fields

		private readonly ILogger logger;

		#endregion

		#region Abstract Properties

		public bool IsDebugEnabled => logger.IsEnabledFor(Level.Debug);
		public bool IsInfoEnabled => logger.IsEnabledFor(Level.Info);
		public bool IsWarnEnabled => logger.IsEnabledFor(Level.Warn);
		public bool IsErrorEnabled => logger.IsEnabledFor(Level.Error);
		public bool IsFatalEnabled => logger.IsEnabledFor(Level.Fatal);

		#endregion

		#region Construction

		public Logger(string name)
		{
			logger = LoggerManager.GetLogger(Assembly.GetCallingAssembly(), name);
		}

		#endregion

		#region Core Log Methods

		[Conditional("DEBUG")]
		public void Debug(object message, Exception exception, object context = null)
		{
			if (!IsDebugEnabled)
				return;

			var logEvent = new Log4NetLoggingEvent(logger.Name, Level.Debug, message, exception, context);
			logger.Log(logEvent);
		}

		public void Info(object message, Exception exception, object context = null)
		{
			if (!IsInfoEnabled)
				return;

			var logEvent = new Log4NetLoggingEvent(logger.Name, Level.Info, message, exception, context);
			logger.Log(logEvent);
		}

		public void Warn(object message, Exception exception, object context = null)
		{
			if (!IsWarnEnabled)
				return;

			var logEvent = new Log4NetLoggingEvent(logger.Name, Level.Warn, message, exception, context);
			logger.Log(logEvent);
		}

		public void Error(object message, Exception exception, object context = null)
		{
			if (!IsErrorEnabled)
				return;

			var logEvent = new Log4NetLoggingEvent(logger.Name, Level.Error, message, exception, context);
			logger.Log(logEvent);
		}

		public void Fatal(object message, Exception exception, object context = null)
		{
			if (!IsFatalEnabled)
				return;

			var logEvent = new Log4NetLoggingEvent(logger.Name, Level.Fatal, message, exception, context);
			logger.Log(logEvent);
		}

		#endregion

		#region Public Methods

		[Conditional("DEBUG")]
		public void Debug(object message, object context = null)
		{
			Debug(message, null, context);
		}

		[Conditional("DEBUG")]
		public void DebugFormat(string message, params object[] args)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, args));
		}

		[Conditional("DEBUG")]
		public void DebugFormat(string message, object arg0)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, new[] { arg0 }));
		}

		[Conditional("DEBUG")]
		public void DebugFormat(string message, object arg0, object arg1)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, new[] { arg0, arg1 }));
		}

		[Conditional("DEBUG")]
		public void DebugFormat(string message, object arg0, object arg1, object arg2)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, new[] { arg0, arg1, arg2 }));
		}

		[Conditional("DEBUG")]
		public void DebugFormatCtx(object context, string message, params object[] args)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, args), context);
		}

		[Conditional("DEBUG")]
		public void DebugFormatCtx(object context, string message, object arg0)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, new[] { arg0 }), context);
		}

		[Conditional("DEBUG")]
		public void DebugFormatCtx(object context, string message, object arg0, object arg1)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, new[] { arg0, arg1 }), context);
		}

		[Conditional("DEBUG")]
		public void DebugFormatCtx(object context, string message, object arg0, object arg1, object arg2)
		{
			if (IsDebugEnabled)
				Debug(string.Format(message, new[] { arg0, arg1, arg2 }), context);
		}

		public void Info(object message, object context = null)
		{
			Info(message, null, context);
		}

		public void InfoFormat(string message, params object[] args)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, args));
		}

		public void InfoFormat(string message, object arg0)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, new[] { arg0 }));
		}

		public void InfoFormat(string message, object arg0, object arg1)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, new[] { arg0, arg1 }));
		}

		public void InfoFormat(string message, object arg0, object arg1, object arg2)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, new[] { arg0, arg1, arg2 }));
		}

		public void InfoFormatCtx(object context, string message, params object[] args)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, args), context);
		}

		public void InfoFormatCtx(object context, string message, object arg0)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, new[] { arg0 }), context);
		}

		public void InfoFormatCtx(object context, string message, object arg0, object arg1)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, new[] { arg0, arg1 }), context);
		}

		public void InfoFormatCtx(object context, string message, object arg0, object arg1, object arg2)
		{
			if (IsInfoEnabled)
				Info(string.Format(message, new[] { arg0, arg1, arg2 }), context);
		}

		public void Warn(object message, object context = null)
		{
			Warn(message, null, context);
		}

		public void WarnFormat(string message, params object[] args)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, args));
		}

		public void WarnFormat(string message, object arg0)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, new[] { arg0 }));
		}

		public void WarnFormat(string message, object arg0, object arg1)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, new[] { arg0, arg1 }));
		}

		public void WarnFormat(string message, object arg0, object arg1, object arg2)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, new[] { arg0, arg1, arg2 }));
		}

		public void WarnFormatCtx(object context, string message, params object[] args)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, args), context);
		}

		public void WarnFormatCtx(object context, string message, object arg0)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, new[] { arg0 }), context);
		}

		public void WarnFormatCtx(object context, string message, object arg0, object arg1)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, new[] { arg0, arg1 }), context);
		}

		public void WarnFormatCtx(object context, string message, object arg0, object arg1, object arg2)
		{
			if (IsWarnEnabled)
				Warn(string.Format(message, new[] { arg0, arg1, arg2 }), context);
		}

		public void Error(object message, object context = null)
		{
			Error(message, null, context);
		}

		public void ErrorFormat(string message, params object[] args)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, args));
		}

		public void ErrorFormat(string message, object arg0)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, new[] { arg0 }));
		}

		public void ErrorFormat(string message, object arg0, object arg1)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, new[] { arg0, arg1 }));
		}

		public void ErrorFormat(string message, object arg0, object arg1, object arg2)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, new[] { arg0, arg1, arg2 }));
		}

		public void ErrorFormatCtx(object context, string message, params object[] args)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, args), context);
		}

		public void ErrorFormatCtx(object context, string message, object arg0)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, new[] { arg0 }), context);
		}

		public void ErrorFormatCtx(object context, string message, object arg0, object arg1)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, new[] { arg0, arg1 }), context);
		}

		public void ErrorFormatCtx(object context, string message, object arg0, object arg1, object arg2)
		{
			if (IsErrorEnabled)
				Error(string.Format(message, new[] { arg0, arg1, arg2 }), context);
		}

		public void Fatal(object message, object context = null)
		{
			Fatal(message, null, context);
		}

		public void FatalFormat(string message, params object[] args)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, args));
		}

		public void FatalFormat(string message, object arg0)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, new[] { arg0 }));
		}

		public void FatalFormat(string message, object arg0, object arg1)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, new[] { arg0, arg1 }));
		}

		public void FatalFormat(string message, object arg0, object arg1, object arg2)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, new[] { arg0, arg1, arg2 }));
		}

		public void FatalFormatCtx(object context, string message, params object[] args)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, args), context);
		}

		public void FatalFormatCtx(object context, string message, object arg0)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, new[] { arg0 }), context);
		}

		public void FatalFormatCtx(object context, string message, object arg0, object arg1)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, new[] { arg0, arg1 }), context);
		}

		public void FatalFormatCtx(object context, string message, object arg0, object arg1, object arg2)
		{
			if (IsFatalEnabled)
				Fatal(string.Format(message, new[] { arg0, arg1, arg2 }), context);
		}

		#endregion
	}
}