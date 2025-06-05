using System.Collections.Generic;
using Midas.Presentation.Data.StatusBlocks;
using UnityEngine;

namespace Midas.Presentation.Paylines
{
	public abstract class PaylineContainer : MonoBehaviour
	{
		/// <summary>
		/// Hide any wins currently showing.
		/// </summary>
		public abstract void HideWins();

		/// <summary>
		/// Show only the win represented by <paramref name="win"/> and hide all others.
		/// </summary>
		public abstract void HighlightWin(IWinInfo win);
	}
}