using UnityEngine;

namespace Midas.Presentation.ButtonHandling
{
	public class ButtonHandler : MonoBehaviour
	{
		[SerializeField]
		private ButtonFunction buttonFunction = new ButtonFunction(ButtonFunction.Undefined);
	}
}