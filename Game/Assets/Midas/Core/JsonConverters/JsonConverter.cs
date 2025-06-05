using System;
using Newtonsoft.Json;

namespace Midas.Core.JsonConverters
{
	/// <summary>
	/// Converts an object to and from JSON.
	/// This is a copy of the original NewtonSoft Json to avoid 'Failed to extract warning'
	/// from Unity which cannot extract Generic classes of specific types
	/// </summary>
	/// <typeparam name="T">The object type to convert.</typeparam>
	public abstract class JsonConverter<T> : JsonConverter
	{
		public static Type ConverterType => typeof(T);

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (!(value != null ? value is T : ReflectionUtil.CanBeNull(typeof(T))))
				throw new JsonSerializationException($"Converter cannot write specified value to JSON. {typeof(T)} is required.");

			WriteJson(writer, (T)value, serializer);
		}

		/// <summary>
		/// Writes the JSON representation of the object.
		/// </summary>
		/// <param name="writer">The <see cref="JsonWriter" /> to write to.</param>
		/// <param name="value">The value.</param>
		/// <param name="serializer">The calling serializer.</param>
		protected abstract void WriteJson(JsonWriter writer, T value, JsonSerializer serializer);

		/// <summary>
		/// Reads the JSON representation of the object.
		/// </summary>
		/// <param name="reader">The <see cref="JsonReader" /> to read from.</param>
		/// <param name="objectType">Type of the object.</param>
		/// <param name="existingValue">The existing value of object being read.</param>
		/// <param name="serializer">The calling serializer.</param>
		/// <returns>The object value.</returns>
		public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new NotSupportedException();

		/// <summary>
		/// Determines whether this instance can convert the specified object type.
		/// </summary>
		/// <param name="objectType">Type of the object.</param>
		/// <returns>
		/// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
		/// </returns>
		public sealed override bool CanConvert(Type objectType) => typeof(T).IsAssignableFrom(objectType);
	}
}