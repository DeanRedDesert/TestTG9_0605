//-----------------------------------------------------------------------
// <copyright file = "CreditFormatter.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// TODO: http://cspjira:8080/jira/browse/AS-8697
// ReSharper disable NonReadonlyMemberInGetHashCode
namespace IGT.Game.Core.Money
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using CompactSerialization;
    using Cloneable;

    /// <summary>
    /// Class that provides formatting for credits.
    /// </summary>
    [Serializable]
    public class CreditFormatter : ICompactSerializable, IDeepCloneable, IEquatable<CreditFormatter>
    {
        #region Static Fields

        // ReSharper disable InconsistentNaming
        /// <summary>
        /// Default US credit formatter. Can be used in the editor or other cases where a default is needed.
        /// </summary>
        private static CreditFormatter defaultUS;

        /// <summary>
        /// Default US credit formatter. Can be used in the editor or other cases where a default is needed.
        /// </summary>
        public static CreditFormatter DefaultUS => defaultUS ?? (defaultUS = new CreditFormatter(".", ",", "$", "¢"));

        // ReSharper restore InconsistentNaming

        #endregion

        #region Private Fields

        /// <summary>
        /// The Decimal Number used to generate the number information to display.
        /// </summary>
        private const long DecimalNumber = 100;

        /// <summary>
        /// The number formatter information game object.
        /// </summary>
        private NumberFormatInfo numberFormatInfo;

        /// <summary>
        /// The character for the decimal separator.
        /// </summary>
        private string decimalSeparator;

        /// <summary>
        /// The character for the digit group separator.
        /// </summary>
        private string digitGroupSeparator;

        /// <summary>
        /// The character for the currency symbol.
        /// </summary>
        private string currencySymbol;

        /// <summary>
        /// The position for the currency symbol.
        /// </summary>
        private CurrencySymbolPosition symbolPosition;

        /// <summary>
        /// Dictionary that maps <see cref="CurrencySymbolPosition"/>s to format strings.
        /// Argument 0 is currency symbol; Argument 1 is currency value.
        /// </summary>
        private readonly Dictionary<CurrencySymbolPosition, string> positionToFormatString =
            new Dictionary<CurrencySymbolPosition, string>
            {
                { CurrencySymbolPosition.Left, "{0}{1}" },
                { CurrencySymbolPosition.LeftWithSpace, "{0} {1}" },
                { CurrencySymbolPosition.Right, "{1}{0}" },
                { CurrencySymbolPosition.RightWithSpace, "{1} {0}" },
            };

        /// <summary>
        /// Format string for formatting credits without partial credits and no comma.
        /// </summary>
        private const string FormatNoCommaNoPartial = "G";

        /// <summary>
        /// Format string for formatting credits with partial credits and no comma.
        /// </summary>
        private const string FormatNoCommaWithPartial = "F2";

        /// <summary>
        /// Format string for formatting credits without partial credits and with a comma.
        /// </summary>
        private const string FormatWithCommaNoPartial = "N0";

        /// <summary>
        /// Format string for formatting credits with partial credits and with a comma.
        /// </summary>
        private const string FormatWithCommaWithPartial = "N2";

        #endregion

        #region Properties

        /// <summary>
        /// Gets the character to use for decimal separator. (10.1)
        /// </summary>
        public string DecimalSeparator
        {
            get => decimalSeparator;
            private set
            {
                decimalSeparator = value;
                numberFormatInfo.NumberDecimalSeparator = numberFormatInfo.CurrencyDecimalSeparator = value;
            }
        }

        /// <summary>
        /// Gets the character to use for separator. (1,000)
        /// </summary>
        public string DigitGroupSeparator
        {
            get => digitGroupSeparator;
            private set
            {
                digitGroupSeparator = value;
                numberFormatInfo.NumberGroupSeparator = numberFormatInfo.CurrencyGroupSeparator = value;
            }
        }

        /// <summary>
        /// Gets the character to use for Currency Symbol ($1)
        /// </summary>
        public string CurrencySymbol
        {
            get => currencySymbol;
            private set
            {
                currencySymbol = value;
                numberFormatInfo.CurrencySymbol = value;
            }
        }

        /// <summary>
        /// Gets the position of the currency symbol.
        /// </summary>
        public CurrencySymbolPosition SymbolPosition
        {
            get => symbolPosition;
            private set
            {
                symbolPosition = value;
                numberFormatInfo.CurrencyPositivePattern = (int)value;
            }
        }

        /// <summary>
        /// Gets the character to use for Cent Symbol (10¢)
        /// </summary>
        public string CurrencyCentSymbol { get; private set; }

        /// <summary>
        /// Gets the position of the currency cent symbol.
        /// </summary>
        public CurrencySymbolPosition CentSymbolPosition { get; private set; }

        /// <summary>
        /// Gets how to format negative numbers.
        /// </summary>
        public NegativeNumberFormat NegativeNumberFormat { get; private set; }

        /// <summary>
        /// Gets the flag indicating whether <see cref="DigitGroupSeparator"/> should
        /// be used for non monetary numbers, such as credits.
        /// </summary>
        /// <remarks>
        /// This flag is usually controlled by a config item in Foundation.
        /// </remarks>
        public bool UseCreditSeparator { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize an instance of credit formatter.
        /// </summary>
        public CreditFormatter()
            : this("?", "?", "?", "?")
        {
        }

        /// <summary>
        /// Initialize an instance of credit formatter.
        /// </summary>
        /// <param name="decimalPoint">String to use for the decimal point. (10.1)</param>
        /// <param name="separator">String to use for the separator. (1,000)</param>
        /// <param name="currencySymbol">String to use for the Currency Symbol. ($1)</param>
        /// <param name="currencyCentSymbol">String to use for the Cent Symbol. (10¢)</param>
        /// <param name="useCreditSeparator">
        /// The flag indicating whether <paramref name="separator"/> should
        /// be used for non monetary numbers, such as credits.
        /// </param>
        public CreditFormatter(string decimalPoint, string separator, string currencySymbol, string currencyCentSymbol,
                               bool useCreditSeparator)
            : this(decimalPoint, separator, currencySymbol, currencyCentSymbol,
                   CurrencySymbolPosition.Left, useCreditSeparator)
        {
        }

        /// <summary>
        /// Initialize an instance of credit formatter.
        /// </summary>
        /// <param name="decimalPoint">String to use for the decimal point. (10.1)</param>
        /// <param name="separator">String to use for the separator. (1,000)</param>
        /// <param name="currencySymbol">String to use for the Currency Symbol. ($1)</param>
        /// <param name="currencyCentSymbol">String to use for the Cent Symbol. (10¢)</param>
        /// <param name="symbolPosition">The position for the currency symbol.</param>
        public CreditFormatter(string decimalPoint, string separator, string currencySymbol, string currencyCentSymbol,
                               CurrencySymbolPosition symbolPosition)
            : this(decimalPoint, separator, currencySymbol, currencyCentSymbol,
                   symbolPosition, false)
        {
        }

        /// <summary>
        /// Initialize an instance of credit formatter.
        /// </summary>
        /// <param name="decimalPoint">String to use for the decimal point. (10.1)</param>
        /// <param name="separator">String to use for the separator. (1,000)</param>
        /// <param name="currencySymbol">String to use for the Currency Symbol. ($1)</param>
        /// <param name="currencyCentSymbol">String to use for the Cent Symbol. (10¢)</param>
        /// <param name="symbolPosition">The position for the currency symbol.</param>
        /// <param name="useCreditSeparator">
        /// The flag indicating whether <paramref name="separator"/> should
        /// be used for non monetary numbers, such as credits.
        /// </param>
        public CreditFormatter(string decimalPoint, string separator, string currencySymbol, string currencyCentSymbol,
                               CurrencySymbolPosition symbolPosition = CurrencySymbolPosition.Left,
                               bool useCreditSeparator = false)
            : this(decimalPoint, separator, currencySymbol, currencyCentSymbol,
                   symbolPosition, CurrencySymbolPosition.Right,
                   NegativeNumberFormat.IsFirstSymbol, useCreditSeparator)
        {
        }

        /// <summary>
        /// Initialize an instance of credit formatter.
        /// </summary>
        /// <param name="decimalPoint">String to use for the decimal point. (10.1)</param>
        /// <param name="separator">String to use for the separator. (1,000)</param>
        /// <param name="currencySymbol">String to use for the Currency Symbol. ($1)</param>
        /// <param name="currencyCentSymbol">String to use for the Cent Symbol. (10¢)</param>
        /// <param name="symbolPosition">The position for the currency symbol.</param>
        /// <param name="centSymbolPosition">The position for the currency cent symbol.</param>
        /// <param name="negativeNumberFormat">The position for the negative sign.</param>
        /// <param name="useCreditSeparator">
        /// The flag indicating whether <paramref name="separator"/> should
        /// be used for non monetary numbers, such as credits.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="decimalPoint"/>, <paramref name="separator"/>, or 
        /// <paramref name="currencySymbol"/> is null.
        /// </exception>
        public CreditFormatter(string decimalPoint, string separator, string currencySymbol, string currencyCentSymbol,
                               CurrencySymbolPosition symbolPosition, CurrencySymbolPosition centSymbolPosition,
                               NegativeNumberFormat negativeNumberFormat, bool useCreditSeparator)
        {
            numberFormatInfo = new NumberFormatInfo();
            DecimalSeparator = decimalPoint ?? throw new ArgumentNullException(nameof(decimalPoint));
            DigitGroupSeparator = separator ?? throw new ArgumentNullException(nameof(separator));
            CurrencySymbol = currencySymbol ?? throw new ArgumentNullException(nameof(currencySymbol));
            CurrencyCentSymbol = currencyCentSymbol;
            SymbolPosition = symbolPosition;
            CentSymbolPosition = centSymbolPosition;
            NegativeNumberFormat = negativeNumberFormat;
            UseCreditSeparator = useCreditSeparator;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Format to use for formatting to currency.
        /// </summary>
        /// <param name="amount">Amount to display.</param>
        /// <param name="displayMode">How to format amount.</param>
        /// <returns>Currency representation of the amount.</returns>
        /// <devdoc>
        /// This method needs to be expanded to handle negative number formatting.
        /// </devdoc>
        public string FormatForCurrency(Amount amount, CashDisplayMode displayMode)
        {
            long wholeNumberToDisplay;
            long decimalNumberToDisplay;

            checked
            {
                var numberToDisplay = amount.CurrencyValue;
                wholeNumberToDisplay = numberToDisplay / DecimalNumber;

                // TODO: Needs to add handling of negative numbers.
                decimalNumberToDisplay = Math.Abs(numberToDisplay % DecimalNumber);
            }

            var formattedString = string.Empty;
            switch(displayMode)
            {
                case CashDisplayMode.Whole:
                    if(decimalNumberToDisplay == 0)
                    {
                        formattedString = wholeNumberToDisplay.ToString("C0", numberFormatInfo);
                    }
                    break;
                case CashDisplayMode.Base:
                    if(wholeNumberToDisplay == 0)
                    {
                        if(!string.IsNullOrEmpty(CurrencyCentSymbol))
                        {
                            var positionString = positionToFormatString[CentSymbolPosition];
                            formattedString = string.Format(positionString, CurrencyCentSymbol, decimalNumberToDisplay);
                        }
                        else
                        {
                            // If there is no cents symbol the whole currency symbol must be used.
                            var positionString = positionToFormatString[SymbolPosition];
                            var innerString = $"0{DecimalSeparator}{decimalNumberToDisplay:D2}";
                            formattedString = string.Format(positionString, currencySymbol, innerString);
                        }
                    }
                    else if(decimalNumberToDisplay == 0)
                    {
                        formattedString = wholeNumberToDisplay.ToString("C0", numberFormatInfo);
                    }

                    break;
            }

            if(string.IsNullOrEmpty(formattedString))
            {
                checked
                {
                    var temp = amount.CurrencyValue / (decimal)DecimalNumber;
                    formattedString = temp.ToString("C2", numberFormatInfo);
                }
            }

            return formattedString;
        }

        /// <summary>
        /// Format the given amount as credits.
        /// </summary>
        /// <param name="amount">
        /// Amount to format.
        /// </param>
        /// <param name="displayMode">
        /// How to format amount.
        /// </param>
        /// <param name="separatorMode">
        /// How the digit group separator should be included.
        /// This parameter is optional.  If not specified, it is default to <see cref="CreditSeparatorMode.Auto"/>.
        /// </param>
        /// <returns>
        /// Credit representation of the amount.
        /// </returns>
        public string FormatForCredits(Amount amount,
                                       CreditDisplayMode displayMode,
                                       CreditSeparatorMode separatorMode = CreditSeparatorMode.Auto)
        {
            string creditNoPartialFormat = null;
            string creditWithPartialFormat = null;

            if(separatorMode == CreditSeparatorMode.Auto)
            {
                separatorMode = UseCreditSeparator
                                    ? CreditSeparatorMode.SeparateAsCash
                                    : CreditSeparatorMode.NoSeparator;
            }

            switch(separatorMode)
            {
                case CreditSeparatorMode.NoSeparator:
                    creditNoPartialFormat = FormatNoCommaNoPartial;
                    creditWithPartialFormat = FormatNoCommaWithPartial;
                    break;

                case CreditSeparatorMode.SeparateAsCash:
                    creditNoPartialFormat = FormatWithCommaNoPartial;
                    creditWithPartialFormat = FormatWithCommaWithPartial;
                    break;

                case CreditSeparatorMode.SeparateAfterFourDigits:
                    if(GetNumberOfDigits(amount.GameCreditValue) > 4)
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

            var stringBuilder =
                new StringBuilder(amount.GameCreditValue.ToString(creditNoPartialFormat, numberFormatInfo));

            switch(displayMode)
            {
                // If the mode is to display the credit value with partials
                case CreditDisplayMode.Credit:
                {
                    if(amount.BaseValue % amount.GameDenom != 0)
                    {
                        // TODO: Needs to add handling of negative numbers.
                        var creditValueWithPartial = Math.Abs((double)amount.BaseValue / amount.GameDenom);
                        stringBuilder.Remove(0, stringBuilder.Length);
                        stringBuilder.Append(creditValueWithPartial.ToString(creditWithPartialFormat, numberFormatInfo));
                    }
                    break;
                }
            }

            return stringBuilder.ToString();
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritDoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, DecimalSeparator);
            CompactSerializer.Write(stream, DigitGroupSeparator);
            CompactSerializer.Write(stream, CurrencySymbol);
            CompactSerializer.Write(stream, CurrencyCentSymbol);
            CompactSerializer.Write(stream, SymbolPosition);
            CompactSerializer.Write(stream, CentSymbolPosition);
            CompactSerializer.Write(stream, NegativeNumberFormat);
            CompactSerializer.Write(stream, UseCreditSeparator);
        }

        /// <inheritDoc/>
        public void Deserialize(Stream stream)
        {
            numberFormatInfo = new NumberFormatInfo();
            DecimalSeparator = CompactSerializer.ReadString(stream);
            DigitGroupSeparator = CompactSerializer.ReadString(stream);
            CurrencySymbol = CompactSerializer.ReadString(stream);
            CurrencyCentSymbol = CompactSerializer.ReadString(stream);
            SymbolPosition = CompactSerializer.ReadEnum<CurrencySymbolPosition>(stream);
            CentSymbolPosition = CompactSerializer.ReadEnum<CurrencySymbolPosition>(stream);
            NegativeNumberFormat = CompactSerializer.ReadEnum<NegativeNumberFormat>(stream);
            UseCreditSeparator = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region IDeepClonable Members

        /// <inheritdoc/>
        public object DeepClone()
        {
            var copy = new CreditFormatter
                       {
                           DecimalSeparator = DecimalSeparator,
                           DigitGroupSeparator = DigitGroupSeparator,
                           CurrencySymbol = CurrencySymbol,
                           CurrencyCentSymbol = CurrencyCentSymbol,
                           SymbolPosition = SymbolPosition,
                           CentSymbolPosition = CentSymbolPosition,
                           NegativeNumberFormat = NegativeNumberFormat,
                           UseCreditSeparator = UseCreditSeparator
                       };
            return copy;
        }

        #endregion

        #region IEquatable<CreditFormatter> Members

        /// <inheritdoc />
        public bool Equals(CreditFormatter other)
        {
            if(other == null)
            {
                return false;
            }

            return DecimalSeparator == other.DecimalSeparator
                   && DigitGroupSeparator == other.DigitGroupSeparator
                   && CurrencySymbol == other.CurrencySymbol
                   && CurrencyCentSymbol == other.CurrencyCentSymbol
                   && SymbolPosition == other.SymbolPosition
                   && CentSymbolPosition == other.CentSymbolPosition
                   && NegativeNumberFormat == other.NegativeNumberFormat
                   && UseCreditSeparator == other.UseCreditSeparator;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as CreditFormatter;
            if(other != null)
            {
                result = Equals(other);
            }

            return result;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;

            hash = DecimalSeparator != null ? hash * 37 + DecimalSeparator.GetHashCode() : hash;
            hash = DigitGroupSeparator != null ? hash * 37 + DigitGroupSeparator.GetHashCode() : hash;
            hash = CurrencySymbol != null ? hash * 37 + CurrencySymbol.GetHashCode() : hash;
            hash = CurrencyCentSymbol != null ? hash * 37 + CurrencyCentSymbol.GetHashCode() : hash;
            hash = hash * 37 + SymbolPosition.GetHashCode();
            hash = hash * 37 + CentSymbolPosition.GetHashCode();
            hash = hash * 37 + NegativeNumberFormat.GetHashCode();
            hash = hash * 37 + UseCreditSeparator.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(CreditFormatter left, CreditFormatter right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if((object)left == null || (object)right == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal. False otherwise.</returns>
        public static bool operator !=(CreditFormatter left, CreditFormatter right)
        {
            return !(left == right);
        }

        #endregion

        #region Private Methods

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

        #endregion
    }
}