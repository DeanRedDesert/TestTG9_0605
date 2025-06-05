using System;
using System.IO;
using Midas.Core.General;
using Midas.Core.Serialization;

namespace Midas.Core
{
	public sealed class ProgressiveHit
	{
		public string LevelId { get; }
		public Money Amount { get; }
		public string Source { get; }
		public string SourceDetails { get; }

		public ProgressiveHit(string levelId, Money amount, string source, string sourceDetails)
		{
			LevelId = levelId;
			Amount = amount;
			Source = source;
			SourceDetails = sourceDetails;
		}

		#region Serialization

		private sealed class CustomSerializer : ICustomSerializer
		{
			public bool SupportsType(Type t) => t == typeof(ProgressiveHit);

			public void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o)
			{
				var progressiveHit = (ProgressiveHit)o;

				writer.Write(progressiveHit.LevelId);
				serializeComplex(writer, progressiveHit.Amount);

				// These values can be null so let the complex serializer take care of it

				serializeComplex(writer, progressiveHit.Source);
				serializeComplex(writer, progressiveHit.SourceDetails);
			}

			public object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex)
			{
				var triggerId = reader.ReadString();
				var amount = (Money)deserializeComplex(reader);

				// These values can be null so let the complex serializer take care of it

				var source = (string)deserializeComplex(reader);
				var sourceDetails = (string)deserializeComplex(reader);

				return new ProgressiveHit(triggerId, amount, source, sourceDetails);
			}
		}

		static ProgressiveHit()
		{
			NvramSerializer.RegisterCustomSerializer(new CustomSerializer());
		}

		#endregion
	}
}