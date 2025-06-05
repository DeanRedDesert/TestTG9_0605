using System;
using System.IO;

namespace Midas.Core.Configuration
{
	public sealed class CurrencyConfig
	{
		public string CurrencySymbol { get; }
		public CurrencySymbolPosition SymbolPosition { get; }
		public string SubCurrencySymbol { get; }
		public CurrencySymbolPosition SubSymbolPosition { get; }
		public string DecimalSeparator { get; }
		public string DigitGroupSeparator { get; }
		public bool UseCreditSeparator { get; }

		public CurrencyConfig(string currencySymbol, CurrencySymbolPosition symbolPosition,
			string subCurrencySymbol, CurrencySymbolPosition subSymbolPosition,
			string decimalSeparator, string digitGroupSeparator,
			bool useCreditSeparator)
		{
			CurrencySymbol = currencySymbol;
			SymbolPosition = symbolPosition;
			SubCurrencySymbol = subCurrencySymbol ?? string.Empty;
			SubSymbolPosition = subSymbolPosition;
			DecimalSeparator = decimalSeparator;
			DigitGroupSeparator = digitGroupSeparator;
			UseCreditSeparator = useCreditSeparator;
		}

		public override string ToString()
		{
			return $"'{CurrencySymbol}', {SymbolPosition}, '{SubCurrencySymbol}', " +
				$"{SubSymbolPosition}, '{DecimalSeparator}', '{DigitGroupSeparator}', " +
				$"{UseCreditSeparator}";
		}

		public void Serialize(BinaryWriter writer)
		{
			writer.Write(CurrencySymbol);
			writer.Write((int)SymbolPosition);
			writer.Write(SubCurrencySymbol);
			writer.Write((int)SubSymbolPosition);
			writer.Write(DecimalSeparator);
			writer.Write(DigitGroupSeparator);
			writer.Write(UseCreditSeparator);
		}

		public static CurrencyConfig Deserialize(BinaryReader reader)
		{
			var currencySymbol = reader.ReadString();
			var symbolPosition = (CurrencySymbolPosition)reader.ReadInt32();
			var subCurrencySymbol = reader.ReadString();
			var subSymbolPosition = (CurrencySymbolPosition)reader.ReadInt32();
			var decimalSeparator = reader.ReadString();
			var digitGroupSeparator = reader.ReadString();
			var useCreditSeparator = reader.ReadBoolean();

			return new CurrencyConfig(currencySymbol, symbolPosition, subCurrencySymbol, subSymbolPosition, decimalSeparator, digitGroupSeparator, useCreditSeparator);
		}
	}
}