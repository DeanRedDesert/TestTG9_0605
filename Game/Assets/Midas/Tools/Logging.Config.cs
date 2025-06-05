namespace Midas.Tools
{
	public static partial class Logging
	{
		/// <summary>
		/// Editor config
		/// </summary>
		private const string Config =
#if UNITY_EDITOR
@"<log4net>

	<appender name=""UnityConsole"" type=""Midas.Presentation.Logging.UnityConsoleAppender"">
		<layout type=""log4net.Layout.PatternLayout"">
			<conversionPattern value=""%date{fff}ms, %-5level [%logger].%type{1}.%method:%line - %message""/>
		</layout>
		<filter type=""log4net.Filter.LevelRangeFilter"">
			<levelMin value=""WARN"" />
			<levelMax value=""EMERGENCY"" />
		</filter>
	</appender>

	<appender name=""FileAppender"" type=""log4net.Appender.RollingFileAppender,log4net"">
		<file value=""%cs{logDirectory}/Game.log"" type=""Midas.Logging.DirectoryPatternString""/>
		<appendToFile value=""true""/>
		<rollingStyle value=""Size""/>
		<maxSizeRollBackups value=""0""/>
		<maximumFileSize value=""10MB""/>
		<staticLogFileName value=""true""/>
		<layout type=""log4net.Layout.PatternLayout"">
			<conversionPattern value=""[%date] %-5level [%thread] [%logger].%type{1}.%method:%line - %message%newline""/>
		</layout>
	</appender>

	<appender name=""PlatformFatalError"" type=""Midas.Logging.FatalErrorHandler"">
		<layout type=""log4net.Layout.PatternLayout"">
			<conversionPattern value=""[%date] %-5level [%thread] [%logger].%type{1}.%method:%line - %message""/>
		</layout>
	</appender>

	<root>
		<level value=""ALL""/>
		<appender-ref ref=""FileAppender""/>
		<appender-ref ref=""PlatformFatalError""/>
		<appender-ref ref=""UnityConsole""/>
	</root>

</log4net>";
#elif DEBUG
@"<log4net>

  <appender name=""RollingFileTXT"" type=""Midas.Logging.AsyncRollingFileAppender"">
    <file value=""%cs{logDirectory}/Game.log"" type=""Midas.Logging.DirectoryPatternString"" />
    <appendToFile value=""true"" />
    <rollingStyle value=""Size"" />
    <maxSizeRollBackups value=""4"" />
    <maximumFileSize value=""10MB"" />
    <staticLogFileName value=""true"" />
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""[%date] %-5level [%thread] [%logger].%type{1}.%method:%line - %message%newline"" />
    </layout>
  </appender>

  <appender name=""PlatformFatalError"" type=""Midas.Logging.FatalErrorHandler"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""[%date] %-5level [%thread] [%logger].%type{1}.%method:%line - %message"" />
    </layout>
  </appender>

  <root>
    <level value=""ALL"" />
    <appender-ref ref=""RollingFileTXT"" />
    <appender-ref ref=""PlatformFatalError"" />
  </root>

</log4net>";
#else
@"<log4net>

  <appender name=""RollingFileTXT"" type=""Midas.Logging.AsyncRollingFileAppender"">
    <file value=""%cs{logDirectory}/Game.log"" type=""Midas.Logging.DirectoryPatternString"" />
    <appendToFile value=""false"" />
    <rollingStyle value=""Size"" />
    <maxSizeRollBackups value=""4"" />
    <maximumFileSize value=""512KB"" />
    <staticLogFileName value=""true"" />
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""[%date] %-5level [%thread] [%logger].%type{1}.%method:%line - %message%newline"" />
    </layout>
  </appender>

  <appender name=""PlatformFatalError"" type=""Midas.Logging.FatalErrorHandler"">
    <layout type=""log4net.Layout.PatternLayout"">
      <conversionPattern value=""[%date] %-5level [%thread] [%logger].%type{1}.%method:%line - %message"" />
    </layout>
  </appender>

  <root>
    <level value=""INFO"" />
    <appender-ref ref=""RollingFileTXT"" />
    <appender-ref ref=""PlatformFatalError"" />
  </root>

</log4net>";
#endif
	}
}