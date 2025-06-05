using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace Midas.Ascent.Editor
{
	[Serializable]
	public sealed class NameValue
	{
		public string Name { get; set; }
		public string Value { get; set; }

		public NameValue()
			: this("<none>", "")
		{
		}

		public NameValue(string name, string value)
		{
			Name = name;
			Value = value;
		}
	}

	[Serializable]
	public sealed class GenerateSettings
	{
		private const string Path = "GenerateRegistrySettings.xml";

		public int SelectedMultiGameConfig { get; set; }
		public NameValue[] SelectedSingleGameOptions { get; set; } = Array.Empty<NameValue>();

		/// <summary>
		/// Loads generate registry settings from file.
		/// </summary>
		/// <returns>GenerateSettings object which has been loaded.</returns>
		public static GenerateSettings Load()
		{
			// If the file does exist, load it.
			if (File.Exists(Path))
			{
				using var file = File.OpenRead(Path);
				return new XmlSerializer(typeof(GenerateSettings)).Deserialize(file) as GenerateSettings;
			}

			// If the file does not exist. Just create a new one with default values.
			var settings = new GenerateSettings();
			Save(settings);
			return settings;
		}

		/// <summary>
		/// Save the settings out to disk.
		/// </summary>
		/// <param name="settings">Settings data to save.</param>
		public static void Save(GenerateSettings settings)
		{
			if (File.Exists(Path) && new FileInfo(Path).IsReadOnly)
			{
				Debug.LogWarning("GenerateSettings file is read only so it cannot be saved.");
			}
			else
			{
				using var streamWriter = new StreamWriter(Path, false, Encoding.Unicode);
				new XmlSerializer(typeof(GenerateSettings)).Serialize(streamWriter, settings);
			}
		}
	}
}