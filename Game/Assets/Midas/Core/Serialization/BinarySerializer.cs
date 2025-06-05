using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Midas.Core.Serialization
{
	public sealed class BinarySerializer
	{
		#region Nested Types

		private enum TypeId
		{
			Null,
			Bool,
			Byte,
			Char,
			Int16,
			Int32,
			Int64,
			UInt16,
			UInt32,
			UInt64,
			Single,
			Double,
			String,
			TimeSpan,
			DateTime,
			Tuple,
			Array,
			List,
			Dictionary,
			Enum,
			Custom,
			Auto
		}

		#endregion

		#region Fields

		private readonly List<Type> typeMap;
		private readonly IReadOnlyList<ICustomSerializer> customSerializers;

		#endregion

		public IReadOnlyList<Type> TypeMap { get { return typeMap; } }

		public BinarySerializer(IReadOnlyList<ICustomSerializer> customSerializers, IReadOnlyList<Type> typeMap)
		{
			this.customSerializers = customSerializers;
			this.typeMap = typeMap.ToList();
		}

		/// <summary>
		/// Serialize an object using a BinaryWriter.
		/// </summary>
		/// <param name="writer">The writer to serialize with.</param>
		/// <param name="o">The object to serialize.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Serialize(BinaryWriter writer, object o)
		{
			// Handle null first.

			if (o == null)
			{
				writer.Write((byte)TypeId.Null);
				return;
			}

			// Then check for a custom serializer.

			if (TrySerializeCustom(writer, o))
				return;

			// Is it an enum?

			var t = o.GetType();
			if (t.IsEnum)
			{
				writer.Write((byte)TypeId.Enum);
				writer.Write(GetTypeIndex(t));

				// Change the enum to its underlying type and allow standard serialization to write it out.

				o = Convert.ChangeType(o, t.GetEnumUnderlyingType());
			}

			// Finally handle simple types.

			switch (o)
			{
				case bool value:
					writer.Write((byte)TypeId.Bool);
					writer.Write(value);
					break;

				case byte value:
					writer.Write((byte)TypeId.Byte);
					writer.Write(value);
					break;

				case char value:
					writer.Write((byte)TypeId.Char);
					writer.Write(value);
					break;

				case short value:
					writer.Write((byte)TypeId.Int16);
					writer.Write(value);
					break;

				case int value:
					writer.Write((byte)TypeId.Int32);
					writer.Write(value);
					break;

				case long value:
					writer.Write((byte)TypeId.Int64);
					writer.Write(value);
					break;

				case ushort value:
					writer.Write((byte)TypeId.UInt16);
					writer.Write(value);
					break;

				case uint value:
					writer.Write((byte)TypeId.UInt32);
					writer.Write(value);
					break;

				case ulong value:
					writer.Write((byte)TypeId.UInt64);
					writer.Write(value);
					break;

				case float value:
					writer.Write((byte)TypeId.Single);
					writer.Write(value);
					break;

				case double value:
					writer.Write((byte)TypeId.Double);
					writer.Write(value);
					break;

				case string value:
					writer.Write((byte)TypeId.String);
					writer.Write(value);
					break;

				case TimeSpan value:
					writer.Write((byte)TypeId.TimeSpan);
					writer.Write(value.Ticks);
					break;

				case DateTime value:
					writer.Write((byte)TypeId.DateTime);
					writer.Write(value.Ticks);
					break;

				default:
					if (TrySerializeCollection(writer, o))
						break;

					if (TrySerializeObject(writer, o))
						break;

					throw new InvalidOperationException($"Unable to serialize object of type {o.GetType()}. To fix this error you must implement {nameof(ICustomSerializer)} and add it to the list pass to the serializer.");
			}
		}

		/// <summary>
		/// Deserialize an object out of a stream.
		/// </summary>
		/// <param name="stream">The stream to read.</param>
		/// <returns>The deserialized object.</returns>
		public object Deserialize(Stream stream)
		{
			using var reader = new BinaryReader(stream, Encoding.UTF8, true);
			return Deserialize(reader);
		}

		/// <summary>
		/// Deserialize an object using a BinaryReader.
		/// </summary>
		/// <param name="reader">The reader to deserialize from.</param>
		/// <returns>The deserialized object.</returns>
		private object Deserialize(BinaryReader reader)
		{
			// Serialize simple types.
			var typeId = (TypeId)reader.ReadByte();

			switch (typeId)
			{
				case TypeId.Null: return null;
				case TypeId.Bool: return reader.ReadBoolean();
				case TypeId.Byte: return reader.ReadByte();
				case TypeId.Char: return reader.ReadChar();
				case TypeId.Int16: return reader.ReadInt16();
				case TypeId.Int32: return reader.ReadInt32();
				case TypeId.Int64: return reader.ReadInt64();
				case TypeId.UInt16: return reader.ReadUInt16();
				case TypeId.UInt32: return reader.ReadUInt32();
				case TypeId.UInt64: return reader.ReadUInt64();
				case TypeId.Single: return reader.ReadSingle();
				case TypeId.Double: return reader.ReadDouble();
				case TypeId.String: return reader.ReadString();
				case TypeId.TimeSpan: return TimeSpan.FromTicks(reader.ReadInt64());
				case TypeId.DateTime: return new DateTime(reader.ReadInt64());
				case TypeId.Tuple: return DeserializeTuple(reader);
				case TypeId.Array: return DeserializeArray(reader);
				case TypeId.List: return DeserializeList(reader);
				case TypeId.Dictionary: return DeserializeDictionary(reader);
				case TypeId.Custom: return DeserializeCustom(reader);
				case TypeId.Enum: return DeserializeEnum(reader);
				case TypeId.Auto: return DeserializeAuto(reader);
			}

			throw new InvalidOperationException($"Unable to deserialize due to invalid type id {typeId}");
		}

		#region Private Methods

		private bool TrySerializeObject(BinaryWriter writer, object o)
		{
			var t = o.GetType();
			var props = o.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0).ToList();

			// Check constructor for order of parameters.

			var c = t.GetConstructors();
			var matchingCs = c.Where(c1 =>
			{
				var param = c1.GetParameters();
				return param.Length == props.Count && props.All(p => param.Any(pa => p.Name.Equals(pa.Name, StringComparison.InvariantCultureIgnoreCase) && pa.ParameterType == p.PropertyType));
			}).ToList();

			if (matchingCs.Count != 1)
			{
				var properties = string.Join(",", props.Select(p => $"\t{p.Name}({p.PropertyType})"));
				var constructors = string.Join("\n", c.Select(c1 => string.Join(",", c1.GetParameters().Select(p => $"\t{p.Name}({p.ParameterType})"))).ToList());
				throw new ArgumentException($"Unable to serialize object of type {o.GetType()} as no valid constructor can be found. Ensure there is only 1 constructor that has all parameters named the same as the properties and have the same type.\nAll Properties\n{properties}\nAll Constructors\n{constructors}");
			}

			writer.Write((byte)TypeId.Auto);
			writer.Write(GetTypeIndex(t));
			writer.Write(props.Count);
			foreach (var cp in matchingCs.First().GetParameters())
			{
				var p = props.Single(p => p.Name.Equals(cp.Name, StringComparison.InvariantCultureIgnoreCase) && cp.ParameterType == p.PropertyType);
				Serialize(writer, p.GetValue(o));
			}

			return true;
		}

		private object DeserializeAuto(BinaryReader reader)
		{
			var autoType = typeMap[reader.ReadInt32()];
			var args = new object[reader.ReadInt32()];
			for (var i = 0; i < args.Length; i++)
				args[i] = Deserialize(reader);

			return Activator.CreateInstance(autoType, args);
		}

		private bool TrySerializeCollection(BinaryWriter writer, object o)
		{
			return TrySerializeTuple(writer, o) || TrySerializeArray(writer, o) || TrySerializeList(writer, o) || TrySerializeDictionary(writer, o);
		}

		private int GetTypeIndex(Type t)
		{
			for (var i = 0; i < typeMap.Count; i++)
			{
				if (t == typeMap[i])
					return i;
			}

			typeMap.Add(t);
			return typeMap.Count - 1;
		}

		private bool TrySerializeTuple(BinaryWriter writer, object o)
		{
			if (!(o is ITuple tuple))
				return false;

			var t = o.GetType();
			writer.Write((byte)TypeId.Tuple);
			writer.Write(GetTypeIndex(t));
			writer.Write(tuple.Length);
			for (var i = 0; i < tuple.Length; i++)
				Serialize(writer, tuple[i]);

			return true;
		}

		private object DeserializeTuple(BinaryReader reader)
		{
			var tupleType = typeMap[reader.ReadInt32()];
			var args = new object[reader.ReadInt32()];
			for (var i = 0; i < args.Length; i++)
				args[i] = Deserialize(reader);

			return Activator.CreateInstance(tupleType, args);
		}

		private bool TrySerializeArray(BinaryWriter writer, object o)
		{
			var t = o.GetType();
			if (!t.IsArray || t.GetArrayRank() != 1)
				return false;

			writer.Write((byte)TypeId.Array);
			writer.Write(GetTypeIndex(t));

			var array = (Array)o;
			writer.Write(array.Length);

			for (var i = 0; i < array.Length; i++)
				Serialize(writer, array.GetValue(i));

			return true;
		}

		private object DeserializeArray(BinaryReader reader)
		{
			var arrayType = typeMap[reader.ReadInt32()];
			var newArray = Array.CreateInstance(arrayType.GetElementType(), reader.ReadInt32());

			for (var i = 0; i < newArray.Length; i++)
				newArray.SetValue(Deserialize(reader), i);

			return newArray;
		}

		private bool TrySerializeList(BinaryWriter writer, object o)
		{
			var t = o.GetType();

			if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(List<>))
				return false;

			writer.Write((byte)TypeId.List);
			writer.Write(GetTypeIndex(t));

			var list = (IList)o;
			writer.Write(list.Count);

			for (var i = 0; i < list.Count; i++)
				Serialize(writer, list[i]);

			return true;
		}

		private object DeserializeList(BinaryReader reader)
		{
			var listType = typeMap[reader.ReadInt32()];
			var newList = (IList)Activator.CreateInstance(listType);
			var elementCount = reader.ReadInt32();

			for (var i = 0; i < elementCount; i++)
				newList.Add(Deserialize(reader));

			return newList;
		}

		private bool TrySerializeDictionary(BinaryWriter writer, object o)
		{
			var t = o.GetType();

			if (!t.IsGenericType || t.GetGenericTypeDefinition() != typeof(Dictionary<,>))
				return false;

			writer.Write((byte)TypeId.Dictionary);
			writer.Write(GetTypeIndex(t));

			var dictionary = (IDictionary)o;
			writer.Write(dictionary.Count);

			foreach (var key in dictionary.Keys)
			{
				Serialize(writer, key);
				Serialize(writer, dictionary[key]);
			}

			return true;
		}

		private object DeserializeDictionary(BinaryReader reader)
		{
			var dictType = typeMap[reader.ReadInt32()];
			var newDict = (IDictionary)Activator.CreateInstance(dictType);
			var elementCount = reader.ReadInt32();

			for (var i = 0; i < elementCount; i++)
				newDict.Add(Deserialize(reader), Deserialize(reader));

			return newDict;
		}

		private bool TrySerializeCustom(BinaryWriter writer, object o)
		{
			var t = o.GetType();
			foreach (var s in customSerializers)
			{
				if (s.SupportsType(t))
				{
					writer.Write((byte)TypeId.Custom);
					writer.Write(GetTypeIndex(t));

					s.Serialize(writer, Serialize, o);
					return true;
				}
			}

			return false;
		}

		private object DeserializeCustom(BinaryReader reader)
		{
			var customType = typeMap[reader.ReadInt32()];
			foreach (var s in customSerializers)
			{
				if (s.SupportsType(customType))
					return s.Deserialize(customType, reader, Deserialize);
			}

			throw new InvalidOperationException($"Deserializer for type {customType.FullName} is not installed.");
		}

		private object DeserializeEnum(BinaryReader reader)
		{
			var customType = typeMap[reader.ReadInt32()];
			var value = Deserialize(reader);
			return Enum.ToObject(customType, value);
		}

		#endregion
	}
}