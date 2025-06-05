using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.General;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Paylines
{
	/// <summary>
	/// Payline container implementation. Instantiates and activates paylines.
	/// </summary>
	public sealed class MeshPaylineContainer : PaylineContainer
	{
		#region Fields

		private readonly List<Payline> paylines = new List<Payline>();
		private readonly List<WinBox> winBoxes = new List<WinBox>();

		#endregion

		#region Inspector Fields

		/// <summary>
		/// The paylines asset to use.
		/// </summary>
		[SerializeField]
		private StandardPaylines standardPaylines;

		/// <summary>
		/// The color selector
		/// </summary>
		[SerializeField]
		private ColorSelector colorSelector;

		/// <summary>
		/// The prefab to use when instantiating paylines.
		/// </summary>
		[SerializeField]
		private Payline paylinePrefab;

		/// <summary>
		/// The prefab to use when instantiating win boxes.
		/// </summary>
		[SerializeField]
		private WinBox winBoxPrefab;

		/// <summary>
		/// The Z offset for paylines and win boxes. Win boxes will be closest to the camera, and the paylines further away.
		/// </summary>
		[FormerlySerializedAs("ZOffset")]
		[SerializeField]
		private float zOffset = 0.01f;

		[FormerlySerializedAs("WinBoxSortingLayer")]
		[SerializeField]
		private CustomSortingLayer winBoxSortingLayer;

		[FormerlySerializedAs("WinLineSortingLayer")]
		[SerializeField]
		private CustomSortingLayer winLineSortingLayer;

		#endregion

		#region Unity Hooks

		private void Awake()
		{
			if (standardPaylines == null)
				return;

			var z = 0f;
			if (winBoxPrefab && standardPaylines.WinBoxes != null)
			{
				// Add win boxes

				for (var index = 0; index < standardPaylines.WinBoxes.Count; index++)
				{
					var winBox = standardPaylines.WinBoxes[index];
					var instance = gameObject.InstantiatePreFabAsChild(winBoxPrefab, new Vector3(winBox.Position.x, winBox.Position.y, z));
					instance.gameObject.SetLayerRecursively(gameObject.layer);
					instance.gameObject.name = $"WinBox {winBox.Column},{winBox.Row}";
					instance.gameObject.SetHideFlagsRecursively(HideFlags.DontSave);
					instance.SetWinBoxData(winBox);
					instance.gameObject.SetActive(false);

					var r = instance.GetComponent<Renderer>();
					winBoxSortingLayer.Apply(r);

					winBoxes.Add(instance);
					z += zOffset;
				}
			}

			if (paylinePrefab && standardPaylines.Paylines != null)
			{
				// Add paylines

				for (var index = 0; index < standardPaylines.Paylines.Count; index++)
				{
					var payine = standardPaylines.Paylines[index];
					var instance = gameObject.InstantiatePreFabAsChild(paylinePrefab, new Vector3(0, 0, z));
					instance.gameObject.SetLayerRecursively(gameObject.layer);
					instance.gameObject.name = payine.Mesh.name;
					instance.gameObject.SetHideFlagsRecursively(HideFlags.DontSave);
					instance.SetPaylineData(payine);
					instance.gameObject.SetActive(false);

					var r = instance.GetComponent<Renderer>();
					winLineSortingLayer.Apply(r);

					paylines.Add(instance);
					z += zOffset;
				}
			}
		}

		#endregion

		#region PaylineContainer Overrides

		public override void HideWins()
		{
			for (var index = 0; index < winBoxes.Count; index++)
				winBoxes[index].gameObject.SetActive(false);

			for (var index = 0; index < paylines.Count; index++)
				paylines[index].gameObject.SetActive(false);
		}

		public override void HighlightWin(IWinInfo win)
		{
			var lineNumber = win.LineNumber ?? 0;
			var lineColor = colorSelector == null ? Color.white : colorSelector.GetColor(lineNumber);

			for (var index = 0; index < paylines.Count; index++)
			{
				var pl = paylines[index];
				if (pl.LineName == win.PatternName)
				{
					pl.SetColor(lineColor);
					pl.gameObject.SetActive(true);
				}
				else
				{
					pl.gameObject.SetActive(false);
				}
			}

			for (var index = 0; index < winBoxes.Count; index++)
			{
				var wb = winBoxes[index];
				if (win.WinningPositions.Any(p => p.Column == wb.Column && p.Row == wb.Row))
				{
					wb.SetColor(lineColor);
					wb.gameObject.SetActive(true);
				}
				else
				{
					wb.gameObject.SetActive(false);
				}
			}
		}

		#endregion

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(StandardPaylines pl, ColorSelector cs, CustomSortingLayer winBoxSort, CustomSortingLayer winLineSort)
		{
			standardPaylines = pl;
			colorSelector = cs;
			winBoxSortingLayer = winBoxSort;
			winLineSortingLayer = winLineSort;
		}
#endif

		#endregion
	}
}