using System;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Types;
using Logic.Core.WinCheck;
using Midas.Presentation.Reels.SmartSymbols;
using Midas.Presentation.StageHandling;

namespace Midas.Gle.Presentation.SmartSymbols
{
	// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global - Intended to be overridden if a game needs to filter symbols.
	public class LockedSymbolSmartSymbolDetector : SmartSymbolDetector
	{
		private readonly string resultName;
		private readonly string lockDataResult;

		public LockedSymbolSmartSymbolDetector(Stage stage, string resultName, string lockDataResult) : base(stage)
		{
			this.resultName = resultName;
			this.lockDataResult = lockDataResult;
		}

		protected override SmartSymbolData FindSmartSymbols()
		{
			var results = GleGameController.GleStatus.CurrentGameResult.Current.StageResults;
			var (swr, lockMask) = results.GetReelDetails(resultName);
			return FindSmartSymbols(swr, lockMask, GetNewLockData());

			ReadOnlyMask GetNewLockData()
			{
				var result = results.First(r => r.Name == lockDataResult);
				return ((ILockData)result.Value).GetLockMask();
			}
		}

		protected virtual bool IsEligibleCell(SymbolWindowResult symbolWindowResult, Cell cell) => true;

		private SmartSymbolData FindSmartSymbols(SymbolWindowResult symbolWindowResult, ReadOnlyMask initLockMask, ReadOnlyMask newLockMask)
		{
			var newLocks = newLockMask.And(initLockMask.Not());
			var cells = symbolWindowResult.SymbolWindowStructure.IndexesToCells(newLocks.EnumerateIndexes().ToArray());

			var eligibleCells = new List<(int Column, int Row)>();
			foreach (var cell in cells)
			{
				if (IsEligibleCell(symbolWindowResult, cell))
					eligibleCells.Add((cell.Column, cell.Row));
			}

			return new SmartSymbolData(eligibleCells, Array.Empty<bool>());
		}
	}
}