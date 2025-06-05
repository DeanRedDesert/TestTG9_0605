using System.Collections.Generic;
using Midas.Core;
using Midas.Core.LogicServices;

namespace Midas.LogicToPresentation.Data.Services
{
	public sealed class BetService : CompositeGameService
	{
		internal readonly GameService<IReadOnlyList<IStakeCombination>> StakeCombosService = new GameService<IReadOnlyList<IStakeCombination>>(HistorySnapshotType.None);
		internal readonly GameService<int> SelectedStakeComboService = new GameService<int>(HistorySnapshotType.GameStart);

		public IGameServiceConsumer<IReadOnlyList<IStakeCombination>> StakeCombos => StakeCombosService.Variable;
		public IGameServiceConsumer<int> SelectedStakeCombo => SelectedStakeComboService.Variable;

		protected override void CreateServices()
		{
			AddService(StakeCombosService, nameof(StakeCombos));
			AddService(SelectedStakeComboService, nameof(SelectedStakeCombo));
		}
	}
}