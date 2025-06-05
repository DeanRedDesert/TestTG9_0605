namespace Midas.Presentation.General
{
	/// <summary>
	/// Modes for formatting credits with a digit group separator.
	/// </summary>
	public enum CreditDisplaySeparatorMode
	{
		/// <summary>
		/// Pick the mode automatically based on "CurrencyConfig.UseCreditSeparator"/>.
		/// <see cref="SeparateAsCash" /> if "CurrencyConfig.UseCreditSeparator"/> is true,
		/// <see cref="NoSeparator" /> if it is false.
		/// </summary>
		Auto,

		/// <summary>
		/// Do not include a separator.
		/// This mode ignores the value of "CurrencyConfig.UseCreditSeparator"/>.
		/// </summary>
		NoSeparator,

		/// <summary>
		/// Separate the same as cash.
		/// This mode ignores the value of "CurrencyConfig.UseCreditSeparator"/>.
		/// </summary>
		SeparateAsCash,

		/// <summary>
		/// Only separate if the number is greater than four digits.
		/// The Separator will sill appear after the third digit (10,000 1000).
		/// This mode ignores the value of "CurrencyConfig.UseCreditSeparator"/>.
		/// </summary>
		SeparateAfterFourDigits
	}
}