using System;
using System.Linq;
using Midas.Core;
using TMPro;
using UnityEngine;

namespace Midas.Presentation.Reels.Ears
{
	/// <summary>
	/// Uses text to represent the currently selected multiway ways and credit value.
	/// </summary>
	public sealed class MultiwayTextEar : Ear
	{
		[Serializable]
		private sealed class WaysToValue
		{
			[Tooltip("The credit value.")]
			public int stakeValue;

			[Tooltip("The number of ways to display for the credit value.")]
			public int ways;
		}

		[SerializeField]
		private TMP_Text waysTextValue;

		[SerializeField]
		private TMP_Text waysTextLabel;

		[SerializeField]
		private TMP_Text creditsTextValue;

		[SerializeField]
		private TMP_Text creditsTextLabel;

		[Tooltip("Maps a ways value to a credit value. EG 243 ways for 50 credits.")]
		private WaysToValue[] waysToValue;

		/// <inheritdoc />
		public override bool UpdateEar(IStakeCombination stakeCombination)
		{
			if (!stakeCombination.Values.TryGetValue(Stake.Multiway, out var multiway))
				return false;

			if (multiway <= 0)
				return false;

			var mapping = waysToValue.FirstOrDefault(ss => ss.stakeValue == multiway);

			waysTextValue.SetText(mapping == null ? "243" : mapping.ways.ToString());
			waysTextLabel.SetText(mapping == null || mapping.ways != 1 ? "WAYS" : "WAY");
			creditsTextValue.SetText(multiway.ToString());
			creditsTextLabel.SetText(multiway == 1 ? "CREDIT" : "CREDITS");
			return true;
		}
	}
}