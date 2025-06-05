using System;
using System.Collections.Generic;
using System.IO;
using Midas.Core.General;
using Midas.Core.Serialization;
using Midas.Gamble.LogicToPresentation;

namespace Midas.Gamble.Logic
{
	public partial class Trumps
	{
		private sealed class TrumpsState
		{
			public List<TrumpsSuit> History;
			public List<TrumpsCycleData> CycleData;

			#region Serialization

			private sealed class CustomSerializer : ICustomSerializer
			{
				public bool SupportsType(Type t) => t == typeof(TrumpsState) || t == typeof(TrumpsCycleData);

				public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
				{
					switch (o)
					{
						case TrumpsState trumpsState:
							serializeComplex(writer, trumpsState.History);
							serializeComplex(writer, trumpsState.CycleData);
							break;
						case TrumpsCycleData cycleData:
							writer.Write((byte)cycleData.Selection);
							writer.Write((byte)cycleData.Suit);
							writer.Write((byte)cycleData.Result);
							serializeComplex(writer, cycleData.WinAmount);
							writer.Write((byte)cycleData.GambleCompleteReason);
							break;
					}
				}

				public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
				{
					if (t == typeof(TrumpsState))
					{
						var result = new TrumpsState();
						result.History = (List<TrumpsSuit>)deserializeComplex(reader);
						result.CycleData = (List<TrumpsCycleData>)deserializeComplex(reader);
						return result;
					}

					if (t == typeof(TrumpsCycleData))
					{
						var selection = (TrumpsSelection)reader.ReadByte();
						var suit = (TrumpsSuit)reader.ReadByte();
						var result = (TrumpsResult)reader.ReadByte();
						var winAmount = (Money)deserializeComplex(reader);
						var reason = (GambleCompleteReason)reader.ReadByte();
						return new TrumpsCycleData(selection, suit, result, winAmount, reason);
					}

					throw new InvalidOperationException($"Unable to deserialize an object of type {t}");
				}
			}

			static TrumpsState()
			{
				NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
			}

			#endregion
		}
	}
}