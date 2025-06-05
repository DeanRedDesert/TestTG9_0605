using System;
using Midas.Presentation.Data;
using Newtonsoft.Json;

namespace Midas.Gaff.Utp
{
	public sealed partial class StatusDatabaseModule
	{
		public abstract class JsonConverter<T> : JsonConverter
		{
			#region Public

			public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				if (!(value != null ? value is T : IsNullable(typeof(T))))
					throw new JsonSerializationException($"Converter cannot write specified value to JSON. {typeof(T)} is required.");

				WriteJson(writer, (T)value, serializer);
			}

			protected abstract void WriteJson(JsonWriter writer, T value, JsonSerializer serializer);

			public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				var existingIsNull = existingValue == null;
				if (!(existingIsNull || existingValue is T))
					throw new JsonSerializationException($"Converter cannot read JSON with the specified existing value. {typeof(T)} is required.");

				return ReadJson(reader, objectType, existingIsNull ? default : (T)existingValue, !existingIsNull, serializer);
			}

			protected abstract T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer);

			public sealed override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);

			private static bool IsNullable(Type t) => !t.IsValueType || IsNullableType(t);

			private static bool IsNullableType(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>);

			#endregion
		}

		public sealed class StatusBlockConverter : JsonConverter<StatusBlock>
		{
			#region Public

			protected override void WriteJson(JsonWriter writer, StatusBlock value, JsonSerializer serializer)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("Name");
				writer.WriteValue(value.Name);

				if (value.Properties.Count != 0)
				{
					writer.WritePropertyName("Properties");
					serializer.Serialize(writer, value.Properties);
				}

				if (value is StatusBlockCompound compound && compound.StatusBlocks.Count != 0)
				{
					writer.WritePropertyName("StatusBlocks");
					serializer.Serialize(writer, compound.StatusBlocks);
				}

				writer.WriteEndObject();
			}

			protected override StatusBlock ReadJson(JsonReader reader, Type objectType, StatusBlock existingValue, bool hasExistingValue, JsonSerializer serializer) => throw new NotSupportedException();

			#endregion
		}

		public sealed class StatusItemValueConverter : JsonConverter<IStatusProperty>
		{
			#region Public

			protected override void WriteJson(JsonWriter writer, IStatusProperty value, JsonSerializer serializer)
			{
				writer.WriteStartObject();
				writer.WritePropertyName(value.Name);
				serializer.Serialize(writer, value.Value);
				writer.WriteEndObject();
			}

			protected override IStatusProperty ReadJson(JsonReader reader, Type objectType, IStatusProperty existingValue, bool hasExistingValue, JsonSerializer serializer) => throw new NotSupportedException();

			#endregion
		}
	}
}