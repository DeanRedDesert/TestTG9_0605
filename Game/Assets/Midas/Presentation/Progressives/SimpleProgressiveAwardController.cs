using System;
using System.Collections.Generic;
using Midas.Core.Coroutine;
using Midas.Core.General;
using Midas.Core.StateMachine;
using Midas.LogicToPresentation;
using Midas.LogicToPresentation.Data.Services;
using Midas.LogicToPresentation.Messages;
using Midas.Presentation.Data;
using Midas.Presentation.Data.StatusBlocks;
using Midas.Presentation.Sequencing;

namespace Midas.Presentation.Progressives
{
	// ReSharper disable UnusedMember.Local
	public enum ProgressiveAwardSequenceIds
	{
		ShowFanfare,
		FanfareComplete
	}

	public enum ProgressivePreShowSequenceIds
	{
		HighlightTrigger,
		PreShowWinComplete
	}
	// ReSharper restore UnusedMember.Local

	public sealed class SimpleProgressiveAwardController : ProgressiveAwardController, ISequenceOwner
	{
		private enum State
		{
			Idle,
			Starting,
			Fanfare,
			Finishing
		}

		private readonly AutoUnregisterHelper autoUnregisterHelper = new AutoUnregisterHelper();
		private Coroutine progressiveAwardCoroutine;
		private bool showRequested;
		private bool isShowing;
		private ProgressiveAwardServiceData awardData;

		private readonly List<Sequence> sequences = new List<Sequence>();
		private readonly Dictionary<string, Sequence> preShowSequences = new Dictionary<string, Sequence>();
		private readonly Dictionary<string, Sequence> awardSequences = new Dictionary<string, Sequence>();

		public IReadOnlyList<Sequence> Sequences => sequences;

		public void RegisterProgressiveAwardSequence(string levelId)
		{
			var seq = SimpleSequence.Create<ProgressiveAwardSequenceIds>($"ProgressiveAward/{levelId}Award");
			awardSequences.Add(levelId, seq);
			sequences.Add(seq);
		}

		public void RegisterProgressiveAwardSequence<T>(string levelId) where T : Enum
		{
			var seq = SimpleSequence.Create<T>($"ProgressiveAward/{levelId}Award");
			awardSequences.Add(levelId, seq);
			sequences.Add(seq);
		}

		public void RegisterPreShowSequence(string levelId)
		{
			var seq = SimpleSequence.Create<ProgressivePreShowSequenceIds>($"ProgressiveAward/{levelId}PreShow");
			preShowSequences.Add(levelId, seq);
			sequences.Add(seq);
		}

		public void RegisterPreShowSequence<T>(string levelId) where T : Enum
		{
			var seq = SimpleSequence.Create<T>($"ProgressiveAward/{levelId}PreShow");
			preShowSequences.Add(levelId, seq);
			sequences.Add(seq);
		}

		public override bool IsAwarding => showRequested || isShowing;

		protected override bool CanAwardProgressive(ProgressiveAwardServiceData award)
		{
			return true;
		}

		protected override void AwardProgressive(int awardIndex)
		{
			if (!CanAwardProgressive(awardIndex))
				return;

			StatusDatabase.ProgressiveStatus.CurrentProgressiveAward = StatusDatabase.ProgressiveStatus.ProgressiveAwards[awardIndex];
			awardData = StatusDatabase.ProgressiveStatus.ProgressiveAwards[awardIndex];
			showRequested = true;
		}

		public override void Init()
		{
			autoUnregisterHelper.RegisterPropertyChangedHandler(StatusDatabase.ProgressiveStatus, nameof(ProgressiveStatus.CurrentProgressiveAward), (_, __) => awardData = StatusDatabase.ProgressiveStatus.CurrentProgressiveAward);
			progressiveAwardCoroutine = StateMachineService.FrameUpdateRoot.StartCoroutine(DoProgressiveAward, State.Idle, "ProgressiveAward");

			foreach (var seq in Sequences)
				seq.Init();
		}

		public override void DeInit()
		{
			foreach (var seq in Sequences)
				seq.DeInit();

			progressiveAwardCoroutine?.Stop();
			progressiveAwardCoroutine = null;
			autoUnregisterHelper.UnRegisterAll();
		}

		public override void Destroy()
		{
		}

		private IEnumerator<CoroutineInstruction> DoProgressiveAward(IStateInfo<State> stateInfo)
		{
			while (true)
			{
				while (!showRequested)
					yield return null;

				isShowing = true;
				showRequested = false;

				yield return stateInfo.SetNextState(State.Starting);

				if (awardData.State == ProgressiveAwardState.Triggered)
					Communication.ToLogicSender.Send(new StartProgressiveAwardMessage());

				if (preShowSequences.TryGetValue(awardData.Hit.LevelId, out var preShow))
					yield return new CoroutineRun(preShow.Run(), preShow.Name);

				while (awardData.State == ProgressiveAwardState.Triggered || awardData.State == ProgressiveAwardState.Starting)
					yield return null;

				yield return stateInfo.SetNextState(State.Fanfare);

				if (awardSequences.TryGetValue(awardData.Hit.LevelId, out var award))
					yield return new CoroutineRun(award.Run(), award.Name);
				else
					Log.Instance.Warn($"No progressive award found for progressive ID: {awardData.Hit.LevelId}");

				yield return stateInfo.SetNextState(State.Finishing);

				if (awardData.State == ProgressiveAwardState.Verified)
					Communication.ToLogicSender.Send(new ProgressiveDisplayCompleteMessage());

				while (awardData.State == ProgressiveAwardState.Verified || awardData.State == ProgressiveAwardState.FinishedDisplay)
					yield return null;

				if (awardData.State == ProgressiveAwardState.Paid)
					Communication.ToLogicSender.Send(new ClearProgressiveAwardMessage());

				while (awardData.State == ProgressiveAwardState.Paid)
					yield return null;

				StatusDatabase.ProgressiveStatus.CurrentProgressiveAward = null;
				isShowing = false;

				yield return stateInfo.SetNextState(State.Idle);
			}

			// ReSharper disable once IteratorNeverReturns
		}
	}
}