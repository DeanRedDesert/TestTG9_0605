using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using Midas.Presentation.Audio;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Audio
{
	[CustomEditor(typeof(SoundDoc))]
	public sealed class SoundDocEditor : UnityEditor.Editor
	{
		private Dictionary<string, string> templateSounds = new Dictionary<string, string>()
		{
			{ @"Assets\Game\Dashboard\Sound\Vol1.wav", "When the volume control is pressed and the volume changes from high volume to low volume." },
			{ @"Assets\Game\Dashboard\Sound\Vol2.wav", "When the volume control is pressed and the volume changes from low volume to middle volume." },
			{ @"Assets\Game\Dashboard\Sound\Vol3.wav", "When the volume control is pressed and the volume changes from middle volume to high volume." },
			{ @"Assets\Game\Dashboard\Sound\VolumePing.ogg", "When the volume slider is released." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_1.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_2.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_3.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_4.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_5.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_6.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_7.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_8.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_9.wav", "When a bet is selected." },
			{ @"Assets\Game\DPP\Sounds\BetSounds\Pick_10.wav", "When a bet is selected." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\ReelStop1.wav", "End of reel 1 stop." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\ReelStop2.wav", "End of reel 2 stop." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\ReelStop3.wav", "End of reel 3 stop." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\ReelStop4.wav", "End of reel 4 stop." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\ReelStop5.wav", "End of reel 5 stop." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\smart_1.wav", "End of reel 1 stop when a feature trigger symbol is visible with the potential to trigger the feature." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\smart_2.wav", "End of reel 2 stop when a feature trigger symbol is visible with the potential to trigger the feature." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\smart_3.wav", "End of reel 3 stop when a feature trigger symbol is visible with the potential to trigger the feature." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\smart_4.wav", "End of reel 4 stop when a feature trigger symbol is visible with the potential to trigger the feature." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\smart_5.wav", "End of reel 5 stop when a feature trigger symbol is visible with the potential to trigger the feature." },
			{ @"Assets\Game\Stages\Common\Reels\Sound\suspend CD.wav", "End of reel 1 stop." },
			{ @"Assets\Game\WinPresentation\Sounds\Rollup_0.wav", "Played when the win celebration level 0 is required." },
			{ @"Assets\Game\WinPresentation\Sounds\RollupLoop_1.wav", "Played when the win celebration level 1 is required." },
			{ @"Assets\Game\WinPresentation\Sounds\RollupLoop_2.wav", "Played when the win celebration level 2 is required." },
			{ @"Assets\Game\WinPresentation\Sounds\RollupLoop_3.wav", "Played when the win celebration level 3 is required." },
			{ @"Assets\Game\WinPresentation\Sounds\RollupLoop_4.wav", "Played when the win celebration level 4 is required." },
			{ @"Assets\Game\WinPresentation\Sounds\RollupLoop_5.wav", "Played when the win celebration level 5 is required." },
			{ @"Assets\Game\WinPresentation\Sounds\RollupTerm_1.wav", "Played when the win celebration level 1 is stopped." },
			{ @"Assets\Game\WinPresentation\Sounds\RollupTerm_2.wav", "Played when the win celebration level 2 is stopped" },
			{ @"Assets\Game\WinPresentation\Sounds\RollupTerm_3.wav", "Played when the win celebration level 3 is stopped" },
			{ @"Assets\Game\WinPresentation\Sounds\RollupTerm_4.wav", "Played when the win celebration level 4 is stopped" },
			{ @"Assets\Game\WinPresentation\Sounds\RollupTerm_5.wav", "Played when the win celebration level 5 is stopped" },
			{ @"Assets\Midas\CreditPlayoff\Presentation\CreditPlayoff\Sounds\Lose.ogg", "Plays at the end of credit playoff feature when the result is negative." },
			{ @"Assets\Midas\CreditPlayoff\Presentation\CreditPlayoff\Sounds\Play_ButtonPress.ogg", "Plays when the 'Play' button for the Credit Playoff is pressed." },
			{ @"Assets\Midas\CreditPlayoff\Presentation\CreditPlayoff\Sounds\spinning.ogg", "Plays during the credit playoff as the dial is spinning." },
			{ @"Assets\Midas\CreditPlayoff\Presentation\CreditPlayoff\Sounds\Text_Appears.ogg", "Plays at the start of the Credit Playoff as the text appears during the introduction." },
			{ @"Assets\Midas\CreditPlayoff\Presentation\CreditPlayoff\Sounds\Wheel_Enters.ogg", "Plays when the Credit Playoff is activated." },
			{ @"Assets\Midas\CreditPlayoff\Presentation\CreditPlayoff\Sounds\Wheel_Exits.ogg", "Plays when the Credit Playoff is deactivated." },
			{ @"Assets\Midas\CreditPlayoff\Presentation\CreditPlayoff\Sounds\Win.ogg", "Plays at the end of credit playoff feature when the result is positive." }
		};

		[MenuItem("Midas/Goto/Sound Document")]
		private static void FocusObject()
		{
			var asset = AssetDatabase.LoadAssetAtPath<SoundDoc>("Assets/Game/Assets/SoundDocData.asset");
			if (asset == null)
			{
				AssetDatabase.CreateAsset(CreateInstance<SoundDoc>(), "Assets/Game/Assets/SoundDocData.asset");
				asset = AssetDatabase.LoadAssetAtPath<SoundDoc>("Assets/Game/Assets/SoundDocData.asset");
			}

			Selection.activeObject = asset;
		}

		public override void OnInspectorGUI()
		{
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Sync"))
				SyncFiles(Array.Empty<SoundDocData>());

			if (GUILayout.Button("Import"))
				Import();

			if (GUILayout.Button("Export"))
				Extract();
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("sounds"), new GUIContent("Sounds"));
		}

		private void SyncFiles(IReadOnlyList<SoundDocData> alternative)
		{
			var existingSoundData = serializedObject.FindProperty("sounds");
			var existing = GetCurrentSoundData(existingSoundData);
			existingSoundData.ClearArray();

			var audioClips = AssetDatabase.FindAssets("t:AudioClip", null);
			var soundFiles = audioClips.Select(AssetDatabase.GUIDToAssetPath).Select(path => path.Replace("/", "\\")).ToList();
			var arrayIndex = 0;
			foreach (var soundFile in soundFiles)
			{
				existingSoundData.InsertArrayElementAtIndex(arrayIndex);
				var ae = existingSoundData.GetArrayElementAtIndex(arrayIndex);

				var pre = existing.FirstOrDefault(e => e.Filename == soundFile);
				var alt = alternative.FirstOrDefault(e => e.Filename == soundFile);
				var use = pre == null || pre.ActivationList == "TODO" ? alt : pre;

				if (use != null)
				{
					SetEntry(ae, soundFile, use.Description, use.Transcript, use.ActivationList);
				}
				else
				{
					if (!templateSounds.TryGetValue(soundFile, out var al))
						al = "TODO";
					SetEntry(ae, soundFile, "Highlight", "NA", al);
				}

				arrayIndex++;
			}

			serializedObject.ApplyModifiedProperties();

			void SetEntry(SerializedProperty ae, string relativeFile, string highlight, string transcript, string activationList)
			{
				var idProperty = ae.FindPropertyRelative("filename");
				idProperty.stringValue = relativeFile;
				idProperty = ae.FindPropertyRelative("description");
				idProperty.stringValue = highlight;
				idProperty = ae.FindPropertyRelative("transcript");
				idProperty.stringValue = transcript;
				idProperty = ae.FindPropertyRelative("activationList");
				idProperty.stringValue = activationList;
			}
		}

		private void Import()
		{
			var path = EditorUtility.OpenFolderPanel("Sound Doc Data to Import", "", "");

			if (string.IsNullOrEmpty(path))
				return;

			var filename = Path.Combine(path.Replace("/", @"\"), @"Game\Assets\Game\Assets\SoundDocData.asset");
			if (!File.Exists(filename))
			{
				EditorUtility.DisplayDialog("Sound Doc to import not found", $"The Sound Document to import can not be found at \n{filename}\nPlease select the root of the game to import from.", "OK");
				return;
			}

			File.Copy(filename, Path.Combine(Application.dataPath.Replace("/", @"\"), @"Game\Assets\SoundDocDataImport.asset"));
			AssetDatabase.Refresh();

			var importedSounds = AssetDatabase.LoadAssetAtPath<SoundDoc>(@"Assets/Game/Assets/SoundDocDataImport.asset");

			SyncFiles(importedSounds.GetSounds());

			AssetDatabase.DeleteAsset("Assets/Game/Assets/SoundDocDataImport.asset");
			AssetDatabase.Refresh();
		}

		private void Extract()
		{
			var existingSoundData = serializedObject.FindProperty("sounds");

			var gameName = GetGameName();
			Directory.CreateDirectory(@"C:\DocGen");
			var soundDataFile = $@"C:\DocGen\{gameName}_SoundInfo_GEN_000.data.txt";
			var outfile = @$"C:\DocGen\{gameName}_SoundInfo_GEN_000.docx";
			var errorOutfile = @$"C:\DocGen\{gameName}_SoundInfo_GEN_000.errors.txt";

			var sounds = Validate(errorOutfile);
			CreateSoundDataFile(soundDataFile);
			CreateSoundDoc();

			File.Delete(soundDataFile);

			if (sounds.HasValidationErrors)
				EditorUtility.DisplayDialog("WARNING - Incomplete sound data.", $"The sound data is not complete. Please review the {errorOutfile} or the UI for list of errors.\n The sound doc has been create including incomplete elements.\n{outfile}", "OK");
			else
				EditorUtility.DisplayDialog("Sound Doc", $"The Sound Document has been created.\n{outfile}", "OK");

			void CreateSoundDoc()
			{
				var baseFolder = Directory.GetParent(Application.dataPath).Parent.FullName;
				var ps1File = Path.Combine(baseFolder, @"Scripts\CreateSoundDoc.ps1");
				var startInfo = new ProcessStartInfo("powershell.exe", $"-NoProfile -ExecutionPolicy unrestricted -file \"{ps1File}\" -soundAsset \"{soundDataFile}\" -soundDoc \"{outfile}\"")
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true
				};

				var test = Process.Start(startInfo);
				test?.WaitForExit();
			}

			(IReadOnlyList<Data> Sounds, bool HasValidationErrors) Validate(string validateFilename)
			{
				var sd = new List<Data>();
				var validationErrors = new List<string>();
				for (var i = 0; i < existingSoundData.arraySize; i++)
				{
					var ae = existingSoundData.GetArrayElementAtIndex(i);
					var data = Data.Create(ae);
					sd.Add(data);

					var error = string.Empty;
					if (!data.IsDescriptionValid())
						error += $"\tDescription Invalid: {data.Description}\n";
					if (!data.IsTranscriptValid())
						error += $"\tTranscription Invalid: {data.Transcript}\n";
					if (!data.IsActivationListValid())
						error += $"\tDescription Invalid: {data.ActivationList}\n";

					if (!string.IsNullOrEmpty(error))
					{
						error = $"{data.Filename} has invalid data:\n" + error;
						validationErrors.Add(error);
					}
				}

				if (validationErrors.Count != 0)
					File.WriteAllLines(validateFilename, validationErrors);

				return (sd, validationErrors.Count != 0);
			}

			void CreateSoundDataFile(string filename)
			{
				var existing = new List<string> { "Filename,Duration,Description,Transcript,ActivationList" };
				foreach (var sound in sounds.Sounds)
				{
					var ac = AssetDatabase.LoadAssetAtPath<AudioClip>(sound.Filename.Replace("\\", "/"));
					existing.Add($"{sound.Filename},{ac.length * 1000}ms,{sound.Description},{sound.Transcript},{sound.ActivationList}");
				}

				File.WriteAllLines(filename, existing);
			}
		}

		private static List<SoundDocData> GetCurrentSoundData(SerializedProperty existingSoundData)
		{
			var existing = new List<SoundDocData>();
			for (var i = 0; i < existingSoundData.arraySize; i++)
			{
				var ae = existingSoundData.GetArrayElementAtIndex(i);
				existing.Add(new SoundDocData(ae.FindPropertyRelative("filename").stringValue, ae.FindPropertyRelative("description").stringValue, ae.FindPropertyRelative("transcript").stringValue, ae.FindPropertyRelative("activationList").stringValue));
			}

			return existing;
		}

		private static string GetGameName()
		{
			const string filename = "Registries/game.xausreg";
			if (!File.Exists(filename))
				return "TEMPLATE GAME";

			var ausRegFile = new XmlDocument();

			using var streamReader = new StreamReader("Registries/game.xausreg");
			ausRegFile.Load(streamReader);
			streamReader.Close();

			return ausRegFile?.DocumentElement == null ? "TEMPLATE GAME" : ausRegFile.DocumentElement.GetAttribute("GameName").ToUpper();
		}

		#region Nested Classes

		[Serializable]
		private class Data
		{
			public string Filename { get; set; }
			public string Description { get; set; }
			public string Transcript { get; set; }
			public string ActivationList { get; set; }

			public bool IsTranscriptValid() => Transcript != "TODO";
			public bool IsActivationListValid() => ActivationList != "TODO";
			public bool IsDescriptionValid() => Description != "Other";

			public Data(string filename, string description, string transcript, string activationList)
			{
				Filename = filename;
				Description = description;
				Transcript = transcript;
				ActivationList = activationList;
			}

			public static Data Create(SerializedProperty property) => new Data(property.FindPropertyRelative("filename").stringValue, property.FindPropertyRelative("description").stringValue, property.FindPropertyRelative("transcript").stringValue, property.FindPropertyRelative("activationList").stringValue);
		}

		#endregion
	}
}