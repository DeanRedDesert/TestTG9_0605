using System.Collections.Generic;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Reels;
using UnityEngine;

namespace Midas.Presentation.Symbols
{
	[RequireComponent(typeof(ReelContainer))]
	public sealed class WinCycleSymbolAnimator : MonoBehaviour
	{
		private ReelContainer reelContainer;
		private HashSet<IReelSymbolAnimation> currentAnims;

		[SerializeField]
		private PropertyReference<IWinInfo> currentWin;

		private void Awake()
		{
			reelContainer = GetComponent<ReelContainer>();
			currentAnims = new HashSet<IReelSymbolAnimation>();
		}

		private void OnEnable()
		{
			if (currentWin == null)
				return;

			currentWin.ValueChanged += OnCurrentWinValueChanged;
			UpdateCurrentWin();
		}

		private void OnCurrentWinValueChanged(PropertyReference reference, string propertyName)
		{
			UpdateCurrentWin();
		}

		private void UpdateCurrentWin()
		{
			foreach (var anim in currentAnims)
				anim.Stop();
			currentAnims.Clear();

			if (currentWin.Value != null)
			{
				var cw = currentWin.Value;
				foreach (var pos in cw.WinningPositions)
				{
					var symAnim = reelContainer.GetSymbolComponent<IReelSymbolAnimation>(pos.Row, pos.Column);
					if (symAnim != null)
					{
						symAnim.Play(cw);
						currentAnims.Add(symAnim);
					}
				}
			}
		}

		private void OnDisable()
		{
			if (currentWin == null)
				return;

			currentWin.ValueChanged -= OnCurrentWinValueChanged;
			currentWin.DeInit();

			foreach (var anim in currentAnims)
				anim.Stop();
			currentAnims.Clear();
		}

		#region Editor

#if UNITY_EDITOR

		public void ConfigureForMakeGame(string statusItemPath)
		{
			currentWin = new PropertyReference<IWinInfo>(statusItemPath);
		}

#endif

		#endregion
	}
}