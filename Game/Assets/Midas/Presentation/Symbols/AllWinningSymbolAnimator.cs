using System.Collections.Generic;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Reels;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	[RequireComponent(typeof(ReelContainer))]
	public sealed class AllWinningSymbolAnimator : MonoBehaviour
	{
		private ReelContainer reelContainer;
		private HashSet<IReelSymbolAnimation> currentAnims;

		[SerializeField]
		private PropertyReference<IReadOnlyList<IWinInfo>> allWins;

		private void Awake()
		{
			reelContainer = GetComponent<ReelContainer>();
			currentAnims = new HashSet<IReelSymbolAnimation>();
		}

		private void OnEnable()
		{
			if (allWins == null)
				return;

			allWins.ValueChanged += OnCurrentWinValueChanged;
			UpdateAnimations();
		}

		private void OnCurrentWinValueChanged(PropertyReference reference, string propertyName)
		{
			UpdateAnimations();
		}

		private void UpdateAnimations()
		{
			foreach (var anim in currentAnims)
				anim.Stop();
			currentAnims.Clear();

			if (allWins.Value != null)
			{
				var wins = allWins.Value;

				var winningPositions = new HashSet<(int Column, int Row)>();

				foreach (var w in wins)
					winningPositions.UnionWith(w.WinningPositions);

				foreach (var pos in winningPositions)
				{
					var symAnim = reelContainer.GetSymbolComponent<IReelSymbolAnimation>(pos.Row, pos.Column);
					if (symAnim != null)
					{
						// No individual wins for this symbol animator.

						symAnim.Play(null);
						currentAnims.Add(symAnim);
					}
				}
			}
		}

		private void OnDisable()
		{
			if (allWins == null)
				return;

			allWins.ValueChanged -= OnCurrentWinValueChanged;
			allWins.DeInit();

			foreach (var anim in currentAnims)
				anim.Stop();
			currentAnims.Clear();
		}

		#region Editor

#if UNITY_EDITOR

		public void ConfigureForMakeGame(string statusItemPath)
		{
			allWins = new PropertyReference<IReadOnlyList<IWinInfo>>(statusItemPath);
		}

#endif

		#endregion
	}
}