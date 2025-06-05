using System.Collections.Generic;
using Midas.Core.General;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Sequencing;

namespace Midas.Presentation.WinPresentation
{
	/// <summary>
	///     custom events:
	///     _winPresMain.InsertCustomEventAfter((int)WinPresentation.SequenceEvent.FRAME_LIGHT_ANIMATION, new CustomEvent(1001,
	///     "MyFrameLightAnim", ()=> true));
	///     _winPresMain.InsertCustomEventAfter((int)WinPresentation.SequenceEvent.BELL_SOUND, new CustomEvent(1002,
	///     "MyFrameBellSound", ()=> true));
	///     registering for events
	///     RegisterForEvent((int) WinPresentation.SequenceEvent.COIN_FLIGHT, new SequenceEventCallbacks((i, source) => {
	///     source.SetResult(1); }, null, null));
	/// </summary>
	public class WinPresSequence : Sequence
	{
		private readonly Dictionary<int, List<CustomEvent>> customEvents = new Dictionary<int, List<CustomEvent>>();
		private WinPresentationStatus winPresentationStatus;

		public EventTable WinPresEventTable { get; } = new EventTable();

		public WinPresSequence(string name)
			: base(name, "WinPres")
		{
		}

		public override void Init()
		{
			winPresentationStatus = StatusDatabase.WinPresentationStatus;

			base.Init();
		}

		public override void DeInit()
		{
			base.DeInit();

			winPresentationStatus = null;
		}

		//Inserts a custom event after a specific event
		//target: can bypass other events using this param
		public void InsertCustomEventAfter(int afterId, CustomEvent evt, int targetId = -1)
		{
			if (customEvents.TryGetValue(afterId, out var events))
			{
				events.Add(evt);
			}
			else
			{
				customEvents.Add(afterId, new List<CustomEvent>()
				{
					evt
				});
			}
		}

		public override void Start()
		{
			if (IsActive)
			{
				Log.Instance.Error($"MainSequence can not be while it is already running. {this}");
			}

			// This must be done before calling base.Start() so that the eligibility details are correct.

			UpdateWinLevel();

			base.Start(); //sets _active to true
			if (Log.Instance.IsInfoEnabled)
			{
				var logList = new List<string>(10);
				logList.Add($"Starting MainSequence {Name} with CurrentWinLevel:{winPresentationStatus.CurrentWinLevel}");

				var eligibleEvents = GetDebugListOfEligibleEvents();
				foreach (var eligibleEvent in eligibleEvents)
				{
					logList.Add($"EventState: {eligibleEvent.state} is eligible with intensity: {eligibleEvent.intensity}");
				}

				Log.Instance.Info(string.Join("\n", logList));
			}

			winPresentationStatus.WinPresActive = true;
		}

		public override IReadOnlyList<(string eventName, int eventId)> GenerateSequenceEventIds()
		{
			var result = new List<(string eventName, int eventId)>();
			for (var i = 0; i < WinPresEventTable.Entries.GetLength(0); ++i)
			{
				result.Add((((SequenceEvent)i).ToString(), i));
				if (customEvents.TryGetValue(i, out var events))
				{
					foreach (var evt in events)
					{
						result.Add((evt.Name, evt.Id));
					}
				}
			}

			return result;
		}

		protected override void OnExitSequenceFinishedDo()
		{
			base.OnExitSequenceFinishedDo();
			winPresentationStatus.WinPresActive = false;
		}

		protected override bool IsSequenceEventEligible(int sequenceEventId)
		{
			if (!winPresentationStatus.WinCountRanges.IsSequenceEligible(sequenceEventId))
				return false;

			var intensity = WinPresEventTable.GetIntensity(sequenceEventId, winPresentationStatus.CurrentWinLevel);
			return intensity > 0;
		}

		protected override SequenceEventArgs GetSequenceEventParameter(int sequenceEventId)
		{
			return new SequenceEventArgs(sequenceEventId, WinPresEventTable.GetIntensity(sequenceEventId, winPresentationStatus.CurrentWinLevel));
		}

		protected virtual void UpdateWinLevel()
		{
			winPresentationStatus.CurrentWinLevel = CalculateWinLevel();
			(winPresentationStatus.CountingIntensity, winPresentationStatus.CountingTime, winPresentationStatus.CountingDelayTime) = winPresentationStatus.WinCountRanges.GetWinCountLevel(Credit.FromMoney(WinPresentationStatus.CurrentWinValue), GameStatus.TotalBet);
		}

		protected virtual int CalculateWinLevel()
		{
			var winValue = WinPresentationStatus.CurrentWinValue;
			if (winValue.IsZero)
				return -1;

			return winPresentationStatus.WinRanges.GetWinLevel(Credit.FromMoney(winValue), GameStatus.TotalBet);
		}
	}
}