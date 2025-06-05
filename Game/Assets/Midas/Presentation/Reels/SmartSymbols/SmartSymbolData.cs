using System;
using System.Collections.Generic;

namespace Midas.Presentation.Reels.SmartSymbols
{
	public sealed class SmartSymbolData
	{
		public static readonly SmartSymbolData Empty = new SmartSymbolData(Array.Empty<(int, int)>(), Array.Empty<bool>());

		/// <summary>
		/// All the cells that have some kind of smart action for this trigger.
		/// </summary>
		public IReadOnlyList<(int Column, int Row)> SmartCells { get; }

		/// <summary>
		/// The mask of reels that should have anticipation
		/// </summary>
		public IReadOnlyList<bool> AnticipationMask { get; }

		/// <summary>
		/// Null if not qualified, otherwise which reel causes the game to qualify for a trigger?
		/// </summary>
		public int? QualifyingReelIndex { get; }

		/// <summary>
		/// If qualified, how many reels are involved in the trigger?
		/// </summary>
		/// <remarks>This allows a game to choose a different set of sounds if there are more reels involved in a trigger</remarks>
		public int InvolvedReels { get; }

		public SmartSymbolData(IReadOnlyList<(int Column, int Row)> smartCells, IReadOnlyList<bool> anticipationMask, int? qualifyingReelIndex = null, int involvedReels = 0)
		{
			SmartCells = smartCells;
			AnticipationMask = anticipationMask;
			QualifyingReelIndex = qualifyingReelIndex;
			InvolvedReels = involvedReels;
		}
	}
}