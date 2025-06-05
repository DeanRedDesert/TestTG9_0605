using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Midas.Core;
using Midas.Core.General;
using Midas.Presentation.ExtensionMethods;
using Midas.Presentation.Game;
using Midas.Presentation.Interruption;
using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.Sequencing
{
	public sealed class SequenceActivator : SequenceActivatorBase, IInterruptable
	{
		#region Types

		// ReSharper disable once MemberCanBePrivate.Global - Used in unity inspector
		public enum ActivateAction
		{
			Activate,
			Deactivate
		}

		#endregion

		#region Fields

		private ICompletionNotifier completionNotifier;
		private TimeSpan startTime;
		private TimeSpan activationOffsetTime;
		private bool activateOffset;
		private SequenceEventArgs sequenceEventArgs;
		private bool isPaused;
		private bool interrupted;
		private bool hasFinished;
		private bool hasActivated;
		private bool completionNotificationReceived;
		private TimeSpan completionNotificationReceivedTime;
		private static List<ISequenceActivatorComponent> activatorComponents;
		private static bool builtInTypesRegistered;

		#endregion

		#region Inspector Fields

		[SerializeField]
		private MultiSequenceEventPair sequenceEventPair;

		[SerializeField]
		private ActivateAction action = ActivateAction.Activate;

		[SerializeField]
		private int activateOffsetMs;

		[SerializeField]
		private CompletionNotifier completionNotifierBehaviour;

		[SerializeField]
		private bool canBeInterrupted;

		[SerializeField]
		private bool canBeAutoInterrupted;

		[SerializeField]
		private int interruptPrio = InterruptPriorities.Default + 50;

		[SerializeField]
		private SequenceComponent[] components = Array.Empty<SequenceComponent>();

		[SerializeField]
		private UnityEvent[] activateEvents = Array.Empty<UnityEvent>();

		[SerializeField]
		private UnityEvent[] resetEvents = Array.Empty<UnityEvent>();

		#endregion

		public static void RegisterActivatorComponent(ISequenceActivatorComponent c)
		{
			activatorComponents.Add(c);
		}

		#region SequenceActivatorBaseOverrides

		public override IReadOnlyList<string> SequenceNames => sequenceEventPair.SequenceNames;

		protected override void Awake()
		{
			base.Awake();
			if (!builtInTypesRegistered)
			{
				RegisterBuiltInTypes();
			}
		}

		protected override void OnEnable()
		{
			base.OnEnable();

			completionNotifier = completionNotifierBehaviour != null ? completionNotifierBehaviour : GameBase.GameInstance.GetInterface<ICompletionNotifier>();

			if (completionNotifier != null)
				completionNotifier.Complete += OnCompletionNotifierComplete;
		}

		protected override void OnDisable()
		{
			base.OnDisable();
			if (completionNotifier != null)
			{
				completionNotifier.Complete -= OnCompletionNotifierComplete;
			}

			// Clean up any dreg event handlers

			foreach (var sequence in SequenceNames.Select(SequenceFinder.FindSequence))
				sequence.PauseSequence -= HandlePauseSequence;
		}

		protected override void RegisterSequenceEvents(AutoUnregisterHelper autoUnregisterHelper, Sequence sequence)
		{
			void OnEnter(SequenceEventArgs args, TaskCompletionSource<int> source)
			{
				isPaused = sequence.IsPaused;
				sequenceEventArgs = args;
				source.SetResult(0);
				sequence.PauseSequence += HandlePauseSequence;

				if (!isPaused)
					Resume();
			}

			var handler = new SequenceEventHandler(this, OnEnter);
			autoUnregisterHelper.RegisterSequenceEventHandler(sequence, sequenceEventPair.EventA, handler);

			handler = new SequenceEventHandler(this,
				(_, promise) => SetAsDependency(promise),
				_ => OnStateExec(),
				_ => sequence.PauseSequence -= HandlePauseSequence);

			autoUnregisterHelper.RegisterSequenceEventHandler(sequence, sequenceEventPair.EventB, handler);
		}

		#endregion

		private void Resume()
		{
			hasActivated = false;
			hasFinished = false;
			interrupted = false;
			completionNotificationReceived = false;
			completionNotificationReceivedTime = TimeSpan.Zero;

			startTime = FrameTime.CurrentTime;
			activationOffsetTime = TimeSpan.FromMilliseconds(activateOffsetMs);
			if (canBeInterrupted)
			{
				GameBase.GameInstance.GetPresentationController<InterruptController>().AddInterruptable(this);
			}

			if (activateOffsetMs == 0)
				Activate();
			else
				activateOffset = true;
		}

		private void HandlePauseSequence(bool pause)
		{
			isPaused = pause;

			if (pause)
				ResetActiveComponents();
			else
				Resume();
		}

		private void OnStateExec()
		{
			if (isPaused)
				return;

			if (sequenceEventArgs == null)
			{
				Finish();
				return;
			}

			var currentTime = FrameTime.CurrentTime;
			if (activateOffset)
			{
				if (currentTime >= startTime + activationOffsetTime)
				{
					activateOffset = false;
					Activate();
				}
				else
				{
					return; //wait a little more
				}
			}

			if (action == ActivateAction.Activate)
			{
				var finished = StopComponents(currentTime);
				if (finished)
				{
					Finish();
					GameBase.GameInstance.GetPresentationController<InterruptController>().RemoveInterruptable(this);
					hasFinished = true;
					sequenceEventArgs = null;
				}
			}
			else
			{
				//Deactivate does not wait for anything
				Finish();
				GameBase.GameInstance.GetPresentationController<InterruptController>().RemoveInterruptable(this);
				hasFinished = true;
				sequenceEventArgs = null;
			}
		}

		private void Activate()
		{
			if (hasActivated)
			{
				return;
			}

			if (sequenceEventArgs == null)
				return;

			hasActivated = true;
			if (action == ActivateAction.Activate)
			{
				StartComponents(sequenceEventArgs.Intensity);
			}
			else
			{
				StopComponents(sequenceEventArgs.Intensity);
			}
		}

		private void StartComponents(int intensity)
		{
			if (sequenceEventArgs == null)
				return;

			for (var i = 0; i < components.Length; ++i)
			{
				var intensityPair = components[i];
				var hasIntensity = intensityPair.ActivatesAtIntensity(intensity);
				if (hasIntensity)
				{
					if (intensityPair.Comp != null)
					{
						var sequenceActivatorComponent = Match(intensityPair.Comp);
						if (sequenceActivatorComponent != null)
						{
							sequenceActivatorComponent.StartComponent(intensityPair, sequenceEventArgs);
						}
						else
						{
							throw new Exception($"GSequenceActivator ({gameObject.name}): SequenceActivatorComponent ({intensityPair.Comp.GetType()}) not found!");
						}
					}

					//fire unity events if applicable
					activateEvents[i].Invoke();
				}
			}
		}

		private void ResetActiveComponents()
		{
			if (sequenceEventArgs == null)
				return;
			for (var i = 0; i < components.Length; i++)
			{
				var component = components[i];
				if (!component.ActivatesAtIntensity(sequenceEventArgs.Intensity))
					continue;

				if (component.Comp != null)
				{
					var sequenceActivatorComponent = Match(component.Comp);
					if (sequenceActivatorComponent != null)
					{
						sequenceActivatorComponent.StopComponent(component, sequenceEventArgs, true);
					}
					else
					{
						throw new Exception($"SequenceActivator ({gameObject.name}): SequenceActivatorComponent ({component.Comp.GetType()}) not found!");
					}
				}

				resetEvents[i].Invoke();
			}
		}

		private bool StopComponents(TimeSpan currentTime)
		{
			if (sequenceEventArgs == null)
				return true;

			var finished = true;
			for (var i = 0; i < components.Length; i++)
			{
				var component = components[i];
				if (!component.ActivatesAtIntensity(sequenceEventArgs.Intensity))
					continue;

				var completionNotificationTimeout = component.DurationOverride && completionNotificationReceived &&
					currentTime >= completionNotificationReceivedTime + TimeSpan.FromSeconds(component.Duration)
					|| interrupted;

				if (component.DurationOverride && !completionNotificationTimeout)
				{
					finished = false;
				}

				if (completionNotificationTimeout && !hasActivated)
				{
					Log.Instance.Error($"resetProviderTimeout=true but hasActivated=false in {this.GetPath()}. interrupted={interrupted}");
				}

				if (component.Comp != null)
				{
					var sequenceActivatorComponent = Match(component.Comp);
					if (sequenceActivatorComponent != null)
					{
						finished = sequenceActivatorComponent.TimeOutComponent(component, sequenceEventArgs, completionNotificationTimeout, finished);
					}
					else
					{
						throw new Exception($"SequenceActivator ({gameObject.name}): SequenceActivatorComponent ({component.Comp.GetType()}) not found!");
					}
				}

				if ((completionNotificationTimeout || !component.DurationOverride && finished) && !hasFinished)
				{
					resetEvents[i].Invoke();
				}
			}

			return finished;
		}

		private void StopComponents(int intensity)
		{
			for (var i = 0; i < components.Length; ++i)
			{
				var component = components[i];
				var hasIntensity = component.ActivatesAtIntensity(intensity);
				if (hasIntensity)
				{
					if (component.Comp != null)
					{
						var sequenceActivatorComponent = Match(component.Comp);
						if (sequenceActivatorComponent != null)
						{
							sequenceActivatorComponent.StopComponent(component, sequenceEventArgs, false);
						}
						else
						{
							throw new Exception($"GSequenceActivator ({gameObject.name}): SequenceActivatorComponent ({component.Comp.GetType()}) not found!");
						}
					}

					//fire unity events if applicable
					resetEvents[i].Invoke();
				}
			}
		}

		private void OnCompletionNotifierComplete(ICompletionNotifier source)
		{
			completionNotificationReceived = true;
			completionNotificationReceivedTime = FrameTime.CurrentTime;
		}

		private static void RegisterBuiltInTypes()
		{
			builtInTypesRegistered = true;
			activatorComponents = new List<ISequenceActivatorComponent>();
			RegisterActivatorComponent(new SequenceActivatorComponentSoundPlayerBase());
			RegisterActivatorComponent(new SequenceActivatorComponentTweenAnimation());
			// RegisterActivatorComponent(new SequenceActivatorComponentAnimation());
			// RegisterActivatorComponent(new SequenceActivatorComponentAudioSource());
			// RegisterActivatorComponent(new SequenceActivatorComponentGAnimation());
			RegisterActivatorComponent(new SequenceActivatorComponentISequencePlayable());
			// RegisterActivatorComponent(new SequenceActivatorComponentParticleSystem());
			// RegisterActivatorComponent(new SequenceActivatorComponentPlayableDirector());
			// RegisterActivatorComponent(new SequenceActivatorComponentTransform());
			// RegisterActivatorComponent(new SequenceActivatorComponentVideoPlayer());
		}

		private static ISequenceActivatorComponent Match(Component c)
		{
			foreach (var item in activatorComponents)
			{
				if (item.CanActivate(c))
				{
					return item;
				}
			}

			return null;
		}

		#region IInterruptable Implementation

		public bool CanBeInterrupted => true;
		public bool CanBeAutoInterrupted => canBeAutoInterrupted;
		public int InterruptPriority => interruptPrio;

		public void Interrupt()
		{
			GameBase.GameInstance.GetPresentationController<InterruptController>().RemoveInterruptable(this);
			interrupted = true;
		}

		#endregion

#if UNITY_EDITOR

		public void ConfigureForMakeGame(string seqName, Enum activateEvent, Enum blockEvent, Component component = null)
		{
			var seqNameList = new List<string> { seqName };
			sequenceEventPair = new MultiSequenceEventPair(seqNameList, activateEvent.ToString(), blockEvent.ToString());

			if (component != null)
			{
				components = new[] { new SequenceComponent(-1, component, false, 0) };
			}

			activateEvents = new UnityEvent[1];
			resetEvents = new UnityEvent[1];
		}

#endif
	}
}