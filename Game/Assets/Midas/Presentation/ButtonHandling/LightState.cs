namespace Midas.Presentation.ButtonHandling
{
	/// <summary>
	///     Light states defines the light state of a button used for buttons
	/// </summary>
	public sealed class LightState
	{
		public enum LampState
		{
			LightFlash,
			LightOn,
			LightOff
		}

		public LampState State { get; }
		public float Frequency { get; }

		public static LightState On { get; } = new LightState(LampState.LightOn, 0);
		public static LightState FlashSlow { get; } = new LightState(LampState.LightFlash, 1.0f / 0.8f);
		public static LightState FlashMedium { get; } = new LightState(LampState.LightFlash, 1.0f / 0.6f);
		public static LightState FlashFast { get; } = new LightState(LampState.LightFlash, 1.0f / 0.4f);
		public static LightState Off { get; } = new LightState(LampState.LightOff, 0);

		public LightState(LampState lampState, float frequency)
		{
			State = lampState;
			Frequency = frequency;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hash = 17;
				// Suitable nullity checks etc, of course :)
				hash = hash * 23 + Frequency.GetHashCode();
				hash = hash * 23 + ((int) State).GetHashCode();
				return hash;
			}
		}

		public override string ToString()
		{
			return $"{State}, {Frequency}";
		}
	}
}