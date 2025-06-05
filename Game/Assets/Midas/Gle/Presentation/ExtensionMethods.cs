using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Engine;
using Logic.Core.Types;
using Midas.Core;

namespace Midas.Gle.Presentation
{
	public static class ExtensionMethods
	{
		/// <summary>
		/// Extracts the correct progressive prize from the results for the progressive hit.
		/// </summary>
		/// <param name="results">The stage results that the hit is associated with.</param>
		/// <param name="hit">The progressive hit to find the prize of.</param>
		/// <returns>Null if no hit was associated, or the progressive prize result.</returns>
		public static ProgressivePrizeResult GetProgressivePrizeForHit(this StageResults results, ProgressiveHit hit)
		{
			if (hit.Source == null || !int.TryParse(hit.SourceDetails, out var hitIndex))
				return null;

			var r = results.Single(r => r.Name == hit.Source);
			return ((IReadOnlyList<ProgressivePrizeResult>)r.Value)[hitIndex];
		}

		/// <summary>
		/// Get reel details out of stage results.
		/// </summary>
		public static (SymbolWindowResult SymbolWindowResult, ReadOnlyMask LockMask) GetReelDetails(this StageResults results, string resultName)
		{
			var result = results.First(r => r.Name == resultName);

			ReadOnlyMask lockMask;
			SymbolWindowResult symWindow;

			switch (result.Value)
			{
				case SymbolWindowResult swr:
					lockMask = ReadOnlyMask.CreateAllFalse(swr.SymbolWindowStructure.Cells.Count);
					symWindow = swr;
					break;

				case LockedSymbolWindowResult lswr:
					lockMask = lswr.LockMask;
					symWindow = lswr.SymbolWindowResult;
					break;

				default:
					throw new InvalidOperationException("result must be either SymbolWindowResult or LockedReelSymbolWindowResult");
			}

			return (symWindow, lockMask);
		}
	}
}