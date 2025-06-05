namespace Midas.Presentation.General
{
	public enum MoneyAndCreditDisplayMode
	{
		/// <summary>
		/// Displays credits with partial values.
		/// Example: 1.3 credits is shown as "1.3"
		/// </summary>
		Credit,

		/// <summary>
		/// Truncates partial credits.
		/// Example: 1.3 credits is shown as "1"
		/// </summary>
		CreditNoPartial,

		/// <summary>
		/// Base value is not shown if it is zero.
		/// Examples: $1, $1.23, $0.23
		/// </summary>
		MoneyWhole,

		/// <summary>
		/// Whole and base values are always shown even when either is zero.
		/// Examples: $1.00, $1.23, $0.23
		/// </summary>
		MoneyWholePlusBase,

		/// <summary>
		/// Base value is not shown if it is zero. If the whole value is zero,
		/// it is not shown and the cent symbol is used.
		/// Examples: $1, $1.23, 23¢
		/// </summary>
		MoneyBase
	}
}