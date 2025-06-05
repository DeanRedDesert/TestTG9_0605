//#define REEL_SPIN_TIMING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Midas.Core.ExtensionMethods;
using Midas.Presentation.StageHandling;
using Midas.Presentation.Symbols;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Midas.Presentation.Reels
{
	/// <summary>
	/// This controller implements a simple, standard reel spin.
	/// </summary>
	public sealed class SuspenseReelSpin : ReelSpinController
	{
		#region Fields

		private IReadOnlyList<ReelData> currentReelData;
		private readonly List<IReadOnlyList<ReelStrip>> reelStrips = new List<IReadOnlyList<ReelStrip>>();
		private readonly List<ReelSymbolList> symbolLists = new List<ReelSymbolList>();
		private Coroutine spinCoroutine;

		private bool isSpinning;
		private bool isSlamming;

		#endregion

		#region Inspector Fields

		[SerializeField]
		private ReelSpinTimings timings;

		/// <summary>
		/// The duration of the suspense spin.
		/// </summary>
		[SerializeField]
		private float suspenseDuration;

		#endregion

		#region Unity Hooks

		private void Awake()
		{
		}

		#endregion

		#region Overrides of ReelSpinController

		public override bool IsSpinning => isSpinning;

		/// <summary>
		/// Spin the reels, and stop them at the defined intervals.
		/// </summary>
		/// <param name="stage">The stage that the reel controller is attached to.</param>
		/// <param name="reelData">The reel data from the reel controller.</param>
		/// <param name="spinTime">The overall spin time to target, in seconds.</param>
		public override void SpinReels(Stage stage, IReadOnlyList<ReelData> reelData, TimeSpan spinTime)
		{
			currentReelData = reelData;
			if (IsSpinning)
			{
				Debug.LogError("Reels are already spinning");
				return;
			}

			reelStrips.Clear();
			symbolLists.Clear();

			foreach (var d in reelData)
			{
				reelStrips.Add(d.DataProvider.GetReelStrips(stage));
				symbolLists.Add(d.SymbolList);
			}

			isSpinning = true;
			isSlamming = false;
			spinCoroutine = StartCoroutine(DoSpin(spinTime));
		}

		public override void AbortSpin()
		{
			if (!IsSpinning)
				return;

			StopCoroutine(spinCoroutine);
			spinCoroutine = null;
			isSpinning = false;
			isSlamming = false;
		}

		public override void SlamReels(bool immediate)
		{
			if (!IsSpinning)
				return;

			isSlamming = true;
			foreach (var d in currentReelData)
			{
				foreach (var reel in d.ReelContainer.Reels)
					reel.Slam(immediate);

				if (d.ReelSpinStateEvent)
					d.ReelSpinStateEvent.SpinSlammed(d.ReelContainer);
			}
		}

		#endregion

		#region Private Methods

		private IEnumerator DoSpin(TimeSpan spinTime)
		{
			TimingDebug("Request reel spin: {0}", spinTime);
			var startTime = Time.time;

			// Kick off the reel spin.

			var reelGroups = currentReelData[0].ReelContainer.GetReelGroups();
			var reelGroupCount = reelGroups.Count;
			var anticipationReels = new bool[currentReelData[0].ReelContainer.Reels.Count];
			var anticipationGroups = new bool[reelGroupCount];
			var overrideTime = new TimeSpan?[reelGroupCount];

			// Determine what reels are going into suspense / anticipation.

			foreach (var rd in currentReelData)
			{
				rd.GetAnticipationMask(anticipationReels);
			}

			// This works assuming that reels inside reel container are defined in the same order as the symbol window result.

			for (var i = 0; i < reelGroups.Count; i++)
			{
				var reelGroup = reelGroups[i];
				foreach (var reel in reelGroup.Reels)
				{
					var reelIndex = currentReelData[0].ReelContainer.Reels.FindIndex(reel);
					anticipationGroups[i] |= anticipationReels[reelIndex];
				}

				if (anticipationGroups[i])
					overrideTime[i] = TimeSpan.FromSeconds(suspenseDuration);
			}

			var stopTime = timings.GetStopTimings(spinTime, reelGroupCount, overrideTime);
			var spinSettings = timings.GetSpinSettings(reelGroupCount);

			for (var index = 0; index < currentReelData.Count; index++)
			{
				var d = currentReelData[index];

				if (d.ReelSpinStateEvent)
				{
					var lockedReels = new List<Reel>();
					for (var i = 0; i < d.ReelContainer.Reels.Count; i++)
					{
						if (reelStrips[index][i] == null)
							lockedReels.Add(d.ReelContainer.Reels[i]);
					}

					d.ReelContainer.SpinAllReels(reelStrips[index], stopTime, spinSettings, symbolLists[index], reel => OnReelStateChanged(d, reel, reelGroups, lockedReels));
				}
				else
				{
					d.ReelContainer.SpinAllReels(reelStrips[index], stopTime, spinSettings, symbolLists[index], null);
				}

				if (d.ReelSpinStateEvent)
					d.ReelSpinStateEvent.SpinStarted(d.ReelContainer);
			}

			for (var index = 0; index < reelGroupCount; index++)
			{
				if (anticipationGroups[index] && !isSlamming)
				{
					foreach (var d in currentReelData)
					{
						if (d.ReelSpinStateEvent)
							d.ReelSpinStateEvent.ReelAnticipating(d.ReelContainer, index);
					}
				}

				foreach (var reel in reelGroups[index].Reels)
				{
					while (reel.SpinState != ReelSpinState.Idle)
						yield return null;
				}
			}

			foreach (var d in currentReelData)
			{
				if (d.ReelSpinStateEvent)
					d.ReelSpinStateEvent.SpinFinished(d.ReelContainer);
			}

			while (!AreContainersIdle())
				yield return null;

			TimingDebug("Spin Duration: {0}", (Time.time - startTime));

			spinCoroutine = null;
			reelStrips.Clear();
			isSpinning = false;

			void OnReelStateChanged(ReelData reelData, Reel reel, IReadOnlyList<ReelGroup> groups, IReadOnlyList<Reel> lockedReels)
			{
				var groupIndex = reel.Group;
				var newSpinState = reel.SpinState;

				for (var reelIndex = 0; reelIndex < groups[groupIndex].Reels.Count; reelIndex++)
				{
					var r = groups[groupIndex].Reels[reelIndex];
					if (lockedReels.Contains(r))
						continue;

					if (r.SpinState != newSpinState)
						return;
				}

				reelData.ReelSpinStateEvent.ReelStateChanged(reelData.ReelContainer, groupIndex, newSpinState);
			}
		}

		private bool AreContainersIdle()
		{
			foreach (var c in currentReelData)
			{
				if (!c.ReelContainer.IsIdle || c.ReelSpinStateEvent && !c.ReelSpinStateEvent.AreAllFinished())
					return false;
			}

			return true;
		}

		[Conditional("REEL_SPIN_TIMING")]
		private static void TimingDebug(string format, params object[] args)
		{
			Log.Instance.DebugFormat(format, args);
		}

		#endregion

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(ReelSpinTimings spinTimings, float suspenseDuration)
		{
			timings = spinTimings;
			this.suspenseDuration = suspenseDuration;
		}
#endif

		#endregion
	}
}