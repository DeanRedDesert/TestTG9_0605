using System;
using System.Collections.Generic;
using Logic.Core.Types;

namespace Logic.Types
{
	public sealed class RespinState : ILockData
	{
		/// <summary>
		/// The cells that have a frame.
		/// </summary>
		public ReadOnlyMask Frames { get; }

		/// <summary>
		/// The mask of the locked cells. Generated from <see cref="Locked"/>.
		/// </summary>
		public ReadOnlyMask LockedCells { get; }

		/// <summary>
		/// The symbols and cells that locked.
		/// </summary>
		public IReadOnlyList<ReadOnlyMask> Locked { get; }

		public RespinState(ReadOnlyMask frames, IReadOnlyList<ReadOnlyMask> locked)
		{
			Frames = frames;
			Locked = locked;
			LockedCells = CreateLockedCells(locked);
		}

		/// <summary>
		/// The mask of the cells that are locked.
		/// </summary>
		/// <returns>The locked cells.</returns>
		public ReadOnlyMask GetLockMask() => LockedCells;

		public int GetLockedSymbolIndexAt(int structureIndex)
		{
			for (var i = 0; i < Locked.Count; i++)
			{
				if (Locked[i][structureIndex])
					return i;
			}

			throw new InvalidOperationException($"Cell at structure index {structureIndex} is not locked");
		}

		#region Private Methods

		private ReadOnlyMask CreateLockedCells(IReadOnlyList<ReadOnlyMask> locked)
		{
			if (locked == null || locked.Count == 0)
				return ReadOnlyMask.CreateAllFalse(Frames.BitLength);

			var result = locked[0];

			for (var i = 1; i < locked.Count; i++)
				result = result.Or(locked[i]);

			return result;
		}

		#endregion
	}
}