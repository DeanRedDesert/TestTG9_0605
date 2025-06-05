using System;
using System.IO;
using log4net.Appender;
using log4net.Core;
using UnityEngine;

namespace Midas.Logging
{
	// ReSharper disable once UnusedType.Global - Constructed internally by log4net
	public sealed class FatalErrorHandler : AppenderSkeleton
	{
		private const string UgpSystemErrorFileName = "ErrorLog.txt";
		private const string AscentSystemErrorFileName = "SystemError.txt";
		private static bool errorReportingDone;

		protected override void Append(LoggingEvent loggingEvent)
		{
			if (loggingEvent.Level == Level.Fatal)
			{
				ReportError(RenderLoggingEvent(loggingEvent));
			}
		}

		private static void ReportError(string errorMessage)
		{
			// we only want to write the first error into the system error file
			if (errorReportingDone)
			{
				return;
			}

			errorReportingDone = true;

			try
			{
				WriteErrorLog(UgpSystemErrorFileName);
				WriteErrorLog(AscentSystemErrorFileName);
			}
			catch (Exception e)
			{
				Log.Instance.Warn("Ignoring writing system error file throws exception", e);
			}

			try
			{
				Core.ErrorHandler.ReportError(errorMessage);
			}
			catch (Exception e)
			{
				Log.Instance.Warn("Ignoring error event throws exception", e);
			}

			if (!Application.isEditor)
			{
				Log.Instance.Warn("Exiting Application");
				Application.Quit(42, errorMessage);
			}
			else
			{
				Debug.LogError(errorMessage);
			}

			void WriteErrorLog(string filename)
			{
				using var tf = File.Create(Path.Combine(Factory.ErrorLogDirectory, filename), 1, FileOptions.WriteThrough);
				{
					using var sw = new StreamWriter(tf);
					sw.WriteLine(errorMessage);
				}
			}
		}
	}
}