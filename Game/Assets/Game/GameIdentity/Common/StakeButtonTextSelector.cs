using Midas.Presentation.Stakes;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	/// <summary>
	/// Selects the first child text object that configures for the specified set of logic inputs based on the bet button configuration.
	/// </summary>
	public sealed class StakeButtonTextSelector : StakeButtonTextBase
	{
		#region Fields

		private StakeButtonTextBase selectedText;

		#endregion

		#region Inspector Fields

#pragma warning disable 0649
		[Tooltip("The text objects that the selector can configure. The first object in this list that configures successfully will be chosen.")]
		[SerializeField]
		private StakeButtonTextBase[] textObjects;
#pragma warning restore 0649

		#endregion

		#region Overrides of BetButtonTextBase

		/// <inheritdoc />
		public override bool Configure(IStakeButtonSpecificData stakeButtonSpecificData, bool isEnabled)
		{
			selectedText = null;
			foreach (var o in textObjects)
			{
				if (selectedText == null && o.Configure(stakeButtonSpecificData, isEnabled))
				{
					o.gameObject.SetActive(true);
					selectedText = o;
				}
				else
				{
					o.gameObject.SetActive(false);
				}
			}

			return selectedText != null;
		}

		/// <inheritdoc />
		public override void UpdateEnabledState(bool enabled)
		{
			if (selectedText)
				selectedText.UpdateEnabledState(enabled);
		}

		#endregion
	}
}