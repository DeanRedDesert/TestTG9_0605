using System;
using System.Collections.Generic;
using Midas.Core.StateMachine;
using Midas.Presentation.Data;
using Midas.Presentation.Game;
using Midas.Presentation.Sequencing;

namespace Midas.Presentation.StageHandling
{
	public sealed class StageController : IPresentationController, ISequenceOwner
	{
		private const string Name = "StageController";

		private StateMachine stateMachine;
		private readonly State idleState = new State("Idle", true);
		private readonly State transitionBeginState = new State("TransitionBegin");
		private readonly State transitioningState = new State("Transitioning");
		private readonly State transitionEndState = new State("TransitionEnd");

		private StageStatus stageStatus;
		private SequenceFinder sequenceFinder;
		private readonly List<Sequence> sequences = new List<Sequence>();

		private Sequence currentSequence;
		private bool changeTriggered;
		private bool transitionDone;

		public IReadOnlyList<Sequence> Sequences => sequences;

		public StageController(IReadOnlyList<Sequence> gameSequences)
		{
			sequences.AddRange(gameSequences);
		}

		public void Init()
		{
			stageStatus = StatusDatabase.StageStatus;
			foreach (var sequence in sequences)
				sequence.Init();

			sequenceFinder = GameBase.GameInstance.GetPresentationController<SequenceFinder>();

			var stmb = new Builder(Name, StateMachineService.FrameUpdateRoot);

			stmb.In(idleState)
				.If(() => stageStatus.CurrentStage != stageStatus.DesiredStage)
				.Then(transitionBeginState);
			stmb.In(transitionBeginState)
				.OnEnterDo(_ => changeTriggered = false)
				.If(() => BeginTransition(stageStatus.CurrentStage, stageStatus.DesiredStage))
				.Then(transitioningState)
				.Else()
				.Then(transitionEndState);
			stmb.In(transitioningState)
				.If(IsTransitionDone)
				.Then(transitionEndState);

			// The short wait guarantees a one frame delay, ensuring any Awake/OnEnable calls
			// happen before other state machines continue.

			stmb.In(transitionEndState)
				.OnEnterDo(_ => stageStatus.SetDesiredAsCurrent())
				.If(() => true)
				.Wait(TimeSpan.FromMilliseconds(1))
				.Then(idleState);

			stateMachine = stmb.CreateStateMachine();
		}

		public void DeInit()
		{
			StateMachines.Destroy(ref stateMachine);
			sequenceFinder = null;

			foreach (var sequence in sequences)
				sequence.DeInit();

			stageStatus = null;
		}

		public void Destroy()
		{
			sequences.Clear();
		}

		public void ActivateStage(Stage newStage, bool recover)
		{
			// Calling this will also cause an interruption.

			if (!recover && stageStatus.CurrentStage.Equals(newStage))
				return;

			StageActivator.Activate(newStage);

			stageStatus.SetDesired(newStage);
			stageStatus.SetDesiredAsCurrent();
		}

		public void SwitchTo(Stage newStage)
		{
			if (newStage == null)
			{
				Log.Instance.Fatal("A stage must not be null");
				return;
			}

			if (stageStatus.DesiredStage.Equals(newStage))
			{
				// If we have already set the desired stage. No need to do anything here.
				return;
			}

			changeTriggered = stageStatus.SetDesired(newStage);
		}

		public bool IsTransitioning()
		{
			return changeTriggered || stateMachine.CurrentState != idleState;
		}

		private void AddSequence(Sequence sequence) => sequences.Add(sequence);

		private bool BeginTransition(Stage currentStage, Stage desiredStage)
		{
			transitionDone = true;
			if (stageStatus.CurrentStage == stageStatus.DesiredStage)
			{
				return false;
			}

			currentSequence = FindSequence(currentStage, desiredStage);
			if (currentSequence != null)
			{
				transitionDone = false;
				currentSequence.Start();
				return true;
			}

			if (!currentStage.Equals(Stages.Undefined))
			{
				// First transition from Undefined to Basegame is not an error.

				Log.Instance.Warn($"There is no transition defined for {currentStage} to {desiredStage}. Please check your GameStartup scene.");
			}

			return false;
		}

		private bool IsTransitionDone()
		{
			if (currentSequence != null && !transitionDone)
			{
				transitionDone = !currentSequence.IsActive;
				if (transitionDone)
				{
					currentSequence = null;
				}
			}

			return transitionDone;
		}

		private static Sequence FindSequence(Stage currentStage, Stage desiredStage)
		{
			return GameBase.GameInstance.GetTransitionSequence(currentStage, desiredStage);
		}

		public void DeactivateAll()
		{
			StageActivator.DeactivateAll();
		}
	}
}