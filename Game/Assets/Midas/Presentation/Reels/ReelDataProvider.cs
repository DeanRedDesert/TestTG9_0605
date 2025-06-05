using System.Collections.Generic;
using Midas.Presentation.StageHandling;
using UnityEngine;

namespace Midas.Presentation.Reels
{
	public abstract class ReelDataProvider : ScriptableObject
	{
		public abstract IReadOnlyList<ReelStrip> GetInitReelStrips(Stage stage, ReelContainer reelContainer);

		public abstract IReadOnlyList<ReelStrip> GetReelStrips(Stage stage);
	}
}