using System.Collections.Generic;
using System.Linq;
using Midas.Core;
using Midas.Core.Coroutine;
using Midas.Core.ExtensionMethods;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Game;

namespace Midas.Presentation.Progressives
{
	public interface IProgressiveDisplay
	{
		void SetValue(string levelId, Money? value);
	}

	public sealed class ProgressiveDisplayController : IPresentationController
	{
		private sealed class Handler
		{
			public ProgressiveMeter ProgressiveMeter;
			public Money? LastValue;
			public readonly List<IProgressiveDisplay> Displays = new List<IProgressiveDisplay>();
		}

		private readonly AutoUnregisterHelper unregisterHelper = new AutoUnregisterHelper();
		private Coroutine progDisplayCoroutine;
		private readonly Dictionary<string, Handler> handlers = new Dictionary<string, Handler>();
		private bool newBroadcastAvailable;

		void IPresentationController.Init()
		{
			progDisplayCoroutine = FrameUpdateService.Update.StartCoroutine(DoProgDisplay());
			unregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ProgressiveStatus, nameof(ProgressiveStatus.BroadcastData), (_, __) => newBroadcastAvailable = true);
		}

		void IPresentationController.DeInit()
		{
			progDisplayCoroutine?.Stop();
			progDisplayCoroutine = null;
			unregisterHelper.UnRegisterAll();
		}

		void IPresentationController.Destroy()
		{
		}

		public void RegisterProgressiveDisplay(IProgressiveDisplay progressiveDisplay, string levelId)
		{
			if (!handlers.TryGetValue(levelId, out var handler))
			{
				var broadcastData = StatusDatabase.ProgressiveStatus?.BroadcastData?.FirstOrDefault(v => v.LevelId == levelId);
				var progMeter = new ProgressiveMeter();

				if (broadcastData != null)
					progMeter.SetValue(broadcastData.Value.Value, StatusDatabase.GameStatus?.GameMode != FoundationGameMode.Play);

				handler = new Handler
				{
					ProgressiveMeter = progMeter,
					LastValue = progMeter.GetDisplayValue()
				};

				handlers.Add(levelId, handler);
			}

			progressiveDisplay.SetValue(levelId, handler.ProgressiveMeter.GetDisplayValue());
			if (!handler.Displays.Contains(progressiveDisplay))
				handler.Displays.Add(progressiveDisplay);
		}

		public void UnRegisterProgressiveDisplay(IProgressiveDisplay progressiveDisplay)
		{
			foreach (var h in handlers.Values)
			{
				h.Displays.Remove(progressiveDisplay);
			}
		}

		private IEnumerator<CoroutineInstruction> DoProgDisplay()
		{
			while (true)
			{
				if (newBroadcastAvailable)
				{
					newBroadcastAvailable = false;

					if (StatusDatabase.ProgressiveStatus.BroadcastData != null)
					{
						var snap = StatusDatabase.GameStatus?.GameMode != FoundationGameMode.Play;
						foreach (var level in StatusDatabase.ProgressiveStatus.BroadcastData)
						{
							if (handlers.TryGetValue(level.LevelId, out var h))
								h.ProgressiveMeter.SetValue(level.Value, snap);
						}
					}
				}

				foreach (var h in handlers)
				{
					var newVal = h.Value.ProgressiveMeter.GetDisplayValue();
					if (!newVal.NullableEquals(h.Value.LastValue))
					{
						foreach (var display in h.Value.Displays)
							display.SetValue(h.Key, newVal);

						h.Value.LastValue = newVal;
					}
				}

				yield return null;
			}
		}
	}
}