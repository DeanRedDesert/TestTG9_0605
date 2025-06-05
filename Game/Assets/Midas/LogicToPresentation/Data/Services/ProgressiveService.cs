using System.Collections.Generic;
using Midas.Core;
using Midas.Core.General;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class ProgressiveService : CompositeGameService
	{
		#region Nested Types

		private sealed class BroadcastDataComparer : IEqualityComparer<IReadOnlyList<(string LevelId, Money Amount)>>
		{
			public bool Equals(IReadOnlyList<(string LevelId, Money Amount)> x, IReadOnlyList<(string LevelId, Money Amount)> y)
			{
				if (ReferenceEquals(x, y)) return true;
				if (x == null || y == null) return false;
				if (x.Count != y.Count)
					return false;

				for (var i = 0; i < x.Count; i++)
				{
					if (x[i].LevelId != y[i].LevelId || x[i].Amount != y[i].Amount)
						return false;
				}

				return true;
			}

			public int GetHashCode(IReadOnlyList<(string LevelId, Money Amount)> obj)
			{
				var hashCode = 0;

				foreach (var val in obj)
				{
					hashCode = (hashCode * 397) ^ val.LevelId.GetHashCode();
					hashCode = (hashCode * 397) ^ val.Amount.GetHashCode();
				}

				return hashCode;
			}
		}

		#endregion

		private static readonly BroadcastDataComparer broadcastDataComparer = new BroadcastDataComparer();

		internal readonly GameService<IReadOnlyList<(string LevelId, Money Value)>> BroadcastDataService = new GameService<IReadOnlyList<(string LevelId, Money Amount)>>(HistorySnapshotType.GameCycle, broadcastDataComparer);
		internal readonly GameService<IReadOnlyList<ProgressiveLevel>> ProgressiveLevelsService = new GameService<IReadOnlyList<ProgressiveLevel>>(HistorySnapshotType.None);
		internal readonly GameService<IReadOnlyList<ProgressiveAwardServiceData>> ProgressiveAwardsService = new GameService<IReadOnlyList<ProgressiveAwardServiceData>>(HistorySnapshotType.None);
		internal readonly GameService<Money> TotalProgressiveAwardInGameService = new GameService<Money>(HistorySnapshotType.None);

		public IGameServiceConsumer<IReadOnlyList<(string LevelId, Money Value)>> BroadcastData => BroadcastDataService.Variable;
		public IGameServiceConsumer<IReadOnlyList<ProgressiveAwardServiceData>> ProgressiveAwards => ProgressiveAwardsService.Variable;
		public IGameServiceConsumer<IReadOnlyList<ProgressiveLevel>> ProgressiveLevels => ProgressiveLevelsService.Variable;
		public IGameServiceConsumer<Money> TotalProgressiveAwardInGame => TotalProgressiveAwardInGameService.Variable;

		protected override void CreateServices()
		{
			AddService(BroadcastDataService, nameof(BroadcastData));
			AddService(ProgressiveAwardsService, nameof(ProgressiveAwards));
			AddService(ProgressiveLevelsService, nameof(ProgressiveLevels));
			AddService(TotalProgressiveAwardInGameService, nameof(TotalProgressiveAwardInGame));
		}
	}

	public enum ProgressiveAwardState
	{
		/// <summary>
		/// Progressive has been triggered by the logic and waiting for acknowledgement from the foundation.
		/// </summary>
		Pending,

		/// <summary>
		/// Progressive has been triggered, logic is waiting for start message from presentation.
		/// </summary>
		Triggered,

		/// <summary>
		/// Progressive award has been started, logic is waiting for verified message from foundation.
		/// </summary>
		Starting,

		/// <summary>
		/// Progressive has been verified by foundation, logic is waiting for display finished message from presentation.
		/// </summary>
		Verified,

		/// <summary>
		/// Progressive has finished display, logic is waiting for paid message from foundation.
		/// </summary>
		FinishedDisplay,

		/// <summary>
		/// Progressive has been paid, logic is waiting for cleared message from presentation.
		/// </summary>
		Paid,

		/// <summary>
		/// Progressive is paid and the cleared message has been received.
		/// </summary>
		Cleared
	}

	public sealed class ProgressiveAwardServiceData
	{
		public ProgressiveHit Hit { get; }
		public ProgressiveAwardState State { get; }
		public Money DisplayAmount { get; }

		/// <summary>
		/// Set to true when the display amount has been verified by the foundation and can be shown to the player as the awarded amount.
		/// UGP -> Verified when the state is ProgressiveAwardState.Verified.
		/// Ascent -> Verified when the state is ProgressiveAwardState.Triggered.
		/// </summary>
		public bool IsDisplayAmountVerified { get; }

		public ProgressiveAwardServiceData(ProgressiveHit hit, ProgressiveAwardState state, Money displayAmount, bool isDisplayAmountVerified)
		{
			Hit = hit;
			State = state;
			DisplayAmount = displayAmount;
			IsDisplayAmountVerified = isDisplayAmountVerified;
		}
	}
}