using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using IGT.Game.Core.Communication.Standalone.Schemas;

namespace Midas.Ascent.Editor
{
	public static class SystemConfigurationHelper
	{
		private const string SystemConfigFile = "SystemConfig.xml";
		private const string SystemConfigurationDirectory = "Temp\\SystemConfiguration";

		public static SystemConfigurations Load()
		{
			var ser = new XmlSerializer(typeof(SystemConfigurations));

			using var xmlReader = XmlReader.Create(SystemConfigFile);
			var systemConfigurations = (SystemConfigurations)ser.Deserialize(xmlReader);
			return systemConfigurations;
		}

		public static void Save(SystemConfigurations systemConfigurations)
		{
			var ser = new XmlSerializer(typeof(SystemConfigurations));
			var settings = new XmlWriterSettings
			{
				Encoding = new UTF8Encoding(false),
				Indent = true
			};

			using (var xmlWriter = XmlWriter.Create(SystemConfigFile, settings))
			{
				ser.Serialize(xmlWriter, systemConfigurations);
				xmlWriter.Flush();
				xmlWriter.Close();
			}

			var systemConfigEditorFile = Path.Combine(SystemConfigurationDirectory, SystemConfigFile);

			if (File.Exists(SystemConfigFile))
			{
				Directory.CreateDirectory(SystemConfigurationDirectory);
				File.Copy(SystemConfigFile, systemConfigEditorFile, true);
			}
		}
	}
}