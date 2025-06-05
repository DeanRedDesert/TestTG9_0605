using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Denom;
using UnityEngine;

namespace Game.DenomMenu
{
	[RequireComponent(typeof(Button))]
	public sealed class DenomButtonFunctionUpdater : MonoBehaviour, IAutoLayoutHandler
	{
		public void OnAutoLayout(int layoutIndex)
		{
			var button = GetComponent<Button>();
			button.ChangeButtonFunction(DenomButtonFunctions.DenomButtons[layoutIndex]);
		}
	}
}