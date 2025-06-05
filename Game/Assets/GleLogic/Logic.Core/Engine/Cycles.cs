using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Engine
{
	/// <summary>
	/// A collection of CycleStates for maintaining all the past and future cycles played in a single game.
	/// This is an immutable class that can be modified using the selection of public methods.
	/// </summary>
	public sealed class Cycles : IReadOnlyList<CycleState>, IToString
	{
		private readonly int nextId;
		private readonly int currentIndex;

		private readonly IReadOnlyList<CycleState> cycleStates;

		public CycleState this[int i] => cycleStates[i];

		public int Count => cycleStates.Count;

		public IEnumerator<CycleState> GetEnumerator() => cycleStates.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private Cycles(IReadOnlyList<CycleState> cycleStates, int currentIndex, int nextId)
		{
			if (cycleStates.Count < 1) throw new ArgumentException("Need at least one cycle state", nameof(cycleStates));
			if (currentIndex < 0) throw new ArgumentException("Invalid current index", nameof(currentIndex));

			this.cycleStates = cycleStates;
			this.currentIndex = currentIndex;
			this.nextId = nextId;
		}

		/// <summary>
		/// Create a fresh Cycles object with 1 game to play in the initial stage.
		/// </summary>
		public static Cycles CreateInitial(string initialStage) => new Cycles(new[]
		{
			new CycleState(0, null, initialStage, null, 1, 0)
		}, 0, 1);

		/// <summary>
		/// Plays one cycles of the current Cycles object and returns the new Cycles object after the game is played.
		/// This is called by the runner after every cycle, but before the triggers are processed.
		/// That way any triggers that occur cannot modify the cycle that triggered them.
		/// </summary>
		public Cycles PlayOne()
		{
			var curr = Current;
			var newBatches = new List<CycleState>(cycleStates)
			{
				[currentIndex] = new CycleState(curr.Id, curr.TriggeringCycle, curr.Stage, curr.CycleId, curr.TotalCycles, curr.CompletedCycles + 1)
			};
			return new Cycles(newBatches, currentIndex, nextId);
		}

		/// <summary>
		/// Moves to the next cycle.
		/// This is called by the runner after every cycle, and after all the triggers are processed.
		/// </summary>
		public Cycles MoveNext() => new Cycles(cycleStates, currentIndex + (cycleStates[currentIndex].IsFinished ? 1 : 0), nextId);

		/// <summary>
		/// Returns true if the all Cycles are finished. This will end the Cycles for the current Game.
		/// </summary>
		public bool IsFinished => Current == null;

		/// <summary>
		/// Gets the currently active CycleState, or null if the Cycles are finished.
		/// </summary>
		public CycleState Current => currentIndex < cycleStates.Count ? cycleStates[currentIndex] : null;

		/// <summary>
		/// Insert and activate a new CycleState immediately before the current CycleState, essentially interrupting the current CycleState, which will be completed after the new one is completed.
		/// If the current CycleState is already finished then the new CycleState will be inserted directly after it.
		/// </summary>
		/// <param name="triggeringCycle">The CycleState that triggered the insert.</param>
		/// <param name="stage">The stage to process during the new CycleState.</param>
		/// <param name="cycleId">The CycleId of new CycleState.</param>
		/// <param name="totalCycles">The total individual cycles for new CycleState.</param>
		/// <param name="completedCycles">The currently completed individual cycles for new CycleState.</param>
		/// <returns>A modified copy of the current Cycles object.</returns>
		public Cycles InsertImmediately(CycleState triggeringCycle, string stage, string cycleId, int totalCycles, int completedCycles = 0)
			=> InsertAtIndex(currentIndex, triggeringCycle, stage, cycleId, totalCycles, completedCycles);

		/// <summary>
		/// Insert a new CycleState immediately after the current CycleState. To be played once the current CycleState is finished.
		/// </summary>
		/// <param name="triggeringCycle">The CycleState that triggered the insert.</param>
		/// <param name="stage">The stage to process during the new CycleState.</param>
		/// <param name="cycleId">The CycleId of new CycleState.</param>
		/// <param name="totalCycles">The total individual cycles for new CycleState.</param>
		/// <param name="completedCycles">The currently completed individual cycles for new CycleState.</param>
		/// <returns>A modified copy of the current Cycles object.</returns>
		public Cycles InsertAtNext(CycleState triggeringCycle, string stage, string cycleId, int totalCycles, int completedCycles = 0)
			=> InsertAtIndex(currentIndex + 1, triggeringCycle, stage, cycleId, totalCycles, completedCycles);

		/// <summary>
		/// Insert a new CycleState at the end of all the current CycleStates.
		/// </summary>
		/// <param name="triggeringCycle">The CycleState that triggered the insert.</param>
		/// <param name="stage">The stage to process during the new CycleState.</param>
		/// <param name="cycleId">The CycleId of new CycleState.</param>
		/// <param name="totalCycles">The total individual cycles for new CycleState.</param>
		/// <param name="completedCycles">The currently completed individual cycles for new CycleState.</param>
		/// <returns>A modified copy of the current Cycles object.</returns>
		public Cycles InsertAtEnd(CycleState triggeringCycle, string stage, string cycleId, int totalCycles, int completedCycles = 0)
			=> InsertAtIndex(cycleStates.Count, triggeringCycle, stage, cycleId, totalCycles, completedCycles);

		/// <summary>
		/// Insert a new CycleState at the given index.
		/// The index logic is tough to get right, where possible prefer using InsertImmediately, InsertAtNext and InsertAtEnd.
		/// 
		/// </summary>
		/// <param name="index">The index to insert at.</param>
		/// <param name="triggeringCycle">The CycleState that triggered the insert.</param>
		/// <param name="stage">The stage to process during the new CycleState.</param>
		/// <param name="cycleId">The CycleId of new CycleState.</param>
		/// <param name="totalCycles">The total individual cycles for new CycleState.</param>
		/// <param name="completedCycles">The currently completed individual cycles for new CycleState.</param>
		/// <returns>A modified copy of the current Cycles object.</returns>
		/// <exception cref="Exception">Throws an exception unless the index is on or after the index of the current CycleState.</exception>
		// ReSharper disable once MemberCanBePrivate.Global - Helper method
		public Cycles InsertAtIndex(int index, CycleState triggeringCycle, string stage, string cycleId, int totalCycles, int completedCycles = 0)
		{
			if (index < currentIndex)
				throw new Exception("Cannot insert a CycleBatch before the current, no rewriting history");

			var newBatch = new CycleState(nextId, triggeringCycle, stage, cycleId, totalCycles, completedCycles);
			var newBatches = new List<CycleState>(cycleStates);
			var newCurrentIndex = currentIndex;

			if (index == currentIndex)
			{
				var currentBatch = newBatches[index];
				newCurrentIndex++;
				index++;

				// If the current batch still has remaining cycles, insert it after the new batch.

				if (!currentBatch.IsFinished)
					newBatches.Insert(index, currentBatch);
			}

			newBatches.Insert(Math.Min(index, cycleStates.Count), newBatch);
			return new Cycles(newBatches, newCurrentIndex, nextId + 1);
		}

		/// <summary>
		/// Replace the TotalCycles and the CompletedCycles of the Current CycleState.
		/// </summary>
		/// <param name="totalCycles">The new value for the total individual cycles.</param>
		/// <param name="completedCycles">The new value for the completed individual cycles.</param>
		/// <returns>A modified copy of the current Cycles object.</returns>
		/// <exception cref="Exception">Throws an exception if the Current CycleState is null</exception>
		public Cycles ReplaceCurrent(int totalCycles, int completedCycles)
		{
			var curr = Current;

			if (curr == null)
				throw new Exception("No current cycle to replace");

			if (curr.TotalCycles == totalCycles && curr.CompletedCycles == completedCycles)
				return this;

			var newBatches = new List<CycleState>(cycleStates)
			{
				[currentIndex] = new CycleState(curr.Id, curr.TriggeringCycle, curr.Stage, curr.CycleId, totalCycles, completedCycles)
			};
			return new Cycles(newBatches, currentIndex, nextId);
		}

		/// <summary>
		/// Replace the TotalCycles and the CompletedCycles of the CycleState at the given index.
		/// </summary>
		/// <param name="index">The index to change.</param>
		/// <param name="totalCycles">The new value for the total individual cycles.</param>
		/// <param name="completedCycles">The new value for the completed individual cycles.</param>
		/// <returns>A modified copy of the current Cycles object.</returns>
		/// <exception cref="Exception">Throws an exception unless the index is on or after the index of the current CycleState.</exception>
		public Cycles ReplaceAtIndex(int index, int totalCycles, int completedCycles)
		{
			if (index < currentIndex)
				throw new Exception("Cannot replace a CycleBatch that is before the current, no rewriting history");

			var curr = cycleStates[index];

			if (curr.TotalCycles == totalCycles && curr.CompletedCycles == completedCycles)
				return this;

			var newBatches = new List<CycleState>(cycleStates)
			{
				[index] = new CycleState(curr.Id, curr.TriggeringCycle, curr.Stage, curr.CycleId, totalCycles, completedCycles)
			};
			return new Cycles(newBatches, currentIndex, nextId);
		}

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				return this.Select((cs, i) => $"{(i == currentIndex ? "»" : " ")} {C(cs, i)}")
					.Join()
					.ToSuccess();

				string A(CycleState cs, int i) =>
					$"{cs.CompletedCycles + (i < currentIndex ? 0 : 1)}/{cs.TotalCycles}";

				string B(CycleState cs, int i) =>
					$"#{cs.Id} {cs.Stage} {(string.IsNullOrEmpty(cs.CycleId) ? "" : $"'{cs.CycleId}' ")}{A(cs, i)}";

				string C(CycleState cs, int i) =>
					cs.TriggeringCycle == null ? B(cs, i) : $"{B(cs, i)} (trigger #{cs.TriggeringCycle.Id} {cs.TriggeringCycle.CompletedCycles + 1}/{cs.TriggeringCycle.TotalCycles})";
			}

			return new NotSupported();
		}
	}
}