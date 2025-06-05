using System;
using System.IO;
using Midas.Core.Serialization;

namespace Midas.Core.General
{
	internal sealed class GeneralCustomSerializer : ICustomSerializer
	{
		public bool SupportsType(Type t)
		{
			return t == typeof(Credit) || t == typeof(Money) || t == typeof(RationalNumber);
		}

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
		{
			switch (o)
			{
				case Money money:
					SerializeRationalNumber(writer, money.Value);
					break;

				case Credit credit:
					SerializeRationalNumber(writer, credit.Value);
					break;

				case RationalNumber number:
					SerializeRationalNumber(writer, number);
					break;
			}
		}

		public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			var number = DeserializeRationalNumber(reader);

			if (t == typeof(Money))
			{
				return number == default ? default : Money.FromRationalNumber(number);
			}

			if (t == typeof(Credit))
			{
				return number == default ? default : Credit.FromRationalNumber(number.Numerator, number.Denominator);
			}

			return number;
		}

		private static void SerializeRationalNumber(BinaryWriter writer, RationalNumber number)
		{
			if (number == default)
				writer.Write(false);
			else
			{
				writer.Write(true);
				writer.Write(number.Numerator);
				writer.Write(number.Denominator);
			}
		}

		private static RationalNumber DeserializeRationalNumber(BinaryReader reader)
		{
			return reader.ReadBoolean() ? new RationalNumber(reader.ReadInt64(), reader.ReadInt64()) : default;
		}
	}
}