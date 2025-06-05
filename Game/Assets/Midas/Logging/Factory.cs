using System;
using System.IO;
using System.Xml;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Util.TypeConverters;

namespace Midas.Logging
{
	public static class Factory
	{
		#region Fields

		private static string logConfig;
		private static int initCount;

		#endregion

		#region Properties

		public static bool IsInitialized => initCount > 0;
		public static string LogDirectory { get; private set; } = string.Empty;
		public static string ErrorLogDirectory { get; private set; } = string.Empty;

		#endregion

		#region Public Methods

		public static Logger GetLogger(Type type)
		{
			return new Logger(type.Namespace);
		}

		public static void InitLogging()
		{
			InitLogging(string.Empty, Directory.GetCurrentDirectory(), Directory.GetCurrentDirectory());
		}

		public static void InitLogging(string logConfig, string logFolder, string errorLogFolder)
		{
			if (initCount++ != 0)
			{
				Log.Instance.InfoFormat("Logging init already done, initCount={0}'", initCount);
				return;
			}

			Factory.logConfig = logConfig;
			LogDirectory = logFolder ?? Directory.GetCurrentDirectory();
			ErrorLogDirectory = errorLogFolder ?? Directory.GetCurrentDirectory();

			CreateLogDirectories();
			InitLog4Net();
		}

		public static void InitLoggingForUnitTesting()
		{
			var appender = new DebugAppender { Threshold = Level.All };
			BasicConfigurator.Configure(appender);
		}

		public static void DeInitLogging()
		{
			if (--initCount == 0)
			{
				LogManager.Shutdown();
			}
		}

		#endregion

		#region Private Methods

		private static void InitLog4Net()
		{
			if (logConfig != null)
			{
				ConverterRegistry.AddConverter(typeof(DirectoryPatternString), typeof(DirectoryPatternStringConverter));
				var document = new XmlDocument();
				try
				{
					document.LoadXml(logConfig);
					XmlConfigurator.Configure(document.DocumentElement);
				}
				catch (Exception e)
				{
					Console.WriteLine($"Parsing log4netConfiguration failed with Exception {e}.");
					Console.WriteLine($"log4netConfiguration={logConfig}");
					CreateBasicConfiguration();
				}
			}
			else
			{
				CreateBasicConfiguration();
			}
		}

		private static void CreateLogDirectories()
		{
			try
			{
				if (!string.IsNullOrEmpty(ErrorLogDirectory) && !Directory.Exists(ErrorLogDirectory))
				{
					Directory.CreateDirectory(ErrorLogDirectory);
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"CreateDirectory exception {e} for ErrorLogDirectory={ErrorLogDirectory}");
			}

			try
			{
				if (!string.IsNullOrEmpty(LogDirectory) && !Directory.Exists(LogDirectory))
				{
					Directory.CreateDirectory(LogDirectory);
				}
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"CreateDirectory exception {e} for LogDirectory={LogDirectory}");
			}
		}

		private static void CreateBasicConfiguration()
		{
			const string config = @"
<log4net>
  <appender name=""ConsoleAppender"" type=""log4net.Appender.ConsoleAppender"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""[%date] %-5level [%thread] [%logger].%type{1}.%method:%line - %message%newline"" />
    </layout>
  </appender>
  <root>
    <level value=""ALL"" />
    <appender-ref ref=""ConsoleAppender"" />
  </root>
</log4net>
";
			var document = new XmlDocument();
			document.LoadXml(config);
			XmlConfigurator.Configure(document.DocumentElement);
		}

		#endregion
	}
}