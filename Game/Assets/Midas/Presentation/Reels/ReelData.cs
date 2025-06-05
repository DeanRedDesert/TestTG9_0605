using System;
using System.Collections.Generic;
using Midas.Presentation.Data.PropertyReference;
using Midas.Presentation.Symbols;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	[Serializable]
	public sealed class ReelData
	{
		[SerializeField]
		private ReelDataProvider dataProvider;

		[SerializeField]
		private ReelSymbolList symbolList;

		[SerializeField]
		private ReelContainer reelContainer;

		[SerializeField]
		private ReelSpinStateEvent reelSpinStateEvent;

		[SerializeField]
		[Tooltip("These masks are or-ed together to get the final anticipation mask")]
		private PropertyReference<IReadOnlyList<bool>>[] anticipationMasks;

		public ReelDataProvider DataProvider => dataProvider;
		public ReelContainer ReelContainer => reelContainer;
		public ReelSymbolList SymbolList => symbolList;
		public ReelSpinStateEvent ReelSpinStateEvent => reelSpinStateEvent;

		public ReelData(ReelDataProvider dataProvider, ReelSymbolList symbolList, ReelContainer reelContainer, ReelSpinStateEvent reelSpinStateEvent)
		{
			this.dataProvider = dataProvider;
			this.symbolList = symbolList;
			this.reelContainer = reelContainer;
			this.reelSpinStateEvent = reelSpinStateEvent;
			anticipationMasks = Array.Empty<PropertyReference<IReadOnlyList<bool>>>();
		}

		public void DeInit()
		{
			if (anticipationMasks != null)
			{
				foreach (var pr in anticipationMasks)
					pr.DeInit();
			}
		}

		public void GetAnticipationMask(bool[] fullAnticipationMask)
		{
			if (anticipationMasks == null)
				return;

			foreach (var maskProp in anticipationMasks)
			{
				var mask = maskProp.Value;

				if (mask == null || mask.Count == 0)
					continue;

				for (var i = 0; i < mask.Count; i++)
					fullAnticipationMask[i] |= mask[i];
			}
		}
	}
}