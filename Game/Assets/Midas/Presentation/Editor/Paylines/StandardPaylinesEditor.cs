using System.Collections.Generic;
using System.Linq;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.Editor.GameData;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Paylines;
using Midas.Presentation.Reels;
using UnityEditor;
using UnityEngine;

namespace Midas.Presentation.Editor.Paylines
{
	[CustomEditor(typeof(StandardPaylines))]
	public sealed class StandardPaylinesEditor : UnityEditor.Editor
	{
		#region Constants

		public const string DefaultPaylineSettings = "Assets/Game/Assets/PaylineSettings.asset";

		#endregion

		#region Fields

		private IReadOnlyList<LinePatternInformation> patterns;
		private ReelContainer selectedReelContainer;

		#endregion

		#region Overrides of Editor

		public override void OnInspectorGUI()
		{
			var paylines = target as StandardPaylines;
			if (paylines == null)
			{
				base.OnInspectorGUI();
				return;
			}

			if (paylines.Settings == null)
			{
				paylines.Settings = (StandardPaylinesSettings)AssetDatabase.LoadAssetAtPath(DefaultPaylineSettings, typeof(StandardPaylinesSettings));
			}

			paylines.Settings = (StandardPaylinesSettings)EditorGUILayout.ObjectField("Settings", paylines.Settings, typeof(StandardPaylinesSettings), false);

			patterns ??= GetStageModelDetails();

			if (patterns.Count > 0)
			{
				var selectedIndex = patterns.FindIndex(p => p.Name == paylines.PatternFieldName);
				var generatePaylines = selectedIndex != -1;
				generatePaylines = EditorGUILayout.Toggle("Generate Paylines?", generatePaylines);

				if (generatePaylines)
				{
					if (selectedIndex == -1)
						selectedIndex = 0;

					selectedIndex = EditorGUILayout.Popup("Payline Data", selectedIndex, patterns.Select(p => p.Name).ToArray());
					paylines.PatternFieldName = patterns[selectedIndex].Name;
				}
				else
				{
					paylines.PatternFieldName = null;
				}
			}

			var allReelContainers = FindObjectsOfType<ReelContainer>();
			if (selectedReelContainer == null && allReelContainers.Length > 0)
				selectedReelContainer = allReelContainers[0];
			var selectedReelIndex = EditorGUILayout.Popup("Reel Container", allReelContainers.FindIndex(c => c == selectedReelContainer), allReelContainers.Select(c => $"{c.name} ({c.gameObject.GetOwningStageName()})").ToArray());
			if (selectedReelIndex != -1)
				selectedReelContainer = allReelContainers[selectedReelIndex];

			var wasEnabled = GUI.enabled;
			GUI.enabled = paylines.Settings != null && selectedReelContainer != null;

			if (GUILayout.Button("Create Paylines"))
			{
				CreatePaylines(paylines, patterns.FirstOrDefault(p => p.Name == paylines.PatternFieldName), selectedReelContainer);
			}

			GUI.enabled = wasEnabled;
		}

		private static IReadOnlyList<LinePatternInformation> GetStageModelDetails()
		{
			var result = new List<LinePatternInformation>();
			foreach (var stage in GameDataFactory.GetAllStageNames())
			{
				foreach (var lp in GameDataFactory.GetLinePattens(stage))
				{
					if (result.Any(r => r.Name == lp.Name))
						continue;
					result.Add(lp);
				}
			}

			return result;
		}

		#endregion

		#region Private Methods

		public static void CreatePaylines(StandardPaylines paylines, LinePatternInformation allPatterns, ReelContainer reelContainer)
		{
			var linePatterns = allPatterns?.Lines;

			if (paylines.Paylines != null)
			{
				foreach (var payline in paylines.Paylines)
					DestroyImmediate(payline.Mesh, true);
			}

			if (paylines.WinBoxes != null)
			{
				foreach (var mesh in paylines.WinBoxes.Select(b => b.Mesh).Distinct())
					DestroyImmediate(mesh, true);
			}

			var newPaylines = new List<PaylineData>();
			if (linePatterns != null)
			{
				for (var patternIndex = 0; patternIndex < linePatterns.Count; patternIndex++)
				{
					var payline = CreatePaylineForPattern(paylines.Settings, reelContainer, linePatterns[patternIndex]);
					newPaylines.Add(payline);
				}

				foreach (var payline in newPaylines)
				{
					AssetDatabase.AddObjectToAsset(payline.Mesh, paylines);
				}
			}

			paylines.Paylines = newPaylines;

			paylines.WinBoxes.Clear();

			var boxes = new Dictionary<Vector2, Mesh>();
			foreach (var reel in reelContainer.Reels)
			{
				for (var symbolIndex = 0; symbolIndex < reel.VisibleSymbols; symbolIndex++)
				{
					var size = reelContainer.GetSymbolSizeByCell(reel.Row + symbolIndex, reel.Column);
					var symbolLocation = reelContainer.GetSymbolLocationByCell(reel.Row + symbolIndex, reel.Column);

					if (!boxes.TryGetValue(size, out var mesh))
					{
						mesh = CreateWinBox(paylines.Settings, size);
						boxes.Add(size, mesh);
						mesh.name = $"WinBox {size.x}x{size.y}";
						AssetDatabase.AddObjectToAsset(mesh, paylines);
					}

					paylines.WinBoxes.Add(new WinBoxData
					{
						Column = reel.Column,
						Row = reel.Row + symbolIndex,
						Position = symbolLocation,
						Mesh = mesh,
						Size = size
					});
				}
			}

			EditorUtility.SetDirty(paylines);
			AssetDatabase.SaveAssets();
		}

		private static PaylineData CreatePaylineForPattern(StandardPaylinesSettings settings, ReelContainer reelContainer, LineDefinition pattern)
		{
			// When creating paylines, we drop the Z axis of the symbol position to flatten the paylines at 0.

			var pos = new List<Vector3>();
			for (var i = 0; i < pattern.Cells.Count; i++)
			{
				var cell = pattern.Cells[i];

				var symbolSize = reelContainer.GetSymbolSizeByCell(cell.Row, cell.Column);
				var symbolPos = reelContainer.GetSymbolLocationByCell(cell.Row, cell.Column);

				if (i == 0)
				{
					// Add the left side.

					pos.Add(new Vector3(symbolPos.x - (settings.LeftExtension + symbolSize.x / 2), symbolPos.y));
				}

				pos.Add(new Vector3(symbolPos.x, symbolPos.y));

				if (i == pattern.Cells.Count - 1)
				{
					// Add the right side.

					pos.Add(new Vector3(symbolPos.x + settings.RightExtension + symbolSize.x / 2, symbolPos.y));
				}
			}

			var mesh = PaylineBuilder.BuildLine(pos, settings.LineWidth, settings.CornerSegments, settings.VertexColor, false);
			mesh.name = pattern.Name;
			return PaylineData.Create(pattern.Name, mesh, pos);
		}

		private static Mesh CreateWinBox(StandardPaylinesSettings settings, Vector2 size)
		{
			var pos = new List<Vector3>();
			pos.Add(new Vector3(-size.x / 2f, size.y / 2f));
			pos.Add(new Vector3(size.x / 2f, size.y / 2f));
			pos.Add(new Vector3(size.x / 2f, -size.y / 2f));
			pos.Add(new Vector3(-size.x / 2f, -size.y / 2f));

			return PaylineBuilder.BuildLine(pos, settings.LineWidth, settings.CornerSegments, settings.VertexColor, true);
		}

		#endregion
	}
}