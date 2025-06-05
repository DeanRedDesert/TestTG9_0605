namespace Midas.Core
{
	public sealed class ExternalJackpot
	{
		/// <summary>
		/// The name of the external jackpot.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The current value of the external jackpot.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Gets whether the jackpot is currently visible.
		/// </summary>
		public bool IsVisible { get; }

		/// <summary>
		/// The icon ID for the jackpot.
		/// </summary>
		public int IconId { get; }

		public ExternalJackpot(string name, string value, bool isVisible, int iconId)
		{
			Name = name;
			Value = value;
			IsVisible = isVisible;
			IconId = iconId;
		}

		public override string ToString() => $"{Name}: {Value} (Visible: {IsVisible}, Icon: {IconId})";
	}
}