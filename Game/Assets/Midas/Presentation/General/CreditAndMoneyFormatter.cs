using System;
using System.Collections.Generic;
using System.Globalization;
using Midas.Core.Configuration;
using Midas.Core.General;

namespace Midas.Presentation.General
{
	public sealed class CreditAndMoneyFormatter
	{
		private const long DecimalNumber = 100;
		private const string FormatNoCommaNoPartial = "G";
		private const string FormatNoCommaWithPartial = "F2";
		private const string FormatWithCommaNoPartial = "N0";
		private const string FormatWithCommaWithPartial = "N2";

		private static readonly IReadOnlyDictionary<CurrencySymbolPosition, string> positionToFormatString =
			new Dictionary<CurrencySymbolPosition, string>
			{
				{ CurrencySymbolPosition.Leading, "{0}{1}" },
				{ CurrencySymbolPosition.LeadingWithSpace, "{0} {1}" },
				{ CurrencySymbolPosition.Trailing, "{1}{0}" },
				{ CurrencySymbolPosition.TrailingWithSpace, "{1} {0}" },
			};

		private readonly NumberFormatInfo numberFormatInfo = new NumberFormatInfo();
		private readonly CurrencyConfig currencyConfig;

		/// <summary>
		/// Gets the character to use for decimal separator. (10.1)
		/// </summary>
		public string DecimalSeparator => numberFormatInfo.NumberDecimalSeparator;

		/// <summary>
		/// Gets the character to use for separator. (1,000)
		/// </summary>
		public string DigitGroupSeparator => numberFormatInfo.NumberGroupSeparator;

		/// <summary>
		/// Gets the character to use for Currency Symbol ($1)
		/// </summary>
		public string CurrencySymbol => numberFormatInfo.CurrencySymbol;

		/// <summary>
		/// Gets the position of the currency symbol.
		/// </summary>
		public CurrencySymbolPosition SymbolPosition => (CurrencySymbolPosition)numberFormatInfo.CurrencyPositivePattern;

		/// <summary>
		/// Gets the character to use for Cent Symbol (10¢)
		/// </summary>
		public string CurrencyCentSymbol => currencyConfig.SubCurrencySymbol;

		/// <summary>
		/// Gets the position of the currency cent symbol.
		/// </summary>
		public CurrencySymbolPosition CentSymbolPosition => currencyConfig.SubSymbolPosition;

		/// <summary>
		/// Gets the flag indicating whether <see cref="DigitGroupSeparator" /> should
		/// be used for non monetary numbers, such as credits.
		/// </summary>
		/// <remarks>
		/// This flag is usually controlled by a config item in Foundation.
		/// </remarks>
		public bool UseCreditSeparator => currencyConfig.UseCreditSeparator;

		/// <summary>
		/// Initialize an instance of CreditAndMoneyFormatter.
		/// </summary>
		internal CreditAndMoneyFormatter(CurrencyConfig currencyConfig)
		{
			this.currencyConfig = currencyConfig;
			numberFormatInfo.NumberDecimalSeparator = numberFormatInfo.CurrencyDecimalSeparator = this.currencyConfig.DecimalSeparator;
			numberFormatInfo.NumberGroupSeparator = numberFormatInfo.CurrencyGroupSeparator = this.currencyConfig.DigitGroupSeparator;
			numberFormatInfo.CurrencySymbol = this.currencyConfig.CurrencySymbol;
			numberFormatInfo.CurrencyPositivePattern = (int)this.currencyConfig.SymbolPosition;
		}

		public string GetFormatted(MoneyAndCreditDisplayMode displayMode, Money money, CreditDisplaySeparatorMode separatorMode)
		{
			string result;
			switch (displayMode)
			{
				case MoneyAndCreditDisplayMode.MoneyBase:
				case MoneyAndCreditDisplayMode.MoneyWhole:
				case MoneyAndCreditDisplayMode.MoneyWholePlusBase:
					result = FormatForCurrency(money, displayMode);
					break;
				default:
					result = FormatForCredits(Credit.FromMoney(money), displayMode, separatorMode);
					break;
			}

			return result;
		}

		public string GetFormatted(MoneyAndCreditDisplayMode displayMode, Credit credits, CreditDisplaySeparatorMode separatorMode)
		{
			string result;
			switch (displayMode)
			{
				case MoneyAndCreditDisplayMode.MoneyBase:
				case MoneyAndCreditDisplayMode.MoneyWhole:
				case MoneyAndCreditDisplayMode.MoneyWholePlusBase:
					result = FormatForCurrency(Money.FromCredit(credits), displayMode);
					break;
				default:
					result = FormatForCredits(credits, displayMode, separatorMode);
					break;
			}

			return result;
		}

		/// <summary>
		/// Format to use for formatting to currency.
		/// </summary>
		/// <param name="money">Amount to display.</param>
		/// <param name="displayMode">How to format amount.</param>
		/// <returns>Currency representation of the amount.</returns>
		/// <devdoc>
		/// This method needs to be expanded to handle negative number formatting.
		/// </devdoc>
		private string FormatForCurrency(Money money, MoneyAndCreditDisplayMode displayMode)
		{
			long wholeNumberToDisplay;
			long decimalNumberToDisplay;

			checked
			{
				var numberToDisplay = money.AsMinorCurrency; //all in cent. Example: $3.45 = 345
				wholeNumberToDisplay = numberToDisplay / DecimalNumber; // Example: 3

				decimalNumberToDisplay = Math.Abs(numberToDisplay % DecimalNumber); //Example: 45
			}

			var formattedString = string.Empty;
			switch (displayMode)
			{
				case MoneyAndCreditDisplayMode.MoneyWhole:
					if (decimalNumberToDisplay == 0)
					{
						formattedString = wholeNumberToDisplay.ToString("C0", numberFormatInfo);
					}

					break;
				case MoneyAndCreditDisplayMode.MoneyBase:
					if (wholeNumberToDisplay == 0)
					{
						if (!string.IsNullOrEmpty(CurrencyCentSymbol))
						{
							var positionString = positionToFormatString[CentSymbolPosition];
							formattedString = string.Format(positionString, CurrencyCentSymbol, decimalNumberToDisplay);
						}
						else
						{
							// If there is no cents symbol the whole currency symbol must be used.
							var positionString = positionToFormatString[SymbolPosition];
							var innerString = $"0{DecimalSeparator}{decimalNumberToDisplay:D2}";
							formattedString = string.Format(positionString, CurrencySymbol, innerString);
						}

						if (money.IsNegative)
						{
							formattedString = $"({formattedString})";
						}
					}
					else if (decimalNumberToDisplay == 0)
					{
						formattedString = wholeNumberToDisplay.ToString("C0", numberFormatInfo);
					}

					break;
			}

			if (string.IsNullOrEmpty(formattedString))
			{
				checked
				{
					var temp = money.AsMinorCurrency / (decimal)DecimalNumber; //Example: §3.45=>3.45
					formattedString = temp.ToString("C2", numberFormatInfo);
				}
			}

			return formattedString;
		}

		/// <summary>
		/// Format the given amount as credits.
		/// </summary>
		/// <param name="credits">
		/// Amount to format.
		/// </param>
		/// <param name="displayMode">
		/// How to format amount.
		/// </param>
		/// <param name="separatorMode">
		/// How the digit group separator should be included.
		/// This parameter is optional.  If not specified, it is default to <see cref="CreditDisplaySeparatorMode.Auto" />.
		/// </param>
		/// <returns>
		/// Credit representation of the amount.
		/// </returns>
		private string FormatForCredits(Credit credits,
			MoneyAndCreditDisplayMode displayMode,
			CreditDisplaySeparatorMode separatorMode)
		{
			string creditNoPartialFormat = null;
			string creditWithPartialFormat = null;

			if (separatorMode == CreditDisplaySeparatorMode.Auto)
			{
				separatorMode = UseCreditSeparator
					? CreditDisplaySeparatorMode.SeparateAsCash
					: CreditDisplaySeparatorMode.NoSeparator;
			}

			switch (separatorMode)
			{
				case CreditDisplaySeparatorMode.NoSeparator:
					creditNoPartialFormat = FormatNoCommaNoPartial;
					creditWithPartialFormat = FormatNoCommaWithPartial;
					break;

				case CreditDisplaySeparatorMode.SeparateAsCash:
					creditNoPartialFormat = FormatWithCommaNoPartial;
					creditWithPartialFormat = FormatWithCommaWithPartial;
					break;

				case CreditDisplaySeparatorMode.SeparateAfterFourDigits:
					if (GetNumberOfDigits(credits.Credits) > 4)
					{
						creditNoPartialFormat = FormatWithCommaNoPartial;
						creditWithPartialFormat = FormatWithCommaWithPartial;
					}
					else
					{
						creditNoPartialFormat = FormatNoCommaNoPartial;
						creditWithPartialFormat = FormatNoCommaWithPartial;
					}

					break;
			}

			string result;
			if (displayMode == MoneyAndCreditDisplayMode.Credit && credits.HasSubCredits)
			{
				var creditValueWithPartial = credits.Value.Numerator / (double)credits.Value.Denominator;
				result = creditValueWithPartial.ToString(creditWithPartialFormat, numberFormatInfo);
			}
			else
			{
				result = credits.Credits.ToString(creditNoPartialFormat, numberFormatInfo);
			}

			return result;
		}

		/// <summary>
		/// Get the number of digits in the integer component of the given number.
		/// </summary>
		/// <param name="number">Number to get the integer digits for.</param>
		/// <returns>The number of digits in the integer component.</returns>
		private static uint GetNumberOfDigits(double number)
		{
			var absoluteValue = Math.Abs(number);
			var floor = Math.Floor(absoluteValue);
			return (uint)(absoluteValue < 1 ? 1 : Math.Log10(floor) + 1);
		}
	}
}