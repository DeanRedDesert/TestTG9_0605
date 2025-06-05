using System.Collections.Generic;
using System.IO;
using IGT.Game.Core.Presentation.PeripheralLights.Streaming;
using UnityEngine;

namespace Midas.Ascent.Cabinet.Lights
{
	/// <summary>
	/// Reads and writes an index of sequence files to be used for choreography playback.
	/// </summary>
	/// <remarks>
	/// The index is created during the build process to improve runtime speed of looking up sequence files.
	/// </remarks>
	public static class LightSequenceIndex
	{
		/// <summary>
		/// Name of the EGM Resources directory that all light files are contained in.
		/// </summary>
		private const string EgmResources = "EGMResources";

		/// <summary>
		/// Loads a dictionary of sequence files that can be referenced at runtime.
		/// </summary>
		/// <param name="gameMountPoint">Absolute path of the root game directory (mount point).</param>
		/// <returns>
		/// A dictionary mapping sequence names to their file names.
		/// If there is an error, returns an empty dictionary.
		/// </returns>
		public static Dictionary<string, string> LoadIndex(string gameMountPoint)
		{
			Dictionary<string, string> sequenceFiles = null;

			// IGTA Loading sequence files directly instead of from an index file
			if (!string.IsNullOrEmpty(gameMountPoint))
			{
				var egmResourcesPath = Path.Combine(gameMountPoint, EgmResources);

				if (Directory.Exists(egmResourcesPath))
				{
					sequenceFiles = DiscoverSequences(egmResourcesPath, gameMountPoint);
				}
				else
				{
					Debug.LogWarning("Cannot locate path for light sequences: " + egmResourcesPath);
				}
			}

			return sequenceFiles ?? new Dictionary<string, string>();
		}

		private static Dictionary<string, string> DiscoverSequences(string egmResourcesPath, string buildDirectory)
		{
			var sequenceFiles = new Dictionary<string, string>();
			var sequenceFileNames = Directory.GetFiles(egmResourcesPath,
				"*.xlightseq",
				SearchOption.AllDirectories);

			foreach (var sequenceFileName in sequenceFileNames)
			{
				var sequenceName = LightSequence.GetNameFromFile(sequenceFileName);

				if (!sequenceFiles.TryGetValue(sequenceName, out var file))
				{
					sequenceFiles[sequenceName] = MakeRelative(buildDirectory, sequenceFileName);
				}
				else
				{
					Log.Instance.Warn($"The following sequence has already been loaded: {sequenceName} ({sequenceFileName}).\n" + $"The following file will be used for choreography: {file}.");
				}
			}

			return sequenceFiles;
		}

		private static string MakeRelative(string rootDirectory, string absolutePath)
		{
			rootDirectory = Path.GetFullPath(rootDirectory);
			return Path.GetFullPath(absolutePath)
				.Replace(rootDirectory, string.Empty)
				.Trim('\\');
		}
	}
}