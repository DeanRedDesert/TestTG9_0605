using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ButtonHandling;
using Midas.Presentation.Data;

namespace Midas.Presentation.Stakes
{
	public enum StakeMode
	{
		/// <summary>
		/// Bet multiplier top row, stake options bottom row.
		/// </summary>
		ClassicBetDominant,

		/// <summary>
		/// Stake options top row, bet multiplier bottom row.
		/// </summary>
		ClassicStakeDominant,

		/// <summary>
		/// All buttons have bet and stake options.
		/// </summary>
		CostToCover
	}

	public sealed class StakesStatus : StatusBlock
	{
		private StatusProperty<StakeMode> stakeMode;
		private StatusProperty<bool> playFromPlayButtonOnly;
		private StatusProperty<IReadOnlyList<ButtonFunction>> betButtonFunctions;
		private StatusProperty<IReadOnlyList<ButtonFunction>> playButtonFunctions;
		private StatusProperty<IReadOnlyList<ButtonFunction>> stakeButtonFunctions;

		private StatusProperty<IReadOnlyList<(long stakeMultiplier, IReadOnlyList<int>)>> stakeGroups;
		private StatusProperty<int> selectedStakeGroup;
		private StatusProperty<int> betButtonCount;
		private StatusProperty<int> playButtonCount;
		private StatusProperty<int> stakeButtonCount;

		public StakeMode StakeMode
		{
			get => stakeMode.Value;
			set => stakeMode.Value = value;
		}

		public bool PlayFromPlayButtonOnly
		{
			get => playFromPlayButtonOnly.Value;
			set => playFromPlayButtonOnly.Value = value;
		}

		public IReadOnlyList<ButtonFunction> BetButtonFunctions
		{
			get => betButtonFunctions.Value;
			set => betButtonFunctions.Value = value;
		}

		public IReadOnlyList<ButtonFunction> PlayButtonFunctions
		{
			get => playButtonFunctions.Value;
			set => playButtonFunctions.Value = value;
		}

		public IReadOnlyList<ButtonFunction> StakeButtonFunctions
		{
			get => stakeButtonFunctions.Value;
			set
			{
				stakeButtonFunctions.Value = value;
				stakeButtonCount.Value = value?.Count ?? 0;
			}
		}

		public IReadOnlyList<(long stakeMultiplier, IReadOnlyList<int> stakeCombinationIndices)> StakeGroups
		{
			get => stakeGroups.Value;
			set => stakeGroups.Value = value;
		}

		public int SelectedStakeGroup
		{
			get => selectedStakeGroup.Value;
			set => selectedStakeGroup.Value = value;
		}

		public int BetButtonCount
		{
			get => betButtonCount.Value;
			set => betButtonCount.Value = value;
		}

		public int PlayButtonCount
		{
			get => playButtonCount.Value;
			set => playButtonCount.Value = value;
		}

		public int StakeButtonCount
		{
			get => stakeButtonCount.Value;
			set => stakeButtonCount.Value = value;
		}

		public StakesStatus() : base(nameof(StakesStatus))
		{
		}

		protected override void DoResetProperties()
		{
			stakeMode = AddProperty(nameof(StakeMode), StakeMode.ClassicBetDominant);
			playFromPlayButtonOnly = AddProperty(nameof(PlayFromPlayButtonOnly), false);
			betButtonFunctions = AddProperty(nameof(BetButtonFunctions), (IReadOnlyList<ButtonFunction>)Array.Empty<ButtonFunction>());
			playButtonFunctions = AddProperty(nameof(PlayButtonFunctions), (IReadOnlyList<ButtonFunction>)Array.Empty<ButtonFunction>());
			stakeButtonFunctions = AddProperty(nameof(StakeButtonFunctions), (IReadOnlyList<ButtonFunction>)Array.Empty<ButtonFunction>());

			stakeGroups = AddProperty(nameof(StakeGroups), default(IReadOnlyList<(long stakeMultiplier, IReadOnlyList<int>)>));
			selectedStakeGroup = AddProperty(nameof(SelectedStakeGroup), 0);
			betButtonCount = AddProperty(nameof(BetButtonCount), 0);
			playButtonCount = AddProperty(nameof(PlayButtonCount), 0);
			stakeButtonCount = AddProperty(nameof(StakeButtonCount), 0);
		}
	}
}