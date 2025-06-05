using IGT.Game.Core.Communication.Cabinet;
using Midas.Presentation.ButtonHandling;
using ButtonPanelType = Midas.Presentation.ButtonHandling.ButtonPanelType;

namespace Midas.Ascent.Cabinet
{
	internal sealed class ButtonPanel
	{
		public uint PanelIdentifier { get; }
		public ButtonPanelType PanelType { get; }
		public PhysicalButtons PhysicalButtons { get; }

		public ButtonPanel(uint panelIdentifier, ButtonPanelType panelType)
			: this(panelIdentifier, panelType, CreateDefaultPhysicalButtons())
		{
		}

		private ButtonPanel(uint panelIdentifier, ButtonPanelType panelType, PhysicalButtons physicalButtons)
		{
			PanelIdentifier = panelIdentifier;
			PanelType = panelType;
			PhysicalButtons = physicalButtons;
		}

		private static PhysicalButtons CreateDefaultPhysicalButtons()
		{
			return new PhysicalButtons
			(
				new PhysicalButton((int)SwitchId.CashOutId),
				new PhysicalButton((int)SwitchId.GhostId),
				new PhysicalButton((int)SwitchId.MaxBetId),
				new PhysicalButton((int)SwitchId.RepeatBetId),
				new PhysicalButton((int)SwitchId.TakeWinId),
				new PhysicalButton((int)SwitchId.DoubleUpId),
				new PhysicalButton((int)SwitchId.InformationId),
				new PhysicalButton((int)SwitchId.PlayNCredits1Id),
				new PhysicalButton((int)SwitchId.PlayNCredits2Id),
				new PhysicalButton((int)SwitchId.PlayNCredits3Id),
				new PhysicalButton((int)SwitchId.PlayNCredits4Id),
				new PhysicalButton((int)SwitchId.PlayNCredits5Id),
				new PhysicalButton((int)SwitchId.SelectNLines1Id),
				new PhysicalButton((int)SwitchId.SelectNLines2Id),
				new PhysicalButton((int)SwitchId.SelectNLines3Id),
				new PhysicalButton((int)SwitchId.SelectNLines4Id),
				new PhysicalButton((int)SwitchId.SelectNLines5Id)
			);
		}

		public bool HasLamp(PhysicalButton button)
		{
			return PhysicalButtons.Ghost != button;
		}
	}
}