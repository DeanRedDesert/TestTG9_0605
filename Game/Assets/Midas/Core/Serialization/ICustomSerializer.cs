using System;
using System.IO;

namespace Midas.Core.Serialization
{
	/// <summary>
	/// Implement this if support for nvram serialization of complex types is required.
	/// </summary>
	public interface ICustomSerializer
	{
		/// <summary>
		/// Gets whether a type is supported by the serializer.
		/// </summary>
		/// <param name="t">The type to be serialized.</param>
		/// <returns>True if the custom serializer supports <paramref name="t"/>, otherwise false.</returns>
		bool SupportsType(Type t);

		/// <summary>
		/// Serialize an object.
		/// </summary>
		/// <param name="writer">The <see cref="BinaryWriter"/> to write the data into.</param>
		/// <param name="serializeComplex">A callback to further serialize complex types.</param>
		/// <param name="o">The object to serialize.</param>
		void Serialize(BinaryWriter writer, Action<BinaryWriter, object> serializeComplex, object o);

		/// <summary>
		/// Deserialize an object.
		/// </summary>
		/// <param name="t">The type that the object is.</param>
		/// <param name="reader">The <see cref="BinaryReader"/> to read the object data from.</param>
		/// <param name="deserializeComplex">A callback to further deserialize complex types.</param>
		/// <returns>The deserialized object.</returns>
		object Deserialize(Type t, BinaryReader reader, Func<BinaryReader, object> deserializeComplex);
	}
}