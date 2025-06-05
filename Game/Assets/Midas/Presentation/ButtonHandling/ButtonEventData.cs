namespace Midas.Presentation.ButtonHandling
{
	/// <summary>
	///     Data which is send with a button event
	/// </summary>
	public sealed class ButtonEventData
	{
		#region Public

		public ButtonEventData(ButtonFunction buttonFunction, ButtonEvent buttonEvent, PhysicalButton physicalButton, uint panelIdentifier)
		{
			ButtonFunction = buttonFunction;
			ButtonEvent = buttonEvent;
			PhysicalButton = physicalButton;
			PanelIdentifier = panelIdentifier;
		}

		public ButtonEventData(ButtonFunction buttonFunction, ButtonEvent buttonEvent)
		{
			ButtonFunction = buttonFunction;
			ButtonEvent = buttonEvent;
			PhysicalButton = PhysicalButton.Undefined;
			PanelIdentifier = 0xFFFFFFFF;
		}

		public override int GetHashCode()
		{
			var hash = 17;
			hash = hash * 23 + ButtonFunction.GetHashCode();
			hash = hash * 23 + ButtonEvent.GetHashCode();
			hash = hash * 23 + PhysicalButton.GetHashCode();
			hash = hash * 23 + PanelIdentifier.GetHashCode();
			return hash;
		}

		public override string ToString()
		{
			return $"{ButtonFunction}, {ButtonEvent}, {PhysicalButton}, {PanelIdentifier}";
		}

		public ButtonFunction ButtonFunction { get; }
		public ButtonEvent ButtonEvent { get; }

		public PhysicalButton PhysicalButton { get; }
		public uint PanelIdentifier { get; }
		public bool IsPressed => ButtonEvent == ButtonEvent.Pressed;
		public bool HasPhysicalOrigins => PhysicalButton != PhysicalButton.Undefined;

		#endregion
	}
}