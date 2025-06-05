using System.Collections.Generic;
using Midas.Core.Configuration;

namespace Midas.Presentation.ButtonHandling
{
	using PhysicalButton2ButtonFunctionList = IReadOnlyList<(PhysicalButton Button, ButtonFunction ButtonFunction)>;

	public interface IButtonPanelMapper
	{
		public bool SupportsButtonPanel(ButtonPanelType panelType);
		public PhysicalButton2ButtonFunctionList CreatePhysicalButton2ButtonFunctionMapping(PhysicalButtons physicalButtons, ConfigData configuration);
	}
}