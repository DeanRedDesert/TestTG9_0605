using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Midas.Tools.Editor
{
	/// <summary>
	/// Editor window to convert textures to PNG. An easy way to tidy up extremely large PSD files
	/// </summary>
	/// <remarks>
	/// Sourced from: https://gist.github.com/yasirkula/407e15017a01324a9ea95bf1bdea72f7
	/// </remarks>
	public sealed class ConvertTexturesToPng : EditorWindow
	{
		private const string DummyTexturePath = "Assets/convert_dummyy_texturee.png";
		private const bool RemoveMatteFromPsdByDefault = true;

		private readonly GUIContent[] maxTextureSizeStrings = { new GUIContent("32"), new GUIContent("64"), new GUIContent("128"), new GUIContent("256"), new GUIContent("512"), new GUIContent("1024"), new GUIContent("2048"), new GUIContent("4096"), new GUIContent("8192"), new GUIContent("16384") };
		private readonly int[] maxTextureSizeValues = { 32, 64, 128, 256, 512, 1024, 2048, 4096, 8192, 16384 };

		private readonly GUIContent rootPathContent = new GUIContent("Root Path:", "Textures inside this folder (recursive) will be converted");
		private readonly GUIContent textureExtensionsContent = new GUIContent("Textures to Convert:", "Only Textures with these extensions will be converted (';' separated)");
		private readonly GUIContent excludedDirectoriesContent = new GUIContent("Excluded Directories:", "Textures inside these directories won't be converted (';' separated)");
		private readonly GUIContent keepOriginalFilesContent = new GUIContent("Keep Original Files:", "If selected, original Texture files won't be deleted after the conversion");
		private readonly GUIContent maxTextureSizeContent = new GUIContent("Max Texture Size:", "Textures larger than this size will be downscaled to this size");
		private readonly GUILayoutOption glWidth25 = GUILayout.Width(25f);

		private string rootPath = "";
		private string textureExtensions = ".tga;.psd;.tiff;.tif;.bmp";
		private string excludedDirectories = "";
		private bool keepOriginalFiles;
		private int maxTextureSize = 8192;

		private Vector2 scrollPos;

		[MenuItem("Midas/Tools/Convert Textures to PNG")]
		private static void Init()
		{
			var window = GetWindow<ConvertTexturesToPng>();
			window.titleContent = new GUIContent("Convert to PNG");
			window.minSize = new Vector2(285f, 160f);
			window.Show();
		}

		private void OnEnable()
		{
			// By default, Root Path points to this project's Assets folder
			if (string.IsNullOrEmpty(rootPath))
				rootPath = Application.dataPath;
		}

		private void OnGUI()
		{
			scrollPos = GUILayout.BeginScrollView(scrollPos);

			rootPath = PathField(rootPathContent, rootPath, true, "Choose target directory");
			textureExtensions = EditorGUILayout.TextField(textureExtensionsContent, textureExtensions);
			excludedDirectories = EditorGUILayout.TextField(excludedDirectoriesContent, excludedDirectories);
			keepOriginalFiles = EditorGUILayout.Toggle(keepOriginalFilesContent, keepOriginalFiles);
			maxTextureSize = EditorGUILayout.IntPopup(maxTextureSizeContent, maxTextureSize, maxTextureSizeStrings, maxTextureSizeValues);

			EditorGUILayout.Space();

			// Convert Textures to PNG
			if (GUILayout.Button("Convert to PNG!"))
			{
				var startTime = EditorApplication.timeSinceStartup;

				var convertedPaths = new List<string>(128);
				long originalTotalSize = 0L, convertedTotalSize = 0L;

				try
				{
					rootPath = rootPath.Trim();
					excludedDirectories = excludedDirectories.Trim();
					textureExtensions = textureExtensions.ToLowerInvariant().Replace(".png", "").Trim();

					if (rootPath.Length == 0)
						rootPath = Application.dataPath;

					var paths = FindTexturesToConvert();
					var pathsLengthStr = paths.Length.ToString();
					var progressMultiplier = paths.Length > 0 ? 1f / paths.Length : 1f;

					CreateDummyTexture(); // Dummy Texture is used while reading Textures' pixels

					for (var i = 0; i < paths.Length; i++)
					{
						if (EditorUtility.DisplayCancelableProgressBar("Please wait...", string.Concat("Converting: ", (i + 1).ToString(), "/", pathsLengthStr), (i + 1) * progressMultiplier))
							throw new Exception("Conversion aborted");

						var pngFile = Path.ChangeExtension(paths[i], ".png");
						var pngMetaFile = pngFile + ".meta";
						var originalMetaFile = paths[i] + ".meta";

						var isPsdImage = Path.GetExtension(paths[i]).ToLowerInvariant() == ".psd";

						// Make sure to respect PSD assets' "Remove Matte (PSD)" option
						if (isPsdImage)
						{
							var removeMatte = RemoveMatteFromPsdByDefault;

							if (File.Exists(originalMetaFile))
							{
								const string removeMatteOption = "pSDRemoveMatte: ";

								var metaContents = File.ReadAllText(originalMetaFile);
								var removeMatteIndex = metaContents.IndexOf(removeMatteOption, StringComparison.Ordinal);
								if (removeMatteIndex >= 0)
									removeMatte = metaContents[removeMatteIndex + removeMatteOption.Length] != '0';
							}

							var removeMatteProp = new SerializedObject(AssetImporter.GetAtPath(DummyTexturePath)).FindProperty("m_PSDRemoveMatte");
							if (removeMatteProp != null && removeMatteProp.boolValue != removeMatte)
							{
								removeMatteProp.boolValue = removeMatte;
								removeMatteProp.serializedObject.ApplyModifiedPropertiesWithoutUndo();
							}
						}

						// Temporarily copy the image file to Assets folder to create a read-write enabled Texture from it
						File.Copy(paths[i], DummyTexturePath, true);
						AssetDatabase.ImportAsset(DummyTexturePath, ImportAssetOptions.ForceUpdate);

						// Convert the Texture to PNG and save it
						var pngBytes = AssetDatabase.LoadAssetAtPath<Texture2D>(DummyTexturePath).EncodeToPNG();
						File.WriteAllBytes(pngFile, pngBytes);

						originalTotalSize += new FileInfo(paths[i]).Length;
						convertedTotalSize += new FileInfo(pngFile).Length;

						// If .meta file exists, copy it to PNG image
						if (File.Exists(originalMetaFile))
						{
							File.Copy(originalMetaFile, pngMetaFile, true);

							// Try changing original meta file's GUID to avoid collisions with PNG (Credit: https://gist.github.com/ZimM-LostPolygon/7e2f8a3e5a1be183ac19)
							if (keepOriginalFiles)
							{
								var metaContents = File.ReadAllText(originalMetaFile);
								var guidIndex = metaContents.IndexOf("guid: ", StringComparison.Ordinal);
								if (guidIndex >= 0)
								{
									var guid = metaContents.Substring(guidIndex + 6, 32);
									var newGuid = Guid.NewGuid().ToString("N");
									metaContents = metaContents.Replace(guid, newGuid);
									File.WriteAllText(originalMetaFile, metaContents);
								}
							}

							// Don't show "Remote Matte (PSD)" option for converted Textures
							if (isPsdImage)
							{
								var metaContents = File.ReadAllText(pngMetaFile);
								var modifiedMeta = false;

								if (metaContents.Contains("pSDShowRemoveMatteOption: 1"))
								{
									metaContents = metaContents.Replace("pSDShowRemoveMatteOption: 1", "pSDShowRemoveMatteOption: 0");
									modifiedMeta = true;
								}

								if (metaContents.Contains("pSDRemoveMatte: 1"))
								{
									metaContents = metaContents.Replace("pSDRemoveMatte: 1", "pSDRemoveMatte: 0");
									modifiedMeta = true;
								}

								if (modifiedMeta)
									File.WriteAllText(pngMetaFile, metaContents);
							}
						}

						if (!keepOriginalFiles)
						{
							File.Delete(paths[i]);

							if (File.Exists(originalMetaFile))
								File.Delete(originalMetaFile);
						}

						convertedPaths.Add(paths[i]);
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
				finally
				{
					EditorUtility.ClearProgressBar();

					if (File.Exists(DummyTexturePath))
						AssetDatabase.DeleteAsset(DummyTexturePath);

					// Force Unity to import PNG images (otherwise we'd have to minimize Unity and then maximize it)
					AssetDatabase.Refresh();

					// Print information to Console
					var sb = new StringBuilder(100 + convertedPaths.Count * 75);
					sb.Append("Converted ").Append(convertedPaths.Count).Append(" Texture(s) to PNG in ").Append((EditorApplication.timeSinceStartup - startTime).ToString("F2")).Append(" seconds (").Append(EditorUtility.FormatBytes(originalTotalSize)).Append(" -> ").Append(EditorUtility.FormatBytes(convertedTotalSize));

					sb.AppendLine("):");
					for (var i = 0; i < convertedPaths.Count; i++)
						sb.Append("- ").AppendLine(convertedPaths[i]);

					Debug.Log(sb.ToString());
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

		private string[] FindTexturesToConvert()
		{
			var texturePaths = new HashSet<string>();
			var targetExtensions = new HashSet<string>(textureExtensions.Split(';'));

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
				// Only process filtered image files
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
						texturePaths.Add(allFiles[i]);
				}
			}

			var result = new string[texturePaths.Count];
			texturePaths.CopyTo(result);

			return result;
		}

		// Creates dummy Texture asset that will be used to read Textures' pixels
		private void CreateDummyTexture()
		{
			if (!File.Exists(DummyTexturePath))
			{
				File.WriteAllBytes(DummyTexturePath, new Texture2D(2, 2).EncodeToPNG());
				AssetDatabase.ImportAsset(DummyTexturePath, ImportAssetOptions.ForceUpdate);
			}

			var textureImporter = AssetImporter.GetAtPath(DummyTexturePath) as TextureImporter;
			textureImporter!.maxTextureSize = maxTextureSize;
			textureImporter.isReadable = true;
			textureImporter.filterMode = FilterMode.Point;
			textureImporter.mipmapEnabled = false;
			textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
			textureImporter.npotScale = TextureImporterNPOTScale.None;
			textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			textureImporter.SaveAndReimport();
		}
	}
}