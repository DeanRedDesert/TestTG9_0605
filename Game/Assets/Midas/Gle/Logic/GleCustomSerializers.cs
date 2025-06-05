using System;
using System.IO;
using Logic.Core.Types;
using Logic.Core.Utility;
using Midas.Core.Serialization;

namespace Midas.Gle.Logic
{
	public sealed class GleCustomSerializers : ICustomSerializer
	{
		public bool SupportsType(Type t) => t == typeof(Credits) || t == typeof(Money) || t == typeof(ReadOnlyMask) || t == typeof(Cell);

		public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
		{
			switch (o)
			{
				case Credits credit:
					writer.Write(credit.ToUInt64());
					break;
				case Money money:
					writer.Write(money.ToCents());
					break;
				case ReadOnlyMask mask:
					writer.Write(mask.ToStringOrThrow("SL"));
					break;
				case Cell cell:
					writer.Write(cell.Column);
					writer.Write(cell.Row);
					break;
			}
		}

		public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
		{
			if (t == typeof(Money))
				return new Money(reader.ReadUInt64());

			if (t == typeof(Credits))
				return new Credits(reader.ReadUInt64());

			if (t == typeof(ReadOnlyMask))
				return ReadOnlyMask.CreateFromBitString(reader.ReadString());

			if (t == typeof(Cell))
				return new Cell(reader.ReadInt32(), reader.ReadInt32());

			throw new ArgumentException($"Unhandled type {t}");
		}
	}
}