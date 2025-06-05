using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using IGT.Game.Fuel.Data;
using IGT.Game.Fuel.Data.TranslationTable;
using Ionic.Zip;
using Midas.Fuel.Editor.Screenshot;

namespace Midas.Fuel.Editor
{
	/// <summary>
	/// Translation data from a game, which can be used to synchronize translation data without access to the fuel database.
	/// </summary>
	public static class FuelPackageGenerator
	{
		#region Constants

		/// <summary>
		/// Name of translation information data file which will be embedded in the fuel package.
		/// </summary>
		private const string TranslationDataFileName = "TranslationData.xml";

		/// <summary>
		/// Fuel package file name format string, used when generating the fuel package file.
		/// </summary>
		private const string FuelPackageNameFormat = "{0}.fuelpackage";

		#endregion

		#region Static API

		/// <summary>
		/// Load a translation table into memory from a translation table asset file.
		/// </summary>
		/// <param name="path">Path to the fuel translation data file to load.</param>
		/// <returns>Fuel translation object which has been loaded.</returns>
		/// <exception cref="ArgumentException">
		/// Thrown if the passed <paramref name="path"/> string is null or empty.
		/// </exception>
		public static FuelPackage Load(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Sync Data File Path cannot be null or empty.", nameof(path));
			}

			FuelPackage loadedTranslations = null;

			if (File.Exists(path))
			{
				using var dataFile = File.Open(path, FileMode.Open);
				loadedTranslations = Load(dataFile);
				dataFile.Close();
			}

			return loadedTranslations;
		}

		/// <summary>
		/// Save a translation table out to disk.
		/// </summary>
		/// <param name="path">Path on disk to save the translation table.</param>
		/// <param name="translationData">Fuel translation data to save.</param>
		public static void Save(string path, FuelPackage translationData)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("File path cannot be null or empty.", nameof(path));
			}

			if (translationData == null)
			{
				throw new ArgumentNullException(nameof(translationData), "Cannot save null translation data file.");
			}

			var directory = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			// Serialize the translation data into a memory stream
			// This way we don't have to write the deserialize code twice.
			var bytes = Save(translationData);
			// Write the memory stream to a file.
			// It appears there is a much better way to do this in .NET 4.5 using MemoryStream.WriteTo()
			using var outputStream = File.Open(path, FileMode.Create, FileAccess.Write);
			outputStream.Write(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Open a fuel translation package and extract the translation data.
		/// </summary>
		/// <param name="path">Path to the package which should be loaded.</param>
		/// <returns>Translation Data object extracted from the package.</returns>
		public static FuelPackage OpenPackage(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				throw new ArgumentException("Path cannot be null or empty.", nameof(path));
			}

			if (!File.Exists(path))
			{
				throw new ArgumentException("Passed file does not exist.", nameof(path));
			}

			FuelPackage data = null;

			// Open the fuel package.
			using (var zip = ZipFile.Read(path))
			{
				// Find the translation data file
				// This allows us to only extract the one file we are going to use, just in case the package contains more than this file.
				var translationFile = zip.FirstOrDefault(entry => entry.FileName.Contains(TranslationDataFileName));

				// Make sure we found it.
				if (translationFile != null)
				{
					// Extract the data file into a memory stream.
					// This way the data file doesn't need to be written to disk as a separate file from the package
					// to import the data.
					using var memoryStream = new MemoryStream();
					translationFile.Extract(memoryStream);

					// Yay we conjured some data!
					data = Load(memoryStream);
				}
			}

			if (data != null)
			{
				return data;
			}

			throw new ArgumentException("Passed file does not contain a valid translation data file.", nameof(path));
		}

		#endregion

		#region FuelPackage Extension Methods

		private static SyncData SyncData { get; set; }

		/// <summary>
		/// Generate a fuel translation package based on the passed in data.
		/// </summary>
		/// <param name="packageInfo">Package information, used for storing the .xml file.</param>
		/// <param name="newTranslationInformation">Screenshot information, used to get screenshot images.</param>
		/// <param name="syncData"></param>
		public static IEnumerator GeneratePackage(this FuelPackage packageInfo, IList<TranslationInformation> newTranslationInformation, SyncData syncData)
		{
			var screenshotPaths = new List<string>();
			SyncData = syncData;

			// It is OK for screenshot info to be null, just don't package any screenshots.
			if (newTranslationInformation != null)
			{
				// Get the actual paths to all images we are going to use.
				// Need the actual paths so they can be added to the fuel package.
				foreach (var translationInformation in packageInfo.Translations)
				{
					var information = translationInformation;
					var translationString = information.ToString();
					SyncData.Status = "calculating screenshot paths.";
					SyncData.Progress = 100 * packageInfo.Translations.IndexOf(information) / packageInfo.Translations.Count;

					//Update SyncStatus
					yield return null;

					if (!string.IsNullOrEmpty(information.ResourceLocation))
					{
						var paths = from newTranslation in newTranslationInformation
							where (newTranslation.GameClientId == information.GameClientId || newTranslation.ToString() == translationString) &&
								newTranslation.Language == information.Language &&
								!string.IsNullOrEmpty(newTranslation.ResourceLocation)
							select ScreenshotUtilities.GetLocalPathFromDatabasePath(newTranslation.ResourceLocation);
						screenshotPaths.AddRange(paths);
					}
				}

				screenshotPaths = screenshotPaths.Distinct().ToList();
			}

			var packageFileName = string.Format(FuelPackageNameFormat, packageInfo.GameThemeId);

			if (File.Exists(packageFileName))
			{
				File.Delete(packageFileName);
			}

			using var zip = new ZipFile(packageFileName);
			// serialize the translation data out to a stream.
			// This way the data isn't written to disk until it is already in the package.
			var translationDataStream = Save(packageInfo);
			// Add the stream to the zip package using the data file name.
			zip.AddEntry(TranslationDataFileName, translationDataStream);

			for (var index = 0; index < screenshotPaths.Count; index++)
			{
				SyncData.Status = $"Adding {screenshotPaths[index]} to fuelpackage.";
				SyncData.Progress = 100 * index / screenshotPaths.Count;

				// Update SyncStatus
				yield return null;

				// Pass empty string to flatten the directories.
				zip.AddFile(screenshotPaths[index], "");
			}

			SyncData.Status = "Saving fuel package.";
			SyncData.Progress = 0;

			// Allow updated sync data to be displayed.
			yield return null;

			zip.Save();
		}

		#endregion

		/// <summary>
		/// Load the standalone sync data from a stream.
		/// </summary>
		/// <param name="stream">Stream to load the data from.</param>
		/// <returns>Loaded translation data.</returns>
		private static FuelPackage Load(Stream stream)
		{
			FuelPackage loadedTranslations = null;

			if (stream != null)
			{
				stream.Position = 0;
				// Load the translation table.
				var contractSerializer = new DataContractSerializer(typeof(FuelPackage));

				loadedTranslations = contractSerializer.ReadObject(stream) as FuelPackage;

				stream.Close();
			}

			return loadedTranslations;
		}

		/// <summary>
		/// Save a translation data object out to a stream.
		/// </summary>
		/// <param name="translationData">Translation Data to Save.</param>
		/// <returns>Stream containing serialized translation data.</returns>
		private static byte[] Save(FuelPackage translationData)
		{
			var outputStream = new MemoryStream();

			var contractSerializer = new DataContractSerializer(typeof(FuelPackage));
			contractSerializer.WriteObject(outputStream, translationData);
			outputStream.Position = 0;

			return outputStream.ToArray();
		}
	}
}