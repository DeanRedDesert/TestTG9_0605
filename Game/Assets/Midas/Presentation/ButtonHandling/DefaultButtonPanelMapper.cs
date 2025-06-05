using System.Collections.Generic;
using Midas.Core.Configuration;
using Midas.Presentation.Dashboard;
using Midas.Presentation.Gaff;
using Midas.Presentation.Gamble;
using Midas.Presentation.Stakes;

namespace Midas.Presentation.ButtonHandling
{
	public sealed class DefaultButtonPanelMapper : IButtonPanelMapper
	{
		public bool SupportsButtonPanel(ButtonPanelType panelType) => panelType == ButtonPanelType.Default;

		public IReadOnlyList<(PhysicalButton Button, ButtonFunction ButtonFunction)> CreatePhysicalButton2ButtonFunctionMapping(PhysicalButtons physicalButtons, ConfigData configuration)
		{
			// Map the Gamble physical button to a play button. It is expected to be used as a play button as it is the left smash button.
			return new[]
			{
				(physicalButtons.Cashout, DashboardButtonFunctions.Cashout),
				(physicalButtons.Ghost, GaffButtonFunctions.ToggleGaffMenu),
				(physicalButtons.LeftSmash, StakeButtonFunctions.Play),
				(physicalButtons.RightSmash, StakeButtonFunctions.Play),
				(physicalButtons.TakeWin, GambleButtonFunctions.TakeWin),
				(physicalButtons.Gamble, StakeButtonFunctions.Play),
				(physicalButtons.Info, DashboardButtonFunctions.Info),
				(physicalButtons.Row11, StakeButtonFunctions.Bet1),
				(physicalButtons.Row12, StakeButtonFunctions.Bet2),
				(physicalButtons.Row13, StakeButtonFunctions.Bet3),
				(physicalButtons.Row14, StakeButtonFunctions.Bet4),
				(physicalButtons.Row15, StakeButtonFunctions.Bet5),
				(physicalButtons.Row21, StakeButtonFunctions.Play1),
				(physicalButtons.Row22, StakeButtonFunctions.Play2),
				(physicalButtons.Row23, StakeButtonFunctions.Play3),
				(physicalButtons.Row24, StakeButtonFunctions.Play4),
				(physicalButtons.Row25, StakeButtonFunctions.Play5)
			};
		}
	}
}