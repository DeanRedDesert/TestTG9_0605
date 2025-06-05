namespace Midas.Presentation.Cabinet
{
	public enum MonitorRole
	{
		Main,
		Top,
		BellyGlass,
		ButtonPanel,
		Topper,
		VideoWall
	}

	public enum MonitorAspect
	{
		Standard,
		Widescreen,
		Ultrawide,
		Portrait
	}

	/// <summary>
	/// Holds information about the monitors.
	/// </summary>
	public sealed class MonitorConfig
	{
		public MonitorRole Role { get; }
		public MonitorAspect Aspect { get; }

		public MonitorConfig(MonitorRole role, MonitorAspect aspect)
		{
			Role = role;
			Aspect = aspect;
		}

		public override string ToString() => $"R:{Role}, A:{Aspect}";
	}
}