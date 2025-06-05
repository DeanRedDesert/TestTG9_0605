using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;
using UnityEngine;

namespace Game.GameIdentity.Common
{
	[RequireComponent(typeof(Button))]
	public sealed class StakeButtonFunctionUpdater : MonoBehaviour, IAutoLayoutHandler
	{
		public enum ButtonType
		{
			Bet,
			Play,
			Stake
		}

		[SerializeField]
		private ButtonType buttonType;

		public void OnAutoLayout(int layoutIndex)
		{
			var button = GetComponent<Button>();
			switch (buttonType)
			{
				case ButtonType.Bet:
					button.ChangeButtonFunction(StatusDatabase.StakesStatus.BetButtonFunctions[layoutIndex]);
					break;
				case ButtonType.Play:
					button.ChangeButtonFunction(StatusDatabase.StakesStatus.PlayButtonFunctions[layoutIndex]);
					break;
				case ButtonType.Stake:
					button.ChangeButtonFunction(StatusDatabase.StakesStatus.StakeButtonFunctions[layoutIndex]);
					break;
			}
		}
	}
}