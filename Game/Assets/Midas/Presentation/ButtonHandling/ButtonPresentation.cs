using UnityEngine;

namespace Midas.Presentation.ButtonHandling
{
	public abstract class ButtonPresentation : MonoBehaviour
	{
		public abstract bool OnSpecificButtonDataChanged(object oldSpecificData, object newSpecificData);

		public abstract void RefreshVisualState(Button button, ButtonStateData buttonStateData);
	}
}