using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Midas.Tools.Humanize;
using UnityEditor;
using UnityEngine;

namespace Midas.Tools.Editor
{
	public sealed class SanitizeFileNames : EditorWindow
	{
		private readonly GUIContent rootPathContent = new GUIContent("Root Path:", "Files inside this folder (recursive) will be converted");
		private readonly GUIContent fileExtensionsContent = new GUIContent("Files to Convert:", "Only Files with these extensions will be converted (';' separated)");
		private readonly GUIContent excludedDirectoriesContent = new GUIContent("Excluded Directories:", "Files inside these directories won't be converted (';' separated)");
		private readonly GUIContent allowedSuffixesContent = new GUIContent("Allowed Suffixes:", "These file name suffixes will be left alone (';' separated)");
		private readonly GUILayoutOption glWidth25 = GUILayout.Width(25f);

		private string rootPath = "";
		private string fileExtensions = ".png;.wav;.ogg";
		private string excludedDirectories = "Assets/Plugins;Assets/Game/Information/CorePages;Assets/Game/Information/Resources";
		private string allowedSuffixes = "_es;_en;_cz";

		private Vector2 scrollPos;

		[MenuItem("Midas/Tools/Sanitize File Names")]
		private static void Init()
		{
			var window = GetWindow<SanitizeFileNames>();
			window.titleContent = new GUIContent("Sanitize File Names");
			window.minSize = new Vector2(285f, 160f);
			window.Show();
		}

		private void OnGUI()
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);

			rootPath = PathField(rootPathContent, rootPath, true, "Choose target directory");
			fileExtensions = EditorGUILayout.TextField(fileExtensionsContent, fileExtensions);
			excludedDirectories = EditorGUILayout.TextField(excludedDirectoriesContent, excludedDirectories);
			allowedSuffixes = EditorGUILayout.TextField(allowedSuffixesContent, allowedSuffixes);
			EditorGUILayout.Space();

			// Convert Textures to PNG

			var go = false;
			var dryRun = false;
			using (new EditorGUILayout.HorizontalScope())
			{
				go = GUILayout.Button("Rename");
				dryRun = GUILayout.Button("Dry Run");
			}

			if (go || dryRun)
			{
				try
				{
					rootPath = rootPath.Trim().Replace('/', '\\');
					excludedDirectories = excludedDirectories.Trim();
					allowedSuffixes = allowedSuffixes.Trim();

					var suffixes = allowedSuffixes.Split(';');

					if (rootPath.Length == 0)
						rootPath = Application.dataPath;

					var paths = FindFilesToRename();
					var pathsLengthStr = paths.Length.ToString();
					var progressMultiplier = paths.Length > 0 ? 1f / paths.Length : 1f;

					for (var i = 0; i < paths.Length; i++)
					{
						if (EditorUtility.DisplayCancelableProgressBar("Please wait...", string.Concat("Renaming: ", (i + 1).ToString(), "/", pathsLengthStr), (i + 1) * progressMultiplier))
							throw new Exception("Conversion aborted");

						var originalFileName = Path.GetFileNameWithoutExtension(paths[i]);
						var ext = Path.GetExtension(paths[i]);

						var suffix = suffixes.FirstOrDefault(originalFileName.EndsWith);
						var preSanitize = suffix == null ? originalFileName : originalFileName.Substring(0, originalFileName.Length - suffix.Length);

						var newFileName = preSanitize.Dehumanize();
						if (suffix != null)
							newFileName += suffix;

						if (newFileName == originalFileName)
							continue;

						newFileName += ext;
						Debug.Log($"Renaming {paths[i]} => {newFileName}");

						if (!dryRun)
						{
							DoFileRename(paths[i], newFileName);
							DoFileRename(paths[i] + ".meta", newFileName + ".meta");
						}
					}

					if (!dryRun)
						AssetDatabase.Refresh();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
				finally
				{
					EditorUtility.ClearProgressBar();
				}

				void DoFileRename(string path, string newName)
				{
					if (!File.Exists(path))
					{
						Debug.LogWarning($"Unable to rename \"{path}\": \"{path}\" does not exist");
						return;
					}

					var newFile = Path.Combine(Path.GetDirectoryName(path), newName);
					if (path.ToLowerInvariant() == newFile.ToLowerInvariant())
					{
						// Windows treats the files as the same so you can't rename in one step

						var intermediateFile = newFile + Guid.NewGuid().GetHashCode();
						File.Move(path, intermediateFile);
						File.Move(intermediateFile, newFile);
						return;
					}

					if (File.Exists(newFile))
					{
						Debug.LogWarning($"Unable to rename \"{path}\" to \"{newFile}\": \"{newFile}\" already exists");
						return;
					}

					File.Move(path, newFile);
				}
			}

			GUILayout.EndScrollView();
		}

		private string PathField(GUIContent label, string path, bool isDirectory, string panelTitle, GUIContent downloadURL = null)
		{
			GUILayout.BeginHorizontal();
			path = EditorGUILayout.TextField(label, path);
			if (GUILayout.Button("o", glWidth25))
			{
				var selectedPath = isDirectory ? EditorUtility.OpenFolderPanel(panelTitle, "", "") : EditorUtility.OpenFilePanel(panelTitle, "", "exe");
				if (!string.IsNullOrEmpty(selectedPath))
					path = selectedPath;

				// Remove focus from active text field

				GUIUtility.keyboardControl = 0;
			}

			if (downloadURL != null && GUILayout.Button(downloadURL, glWidth25))
				Application.OpenURL(downloadURL.tooltip);
			GUILayout.EndHorizontal();

			return path;
		}

		private string[] FindFilesToRename()
		{
			var filePaths = new HashSet<string>();
			var targetExtensions = new HashSet<string>(fileExtensions.Split(';'));

			// Get directories to exclude
			var excludedPaths = excludedDirectories.Split(';');
			for (var i = 0; i < excludedPaths.Length; i++)
			{
				excludedPaths[i] = excludedPaths[i].Trim();
				if (excludedPaths[i].Length == 0)
					excludedPaths[i] = "NULL/";
				else
				{
					excludedPaths[i] = Path.GetFullPath(excludedPaths[i]);

					// Make sure excluded directory paths end with directory separator char
					if (Directory.Exists(excludedPaths[i]) && !excludedPaths[i].EndsWith(Path.DirectorySeparatorChar.ToString()))
						excludedPaths[i] += Path.DirectorySeparatorChar;
				}
			}

			// Iterate through all files in Root Path

			var allFiles = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
			for (var i = 0; i < allFiles.Length; i++)
			{
				// Only process filtered files

				if (targetExtensions.Contains(Path.GetExtension(allFiles[i]).ToLowerInvariant()))
				{
					var isExcluded = false;
					if (excludedPaths.Length > 0)
					{
						// Make sure the image file isn't part of an excluded directory
						var fileFullPath = Path.GetFullPath(allFiles[i]);
						for (var j = 0; j < excludedPaths.Length; j++)
						{
							if (fileFullPath.StartsWith(excludedPaths[j]))
							{
								isExcluded = true;
								break;
							}
						}
					}

					if (!isExcluded)
						filePaths.Add(allFiles[i]);
				}
			}

			var result = new string[filePaths.Count];
			filePaths.CopyTo(result);

			return result;
		}
	}
}