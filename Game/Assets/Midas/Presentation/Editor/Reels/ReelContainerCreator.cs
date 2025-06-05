using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Editor.GameData;
using Midas.Presentation.Editor.Utilities;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Reels;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Midas.Presentation.Editor.Reels
{
	/// <summary>
	/// Lays out a reel container and reels using the data found in a stage model.
	/// </summary>
	public sealed class ReelContainerCreator : EditorWindow
	{
		#region Fields

		private string selectedStage;
		private ReelInformation selectedReels;

		private int symbolsAbove = 1;
		private int symbolsBelow = 1;
		private Vector2 symbolSize = new Vector2(2.56f, 2.56f);
		private float reelGap = 2.7f;
		private Reel reelPrefab;
		private Sprite maskSprite;

		#endregion

		#region Static Methods

		/// <summary>
		/// This command will open the symbol list generator window.
		/// </summary>
		[MenuItem("GameObject/Midas/Reel Container")]
		public static void MenuCreateReelContainer()
		{
			var w = GetWindow<ReelContainerCreator>(true, "Reel Container");
			w.Show();
		}

		#endregion

		#region Editor

		private void OnEnable()
		{
			if (reelPrefab == null)
				reelPrefab = (Reel)AssetDatabase.LoadAssetAtPath("Assets/Midas/Presentation/Editor/Reels/Prefabs/Reel.prefab", typeof(Reel));
		}

		/// <summary>
		/// Unity GUI function.
		/// </summary>
		private void OnGUI()
		{
			GUILayout.Label("Create a reel container for a stage model.");

			ShowGameSelector();

			symbolsAbove = EditorGUILayout.IntField("Symbols Above", symbolsAbove);
			if (symbolsAbove < 0)
				symbolsAbove = 0;

			symbolsBelow = EditorGUILayout.IntField("Symbols Below", symbolsBelow);
			if (symbolsBelow < 0)
				symbolsBelow = 0;

			symbolSize = EditorGUILayout.Vector2Field("Symbol Size", symbolSize);
			if (symbolSize.x < 0)
				symbolSize.x = 0;
			if (symbolSize.y < 0)
				symbolSize.y = 0;

			reelGap = EditorGUILayout.FloatField(new GUIContent("Reel Gap", "The distance between the centre points of adjacent reels"), reelGap);
			if (reelGap < 0)
				reelGap = 0;

			reelPrefab = (Reel)EditorGUILayout.ObjectField("Reel Prefab", reelPrefab, typeof(Reel), false);
			maskSprite = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Mask (Optional)", "Include a mask sprite to generate masked reels"), maskSprite, typeof(Sprite), false);

			var guiWasEnabled = GUI.enabled;
			GUI.enabled = reelPrefab != null && selectedReels != null;

			if (GUILayout.Button(new GUIContent("Create Reels", "The symbol window data will be taken from the selected processor.")))
			{
				var reelsObject = maskSprite
					? BuildReelsWithSpriteMask(selectedReels, reelPrefab, reelGap, symbolsAbove, symbolsBelow, symbolSize, maskSprite)
					: BuildReels(selectedReels, reelPrefab, reelGap, symbolsAbove, symbolsBelow, symbolSize);

				if (Selection.activeTransform != null)
					reelsObject.transform.SetParent(Selection.activeTransform, false);
			}

			GUI.enabled = guiWasEnabled;
		}

		/// <summary>
		/// Show the game selection buttons.
		/// </summary>
		private void ShowGameSelector()
		{
			selectedStage = GameDataFactory.ShowStageSelector(selectedStage);
			selectedReels = GameDataFactory.ShowReelsSelector(selectedStage, selectedReels);
		}

		#endregion

		#region Private Methods

		public static GameObject BuildReelsWithSpriteMask(ReelInformation reelInfo, Reel reelPrefab, float reelGap, int symbolsAbove, int symbolsBelow, Vector2 symbolSize, Sprite maskSprite)
		{
			var reelContainerGo = BuildReels(reelInfo, reelPrefab, reelGap, symbolsAbove, symbolsBelow, symbolSize);
			var reelContainer = reelContainerGo.GetComponent<ReelContainer>();

			AddReelMasks(reelContainer, maskSprite, symbolSize, reelGap);

			return reelContainerGo;
		}

		public static GameObject BuildReels(ReelInformation reelInfo, Reel reelPrefab, float reelGap, int symbolsAbove, int symbolsBelow, Vector2 symbolSize)
		{
			// Create the actual container with a standard reel spin.

			var gameObject = new GameObject(reelInfo.Name + " Reel Container");
			var reelContainer = gameObject.AddComponent<ReelContainer>();

			// Now create the reels.

			var columnOffset = reelInfo.Populations.Min(p => p.Min(c => c.column));
			var reelPos = -((reelInfo.ColumnCount - 1) * reelGap) / 2f;

			var populatorIndex = 0;
			var reels = new List<Reel>();
			foreach (var pop in reelInfo.Populations)
			{
				var rowOffset = reelInfo.Populations.SelectMany(p => p.Where(c => c.column == pop[0].column)).Min(c => c.row);
				var i = pop[0].column - columnOffset;
				var rowIndex = pop.Min(c => c.row);
				var j = rowIndex - rowOffset;
				var symbols = pop.Max(c => c.row) - rowIndex + 1;
				var yPos = reelInfo.ColumnVisibleSymbols[i] / 2f - symbols / 2f - j;
				var newReel = gameObject.InstantiatePreFabAsChild(reelPrefab, new Vector3(reelPos + reelGap * i, yPos * symbolSize.y, 0));
				newReel.name = "Reel" + populatorIndex++;
				newReel.SymbolsAbove = symbolsAbove;
				newReel.SymbolsBelow = symbolsBelow;
				newReel.SymbolSize = symbolSize;
				newReel.VisibleSymbols = symbols;
				newReel.Row = rowIndex;
				newReel.Column = pop[0].column;

				reels.Add(newReel);
			}

			reelContainer.Configure(reels);
			return gameObject;
		}

		private static void AddReelMasks(ReelContainer container, Sprite maskSprite, Vector2 symbolSize, float reelGap)
		{
			if (AreReelsIndependent(container))
			{
				// Independent reels get per-row groups of reels

				var rows = container.Reels.GroupBy(r => r.Row).Select(g => (Row: g.Key, Reels: g.ToArray()));

				if (AreReelsJagged(container))
				{
					// One mask per reel.

					foreach (var row in rows)
					{
						var reelRoot = new GameObject($"Row{row.Row}Group");
						reelRoot.transform.SetParent(container.transform, false);
						var sg = reelRoot.AddComponent<SortingGroup>();
						sg.sortingOrder = 10;

						foreach (var reel in row.Reels)
						{
							reel.transform.SetParent(reelRoot.transform, true);

							var reelSize = new Vector2(symbolSize.x, symbolSize.y * reel.VisibleSymbols);
							SpriteMaskHelper.CreateMask(reelSize, maskSprite, reelRoot.transform, $"{reel.gameObject.name}Mask", reel.transform.localPosition);
						}
					}
				}
				else
				{
					// One mask per row.

					foreach (var row in rows)
					{
						var reelRoot = new GameObject($"Row{row.Row}Group");
						reelRoot.transform.SetParent(container.transform, false);
						reelRoot.transform.localPosition = new Vector2(0, row.Reels[0].transform.position.y);
						foreach (var reel in row.Reels)
							reel.transform.SetParent(reelRoot.transform, true);

						var sg = reelRoot.AddComponent<SortingGroup>();
						sg.sortingOrder = 10;

						var size = new Vector2(row.Reels.Length * symbolSize.x + (reelGap - symbolSize.x) * (row.Reels.Length - 1), row.Reels[0].VisibleSymbols * symbolSize.y);
						SpriteMaskHelper.CreateMask(size, maskSprite, reelRoot.transform, $"ReelMask", Vector2.zero);
					}
				}
			}
			else
			{
				// Regular reels are not grouped.

				var sg = container.gameObject.AddComponent<SortingGroup>();
				sg.sortingOrder = 10;

				if (AreReelsJagged(container))
				{
					// One mask per reel.

					foreach (var reel in container.Reels)
					{
						var reelSize = new Vector2(symbolSize.x, symbolSize.y * reel.VisibleSymbols);
						SpriteMaskHelper.CreateMask(reelSize, maskSprite, container.transform, $"{reel.gameObject.name}Mask", reel.transform.localPosition);
					}
				}
				else
				{
					// One mask per reel container.

					var groups = container.Reels.GroupBy(r => r.Row);
					foreach (var group in groups)
					{
						var row = group.ToList();
						var size = new Vector2(row.Count * symbolSize.x + (reelGap - symbolSize.x) * (row.Count - 1), row[0].VisibleSymbols * symbolSize.y);
						SpriteMaskHelper.CreateMask(size, maskSprite, container.transform, $"ReelMask", Vector3.zero);
					}
				}
			}
		}

		private static bool AreReelsIndependent(ReelContainer container)
		{
			// Reels are independent if there are more than one reel in a column.

			for (var i = 0; i < container.ReelCount; i++)
			{
				var reel = container.Reels[i];
				for (var j = i + 1; j < container.ReelCount; j++)
				{
					if (reel.Column == container.Reels[j].Column)
						return true;
				}
			}

			return false;
		}

		private static bool AreReelsJagged(ReelContainer container)
		{
			// Reels are jagged if there are reels the same row but have a different y position or number of visible symbols.

			var rowCache = new Dictionary<int, (float YPos, int VisibleSymbols)>();

			foreach (var reel in container.Reels)
			{
				if (!rowCache.TryGetValue(reel.Row, out var rowPos))
				{
					rowCache.Add(reel.Row, (reel.transform.position.y, reel.VisibleSymbols));
				}
				else
				{
					if (reel.transform.position.y != rowPos.YPos || reel.VisibleSymbols != rowPos.VisibleSymbols)
						return true;
				}
			}

			return false;
		}

		#endregion
	}
}