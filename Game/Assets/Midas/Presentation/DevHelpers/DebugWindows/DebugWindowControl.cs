using Midas.Presentation.Data;
using UnityEngine;
using UnityEngine.UI;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public sealed class DebugWindowControl : MonoBehaviour
	{
		[SerializeField]
		private DebugWindowsEnabler windowsEnabler;

		[SerializeField]
		private Button hiddenButton;

		private void Update()
		{
			if (!StatusDatabase.IsInitialised)
				return;

			var debugEnabled = StatusDatabase.GaffStatus.IsDebugEnabled;

			if (hiddenButton.interactable != debugEnabled)
			{
				hiddenButton.interactable = debugEnabled;
				if (!debugEnabled)
				{
					windowsEnabler.DisableAllWindows();
					if (windowsEnabler.enabled)
						windowsEnabler.ToggleEnabled();
				}
			}
		}
	}
}