namespace Midas.Presentation.ButtonHandling
{
	/// <summary>
	///     Struct holding data for updating the button states
	/// </summary>
	public sealed class ButtonStateData
	{
		#region Public

		public ButtonStateData(ButtonFunction buttonFunction, ButtonState buttonState, LightState lightState, object specificData)
		{
			ButtonFunction = buttonFunction;
			ButtonState = buttonState;
			LightState = lightState;
			SpecificData = specificData;
		}

		public override string ToString()
		{
			return $"{ButtonFunction}, {ButtonState}, {LightState}, {SpecificData}";
		}

		public ButtonFunction ButtonFunction { get; }
		public ButtonState ButtonState { get; }
		public LightState LightState { get; }
		public object SpecificData { get; }

		#endregion
	}
}