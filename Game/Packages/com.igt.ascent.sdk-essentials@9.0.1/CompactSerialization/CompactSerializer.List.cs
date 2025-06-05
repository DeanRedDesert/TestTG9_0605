//-----------------------------------------------------------------------
// <copyright file = "CompactSerializer.List.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// While ReSharper and Visual Studio can infer the types, Mono cannot.
// ReSharper disable RedundantTypeArgumentsOfMethod
namespace IGT.Game.Core.CompactSerialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// This class provides functions of writing and reading a generic
    /// List of a supported base type data.
    /// These functions can be utilized by other classes that implements
    /// ICompactSerializable or other custom serialization.
    /// </summary>
    public partial class CompactSerializer
    {
        #region Writing List of TItem

        /// <summary>
        /// Write a generic list of type <typeparamref name="TItem"/> data
        /// to the stream.
        /// </summary>
        /// <typeparam name="TItem">The type of the list item.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="list">The list to write.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        /// Thrown when the element of <paramref name="list"/> is of
        /// unsupported type.
        /// </exception>
        public static void WriteList<TItem>(Stream stream, IList<TItem> list)
        {
            WriteList(stream, list as IList, typeof(TItem));
        }
        
        /// <summary>
        /// Write a generic list of type <paramref name="itemType"/> data
        /// to the stream.
        /// </summary>
        /// <param name="itemType">The type of the list item.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="list">The list to write.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        /// <exception cref="CompactSerializationException">
        /// Thrown when the element of <paramref name="list"/> is of
        /// unsupported type.
        /// </exception>
        private static void WriteList(Stream stream, IList list, Type itemType)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if(list == null)
            {
                // The list is null.
                Write(stream, true);
            }
            else
            {
                // The list is not null.
                Write(stream, false);

                var compactType = GetCompactType(itemType);

                // Check if the collection's element type is supported.
                // No nested collection is allowed.
                if(compactType.BaseType.TypeId == TypeId.TypeNotSupported)
                {
                    throw new CompactSerializationException(
                        $"List element type {itemType} is not supported by CompactSerializer.");
                }

                var count = list.Count;
                Write(stream, count);
                WriteCollectionType(stream, list, compactType);
            }
        }        

        /// <summary>
        /// Writes a list based on the collection type of the elements contained in the list.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="list">The list to write.</param>
        /// <param name="compactType">Type of data to go in the list.</param>
        private static void WriteCollectionType(Stream stream, IList list, CompactType compactType)
        {
            switch (compactType.Collection)
            {
                case CollectionId.CollectionList:
                    foreach (var data in list)
                    {
                        WriteList(stream, data as IList, compactType.BaseType.ImplementationType);
                    }
                    break;
                case CollectionId.CollectionNone:
                    foreach (var data in list)
                    {
                        WriteBaseType(stream, data, compactType.BaseType.TypeId);
                    }
                    break;
                case CollectionId.CollectionDictionary:
                    {
                        foreach (IDictionary data in list)
                        {
                            WriteDictionary(stream, data, compactType.BaseType.ImplementationType,
                                            compactType.SecondType.ImplementationType);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Reading List of Base Types

        /// <summary>
        /// Utility function to read a generic list of type
        /// <typeparamref name="TItem"/> data from the stream.
        /// </summary>
        /// <typeparam name="TItem">The type of the list item.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <param name="itemReader">The function to read one <typeparamref name="TItem"/> at a time.</param>
        /// <returns>The list of <typeparamref name="TItem"/> values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        private static List<TItem> ReadList<TItem>(Stream stream, Func<Stream, TItem> itemReader)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            List<TItem> result = null;

            // The list is not null.
            if(!ReadBool(stream))
            {
                var count = ReadInt(stream);

                result = new List<TItem>(count);
                for(var i = 0; i < count; i++)
                {
                    var data = itemReader(stream);
                    result.Add(data);
                }
            }

            return result;
        }

        /// <summary>
        /// Read a list of Boolean values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Boolean values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<bool> ReadListBool(Stream stream)
        {
            return ReadList<bool>(stream, ReadBool);
        }

        /// <summary>
        /// Read a list of Byte values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Byte values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<byte> ReadListByte(Stream stream)
        {
            return ReadList<byte>(stream, ReadByte);
        }

        /// <summary>
        /// Read a list of Char values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Char values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<char> ReadListChar(Stream stream)
        {
            return ReadList<char>(stream, ReadChar);
        }

        /// <summary>
        /// Read a list of Int16 values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Int16 values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<short> ReadListShort(Stream stream)
        {
            return ReadList<short>(stream, ReadShort);
        }

        /// <summary>
        /// Read a list of Int32 values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Int32 values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<int> ReadListInt(Stream stream)
        {
            return ReadList<int>(stream, ReadInt);
        }

        /// <summary>
        /// Read a list of Int64 values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Int64 values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<long> ReadListLong(Stream stream)
        {
            return ReadList<long>(stream, ReadLong);
        }

        /// <summary>
        /// Read a list of UInt16 values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of UInt16 values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<ushort> ReadListUshort(Stream stream)
        {
            return ReadList<ushort>(stream, ReadUshort);
        }

        /// <summary>
        /// Read a list of UInt32 values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of UInt32 values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<uint> ReadListUint(Stream stream)
        {
            return ReadList<uint>(stream, ReadUint);
        }

        /// <summary>
        /// Read a list of UInt64 values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of UInt64 values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<ulong> ReadListUlong(Stream stream)
        {
            return ReadList<ulong>(stream, ReadUlong);
        }

        /// <summary>
        /// Read a list of Single values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Single values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<float> ReadListFloat(Stream stream)
        {
            return ReadList<float>(stream, ReadFloat);
        }

        /// <summary>
        /// Read a list of Double values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Double values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<double> ReadListDouble(Stream stream)
        {
            return ReadList<double>(stream, ReadDouble);
        }

        /// <summary>
        /// Read a list of Decimal values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Decimal values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<decimal> ReadListDecimal(Stream stream)
        {
            return ReadList<decimal>(stream, ReadDecimal);
        }

        /// <summary>
        /// Read a list of nullable values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of nullable values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<T?> ReadListNullable<T>(Stream stream) where T : struct
        {
            return ReadList<T?>(stream, ReadNullable<T>);
        }

        /// <summary>
        /// Read a list of String values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of String values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<string> ReadListString(Stream stream)
        {
            return ReadList<string>(stream, ReadString);
        }

        /// <summary>
        /// Read a list of Byte arrays from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of Byte arrays read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<byte[]> ReadListByteArray(Stream stream)
        {
            return ReadList<byte[]>(stream, ReadByteArray);
        }

        /// <summary>
        /// Read a list of compact serializable values from a stream.
        /// </summary>
        /// <typeparam name="TItem">
        /// The type of the list item.  It should implement <see cref="ICompactSerializable"/>,
        /// and has a public parameter-less constructor.
        /// </typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The list of compact serializable values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        public static List<TItem> ReadListSerializable<TItem>(Stream stream) where TItem : ICompactSerializable, new()
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            List<TItem> result = null;

            // The list is not null.
            if(!ReadBool(stream))
            {
                var count = ReadInt(stream);
                result = new List<TItem>(count);
                for(var i = 0; i < count; i++)
                {
                    var data = ReadSerializable<TItem>(stream);
                    result.Add(data);
                }
            }

            return result;
        }

        /// <summary>
        /// Read a list of enum values from a stream.
        /// </summary>
        /// <param name="stream">The stream to read from.</param>
        /// <typeparam name="TEnum">The enum data type being read.</typeparam>
        /// <returns>The list of enum values read.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="stream"/> is null.
        /// </exception>
        /// <exception cref="System.InvalidCastException">
        /// Thrown when <typeparamref name="TEnum"/> is not an enum.
        /// </exception>
        public static List<TEnum> ReadListEnum<TEnum>(Stream stream)
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new InvalidCastException("The specified data type is not a enum.");
            }

            return ReadList<TEnum>(stream, ReadEnum<TEnum>);
        }

        /// <summary>
        /// Reads a list containing nested collections.
        /// Currently supports Lists and Dictionaries.
        /// </summary>
        /// <typeparam name="TItem">Type of the collection.</typeparam>
        /// <param name="stream">Data stream to read from.</param>
        /// <exception cref="CompactSerializationException">thrown if data type is not a supported collection</exception>
        /// <returns>Unserialized result</returns>
        public static List<TItem> ReadNestedList<TItem>(Stream stream)
        {
            var compactType = GetCompactType(typeof(TItem));
            switch (compactType.Collection)
            {
                case CollectionId.CollectionList:
                    return ReadList<TItem>(stream, ReadListFromList<TItem>);
                case CollectionId.CollectionDictionary:
                    return ReadList<TItem>(stream, ReadDictionaryFromList<TItem>);
                default:
                    throw new CompactSerializationException("Unsupported CollectionID " + compactType.Collection);
            }
        }

        /// <summary>
        /// Reads a dictionary from a list.
        /// </summary>
        /// <typeparam name="TDictionary">Type of the dictionary.</typeparam>
        /// <param name="stream">Stream to read data from.</param>
        /// <returns>Dictionary element of the List.</returns>
        private static TDictionary ReadDictionaryFromList<TDictionary>(Stream stream)
        {
            var dataType = typeof(TDictionary);
            return (TDictionary)InvokeReadDictionary(dataType, dataType.GetGenericArguments(), stream);
        }

        /// <summary>
        /// Reads a list from a list.
        /// </summary>
        /// <typeparam name="TList">Type of the list.</typeparam>
        /// <param name="stream">Stream to read data from.</param>
        /// <returns>List element of the List.</returns>
        private static TList ReadListFromList<TList>(Stream stream)
        {
            var listType = typeof(TList);
            var compactType = GetCompactType(listType);
            switch (compactType.BaseType.TypeId)
            {
                case TypeId.TypeNestedCollection:
                    return (TList)InvokeReadNestedList(compactType.BaseType.ImplementationType, stream);

                default:
                    return (TList)ReadCollectionList(stream, compactType);
            }
        }

        #endregion

        #region Invoking Generic List Read

        /// <summary>
        /// The cached reflection of method <see cref="ReadNestedList{TItem}"/>.
        /// </summary>
        /// <remarks>
        /// This static field is local to a thread, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static MethodInfo methodReadNestedListReflection;

        /// <summary>
        /// The cached reflection of method <see cref="ReadListSerializable{TItem}"/>.
        /// </summary>
        /// <remarks>
        /// This static field is local to a thread, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static MethodInfo methodReadListSerializableReflection;

        /// <summary>
        /// The cached reflection of method <see cref="ReadListEnum{TItem}"/>.
        /// </summary>
        /// <remarks>
        /// This static field is local to a thread, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static MethodInfo methodReadListEnumReflection;

        /// <summary>
        /// The dynamically emitted, specifically typed methods from the generic read list methods;
        /// indexed by the list element type.
        /// </summary>
        /// <remarks>
        /// This static field is thread local, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static Dictionary<Type, MethodInfo> methodReadListImpls;

        /// <summary>
        /// Invoke the method <see cref="ReadNestedList{TItem}"/> with the given type parameter.
        /// </summary>
        /// <param name="elementType">The type of the collection item.</param>
        /// <param name="arguments">The arguments indicates the stream to read from.</param>
        /// <returns>The list of <paramref name="elementType"/> items.</returns>
        private static object InvokeReadNestedList(Type elementType, params object[] arguments)
        {
            if(methodReadNestedListReflection == null)
            {
                methodReadNestedListReflection = typeof(CompactSerializer).GetMethod("ReadNestedList");
            }

            return InvokeReadList(methodReadNestedListReflection, elementType, arguments);
        }

        /// <summary>
        /// Invoke the method <see cref="ReadListSerializable{TItem}"/> with the given type parameter.
        /// </summary>
        /// <param name="elementType">The type of the collection item.</param>
        /// <param name="arguments">The arguments indicates the stream to read from.</param>
        /// <returns>The list of <paramref name="elementType"/> items.</returns>
        private static object InvokeReadListSerializable(Type elementType, params object[] arguments)
        {
            if(methodReadListSerializableReflection == null)
            {
                methodReadListSerializableReflection = typeof(CompactSerializer).GetMethod("ReadListSerializable");
            }

            return InvokeReadList(methodReadListSerializableReflection, elementType, arguments);
        }

        /// <summary>
        /// Invoke the method <see cref="ReadListEnum{TEnum}"/> with the given type parameter.
        /// </summary>
        /// <param name="elementType">The type of the collection item.</param>
        /// <param name="arguments">The arguments indicates the stream to read from.</param>
        /// <returns>The list of <paramref name="elementType"/> items.</returns>
        private static object InvokeReadListEnum(Type elementType, params object[] arguments)
        {
            if(methodReadListEnumReflection == null)
            {
                methodReadListEnumReflection = typeof(CompactSerializer).GetMethod("ReadListEnum");
            }

            return InvokeReadList(methodReadListEnumReflection, elementType, arguments);
        }

        /// <summary>
        /// Invoke the generic read list method with the given method name and the type parameter.
        /// </summary>
        /// <param name="methodReadListReflection">The reflection of the read list method to inovke.</param>
        /// <param name="elementType">The type of the collection item.</param>
        /// <param name="arguments">The arguments indicates the stream to read from.</param>
        /// <returns>The list of <paramref name="elementType"/> items.</returns>
        private static object InvokeReadList(MethodInfo methodReadListReflection, Type elementType, params object[] arguments)
        {
            if(methodReadListImpls == null)
            {
                methodReadListImpls = new Dictionary<Type, MethodInfo>();
            }

            if(!methodReadListImpls.ContainsKey(elementType))
            {
                methodReadListImpls[elementType] = methodReadListReflection.MakeGenericMethod(elementType);
            }

            return methodReadListImpls[elementType].Invoke(null, arguments);
        }

        #endregion
    }
}
