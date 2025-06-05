using System;
using System.Collections.Generic;
using IGT.Game.Core.Communication.Cabinet;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.LogicToPresentation.Data;
using Midas.Presentation;
using Midas.Presentation.Data;
using Midas.Presentation.ExtensionMethods;
using Coroutine = Midas.Core.Coroutine.Coroutine;

namespace Midas.Ascent.Cabinet
{
	internal sealed class WindowVisibilityController : ICabinetController
	{
		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private Coroutine coroutine;
		private DisplayState? lastDisplayState;
		private DisplayState? requestedDisplayState;

		public void Init()
		{
			WindowController.Instance.Initialize();
		}

		public void OnBeforeLoadGame()
		{
			StatusDatabase.ConfigurationStatus.ConfiguredMonitors = WindowController.Instance.ConfiguredMonitors;
		}

		public void Resume()
		{
			WindowController.Instance.AsyncConnect();
			WindowController.Instance.PostConnect();

			RegisterForEvents();
			lastDisplayState = DisplayState.Hidden;
			WindowController.Instance.HideWindows();
			StatusDatabase.ConfigurationStatus.ConfiguredMonitors = WindowController.Instance.ConfiguredMonitors;
			coroutine = FrameUpdateService.PreUpdate.StartCoroutine(DoDisplayState());

			IEnumerator<CoroutineInstruction> DoDisplayState()
			{
				while (true)
				{
					while (requestedDisplayState == null)
						yield return null;

					if (lastDisplayState == DisplayState.Hidden)
					{
						// Wait a short time
						yield return new CoroutineDelay(0.1);
					}

					OnDisplayStateMessage(requestedDisplayState.Value);
					requestedDisplayState = null;
				}
			}
		}

		public void Pause()
		{
			coroutine?.Stop();
			coroutine = null;
			WindowController.Instance.DestroyUnityWindows();
			UnRegisterFromEvents();
		}

		public void OnAfterUnLoadGame()
		{
			// nothing to be done
		}

		public void DeInit()
		{
			lastDisplayState = null;
		}

		private void RegisterForEvents()
		{
			WindowController.Instance.CabinetWindowSizeChangedEvent += OnCabinetWindowSizeChanged;
			autoUnregisterHelper.RegisterGameServiceChangedHandler(GameServices.MachineStateService.DisplayState, ds => requestedDisplayState = ds);
		}

		private void UnRegisterFromEvents()
		{
			WindowController.Instance.CabinetWindowSizeChangedEvent -= OnCabinetWindowSizeChanged;
			autoUnregisterHelper.UnRegisterAll();
		}

		private void OnDisplayStateMessage(DisplayState displayState)
		{
			Log.Instance.InfoFormat("Display state={0}", displayState);
			if (displayState != lastDisplayState)
			{
				lastDisplayState = displayState;
				switch (displayState)
				{
					case DisplayState.Normal:
						WindowController.Instance.ShowWindows();
						break;
					case DisplayState.Suspended:
						WindowController.Instance.ShowWindows(true);
						break;
					case DisplayState.Hidden:
						WindowController.Instance.HideWindows();
						break;
					default:
						Log.Instance.Error($"Unknown display state: {lastDisplayState}");
						break;
				}
			}
		}

		private static void OnCabinetWindowSizeChanged(object sender, WindowResizeEventArgs e)
		{
			Log.Instance.Info($"Resize complete, RequestId: {e.RequestId}, " +
				$"WindowId: {e.RequestedWindow}, Status: {e.RequestedWindow.Status}");

			WaitXFrames(2, SendMessage);
			return;

			void SendMessage()
			{
				Log.Instance.Debug($"Sending Resize complete, WindowId: {e.RequestedWindow.WindowId}, " +
					$"RequestId: {e.RequestId}, Status: {e.RequestedWindow.Status}");

				try
				{
					AscentCabinet.CabinetLib.SendWindowResizeComplete(e.RequestId);
				}
				catch (Exception exception)
				{
					Log.Instance.Warn(exception);
				}
			}
		}

		private static void WaitXFrames(int numFrames, Action callback)
		{
			if (numFrames > 0)
			{
				Log.Instance.Info($"Waiting for {numFrames} before calling callback");

				// OnNextFrameUpdate is cleared on every frame so register to wait another frame

				FrameUpdateService.Update.OnNextFrameUpdate += () => WaitXFrames(numFrames - 1, callback);
			}
			else
				callback.Invoke();
		}
	}
}