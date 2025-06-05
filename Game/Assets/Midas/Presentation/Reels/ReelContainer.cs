using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Symbols;
using UnityEngine;
using UnityEngine.Serialization;

namespace Midas.Presentation.Reels
{
	[Serializable]
	public class ReelGroup
	{
		[SerializeField]
		private List<Reel> reels;

		public IReadOnlyList<Reel> Reels => reels;

		public ReelGroup() { }

		public ReelGroup(IEnumerable<Reel> reels)
		{
			this.reels = reels.ToList();
		}
	}

	public enum ReelGroupingMode
	{
		ByColumn,
		Individual,
		Custom
	}

	/// <summary>
	/// The standard reel container implementation.
	/// </summary>
	public sealed class ReelContainer : MonoBehaviour
	{
		#region Fields

		private Reel[][] reelsByCell;
		private ReelGroup[] processedReelGroups;

		#endregion

		#region Inspector Fields

		[SerializeField]
		[FormerlySerializedAs("Reels")]
		private List<Reel> reels = new List<Reel>();

		[Tooltip("How the reels should be grouped for reel spin.")]
		[SerializeField]
		private ReelGroupingMode reelGroupingMode;

		[Tooltip("Custom reel groups. Only used in custom reel grouping mode.")]
		[SerializeField]
		private List<ReelGroup> customReelGroups;

		#endregion

		#region Properties

		/// <summary>
		/// The reels connected to this container.
		/// </summary>
		public IReadOnlyList<Reel> Reels => reels;

		/// <summary>
		/// Gets the number of reels in the container.
		/// </summary>
		public int ReelCount => Reels.Count;

		/// <summary>
		/// Gets whether all reels are idle.
		/// </summary>
		public bool IsIdle => Reels.All(r => r.SpinState == ReelSpinState.Idle);

		#endregion

		#region Public Methods

		/// <summary>
		/// Get the symbol size
		/// </summary>
		/// <param name="row">The cell row location of the symbol.</param>
		/// <param name="column">The cell column location of the symbol.</param>
		/// <returns>The symbol size relative to the container.</returns>
		public Vector2 GetSymbolSizeByCell(int row, int column)
		{
			var reel = GetReelByCell(row, column);
			if (!reel)
				return Vector2.zero;

			var ss = reel.GetSymbolSize(row - reel.Row);
			var ls = transform.localScale;
			ss.Scale(ls);
			return new Vector3(ss.x * ls.x, ss.y * ls.y, ls.z);
		}

		/// <summary>
		/// Get the symbol position by cell.
		/// </summary>
		/// <param name="row">The cell row location of the symbol.</param>
		/// <param name="column">The cell column location of the symbol.</param>
		/// <returns>The symbol location relative to the container.</returns>
		public Vector3 GetSymbolLocationByCell(int row, int column)
		{
			var reel = GetReelByCell(row, column);
			if (!reel)
				return Vector3.zero;

			return transform.InverseTransformPoint(reel.GetSymbolWorldPosition(row - reel.Row));
		}

		/// <summary>
		/// Get the symbol game object by cell.
		/// </summary>
		/// <param name="row">The cell row location of the symbol.</param>
		/// <param name="column">The cell column location of the symbol.</param>
		/// <returns>The symbol game object of the cell.</returns>
		public GameObject GetSymbolByCell(int row, int column)
		{
			var reel = GetReelByCell(row, column);
			return reel ? reel.GetSymbol(row - reel.Row) : null;
		}

		/// <summary>
		/// Initialise the reels.
		/// </summary>
		/// <param name="reelStrips">The strips to use.</param>
		/// <param name="symbolList">Provides a symbol list for each reel.</param>
		public void Initialise(IReadOnlyList<ReelStrip> reelStrips, ReelSymbolList symbolList)
		{
			StopAllCoroutines();

			if (reelStrips == null || symbolList == null)
			{
				foreach (var reel in Reels)
					reel.Initialise(null, null);
			}
			else
			{
				for (var i = 0; i < Reels.Count; i++)
				{
					var reel = Reels[i];
					if (reel != null)
					{
						reel.Initialise(reelStrips[i], symbolList);
					}
				}
			}
		}

		public void SpinAllReels(IReadOnlyList<ReelStrip> reelStrips, IReadOnlyList<TimeSpan> stopTimings, IReadOnlyList<SpinSettings> spinSettings, ReelSymbolList symbolList, Action<Reel> reelStateChangedCallback)
		{
			for (var i = 0; i < Reels.Count; i++)
			{
				var groupIndex = Reels[i].Group;

				if (reelStrips[i] != null)
					Reels[i].Spin(reelStrips[i], stopTimings[groupIndex], spinSettings[groupIndex], symbolList, reelStateChangedCallback);
			}
		}

		public IReadOnlyList<ReelGroup> GetReelGroups()
		{
			if (processedReelGroups == null)
			{
				switch (reelGroupingMode)
				{
					case ReelGroupingMode.Individual:
						processedReelGroups = Reels.Select(r => new ReelGroup(new[] { r })).ToArray();
						break;

					case ReelGroupingMode.ByColumn:
						processedReelGroups = Reels.GroupBy(r => r.Column).Select(r => new ReelGroup(r)).ToArray();
						break;

					case ReelGroupingMode.Custom:
						processedReelGroups = customReelGroups.ToArray();
						break;
				}
			}

			return processedReelGroups;
		}

		#endregion

		#region Private Methods

		private void Awake()
		{
			var groups = GetReelGroups();
			for (var i = 0; i < groups.Count; i++)
			{
				foreach (var reel in groups[i].Reels)
					reel.Group = i;
			}
		}

		public Reel GetReelByCell(int row, int column)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				// The cellsByRow cache is only initialised at runtime, so the editor needs to search the list if not playing.

				var editorReel = Reels.FirstOrDefault(r => column == r.Column && row >= r.Row && row < r.Row + r.VisibleSymbols);
				if (!editorReel)
				{
					Debug.LogError($"No symbol at cell C{column}R{row}");
					return null;
				}

				return editorReel;
			}
#endif

			if (reelsByCell == null)
			{
				// Create the reels by cell cache.

				var columns = Reels.GroupBy(r => r.Column).ToArray();

				reelsByCell = new Reel[columns.Max(c => c.Key) + 1][];

				foreach (var c in columns)
				{
					var reels = c.ToArray();
					reelsByCell[c.Key] = new Reel[reels.Max(r => r.Row + r.VisibleSymbols)];

					foreach (var reel in reels)
					{
						for (var i = 0; i < reel.VisibleSymbols; i++)
							reelsByCell[c.Key][reel.Row + i] = reel;
					}
				}
			}

			if (column < 0 || column >= reelsByCell.Length || reelsByCell[column] == null ||
				row < 0 || row >= reelsByCell[column].Length || reelsByCell[column][row] == null)
			{
				Debug.LogError($"No symbol at cell C{column}R{row}");
				return null;
			}

			return reelsByCell[column][row];
		}

		#endregion

		#region Editor

#if UNITY_EDITOR

		public void Configure(IReadOnlyList<Reel> reels)
		{
			this.reels = reels.ToList();
		}

#endif

		#endregion
	}
}