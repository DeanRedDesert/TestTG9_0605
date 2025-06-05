using Midas.Core;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Reels.Ears
{
	/// <summary>
	/// Uses text to represent the currently selected lines.
	/// </summary>
	public sealed class LineTextEar : Ear
	{
		[SerializeField]
		private TMP_Text digitsTextArea;

		[SerializeField]
		private TMP_Text lineTextArea;

		/// <inheritdoc />
		public override bool UpdateEar(IStakeCombination stakeCombination)
		{
			if (!stakeCombination.Values.TryGetValue(Stake.LinesBet, out var linesBet))
				return false;

			digitsTextArea.SetText($"{linesBet}");
			lineTextArea.SetText(linesBet == 1 ? "LINE" : "LINES");
			return true;
		}
	}
}