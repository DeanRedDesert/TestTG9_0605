using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public class DebugWindowRegistration : MonoBehaviour
	{
		private DebugWindowsEnabler debugWindowsEnabler;
		private readonly List<DebugWindow> registeredWindows = new List<DebugWindow>();

		protected virtual void Awake()
		{
			RegisterWindows();
		}

		protected virtual void OnDestroy()
		{
			UnregisterWindows();
		}

		protected virtual void OnEnable()
		{
			RegisterWindows();
		}

		protected virtual void OnDisable()
		{
			UnregisterWindows();
		}

		private void RegisterWindows()
		{
			if (registeredWindows.Count > 0)
			{
				UnregisterWindows();
			}

			debugWindowsEnabler ??= FindObjectOfType<DebugWindowsEnabler>(true);
			if (debugWindowsEnabler)
			{
				var windows = GetComponentsInChildren<DebugWindow>(true);
				foreach (var window in windows)
				{
					if (debugWindowsEnabler.AddDebugWindow(window))
					{
						registeredWindows.Add(window);
					}
				}
			}
		}

		private void UnregisterWindows()
		{
			foreach (var registeredWindow in registeredWindows)
			{
				debugWindowsEnabler.RemoveDebugWindow(registeredWindow);
			}

			registeredWindows.Clear();
		}

	}
}