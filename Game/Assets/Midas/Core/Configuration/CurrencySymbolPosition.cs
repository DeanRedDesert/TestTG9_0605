namespace Midas.Core.Configuration
{
	public enum CurrencySymbolPosition
	{
		Leading = 0, // Enum values are important, as they are being used by NumberFormatInfo.CurrencyPositivePattern
		Trailing = 1,
		LeadingWithSpace = 2,
		TrailingWithSpace = 3
	}
}