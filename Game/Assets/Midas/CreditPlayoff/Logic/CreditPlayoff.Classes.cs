using System;
using System.IO;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.CreditPlayoff.LogicToPresentation;

namespace Midas.CreditPlayoff.Logic
{
	public sealed partial class CreditPlayoff
	{
		private sealed class CreditPlayoffData
		{
			public CreditPlayoffState State;
			public Money Cash;
			public Money Bet;
			public int Number;

			#region Serialization

			private sealed class CustomSerializer : ICustomSerializer
			{
				public bool SupportsType(Type t) => t == typeof(CreditPlayoffData);

				public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
				{
					switch (o)
					{
						case CreditPlayoffData cps:
							serializeComplex(writer, cps.State);
							serializeComplex(writer, cps.Cash);
							serializeComplex(writer, cps.Bet);
							writer.Write(cps.Number);
							break;

						default:
							throw new InvalidOperationException($"Unable to serialize object of type {o.GetType()}");
					}
				}

				public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
				{
					if (t == typeof(CreditPlayoffData))
					{
						var cps = new CreditPlayoffData();
						cps.State = (CreditPlayoffState)deserializeComplex(reader);
						cps.Cash = (Money)deserializeComplex(reader);
						cps.Bet = (Money)deserializeComplex(reader);
						cps.Number = reader.ReadInt32();
						return cps;
					}

					throw new InvalidOperationException($"Unable to deserialize object of type {t}");
				}
			}

			static CreditPlayoffData()
			{
				NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
			}

			#endregion
		}
	}
}