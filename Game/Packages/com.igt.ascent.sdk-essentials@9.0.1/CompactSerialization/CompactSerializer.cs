//-----------------------------------------------------------------------
// <copyright file = "CompactSerializer.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.CompactSerialization
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;

    #endregion

    /// <summary>
    ///   This class serializes and deserializes supported types into
    ///   a compact binary format.
    /// </summary>
    /// <remarks>
    ///   This class supports the serialization and deserialization of
    ///   the following types:
    ///   <list type = "bullet">
    ///     <item>System.Boolean</item>
    ///     <item>System.Byte</item>
    ///     <item>System.Char</item>
    ///     <item>System.Int16</item>
    ///     <item>System.Int32</item>
    ///     <item>System.Int64</item>
    ///     <item>System.UInt16</item>
    ///     <item>System.UInt32</item>
    ///     <item>System.UInt64</item>
    ///     <item>System.Single</item>
    ///     <item>System.Double</item>
    ///     <item>System.Decimal</item>
    ///     <item>Nullable types T? where T is any of the above types</item>
    ///     <item>System.String</item>
    ///     <item>System.Byte[]</item>
    ///     <item>System.Enum</item>
    ///     <item>Any type that implements <see cref = "ICompactSerializable" /></item>
    ///     <item>List(T) or IList(T) of any of the above types.</item>
    ///     <item>Dictionary(T,T) or IDictionary(T,T) of any of the above types.</item>
    ///   </list>
    /// 
    ///   Use Serialize method if the data type is unknown to the caller, use Write methods
    ///   (Write, WriteList, WriteDictionary) if the caller knows what type the data is.
    ///   However, when calling Deserialize or the proper Read method, the caller must know the
    ///   data type and instruct the CompactSerializer to deserialize the data bytes into that
    ///   type because compact serialization does not record any type information when serializing
    ///   the original data.
    /// 
    ///   The main usage of this class is to serialize and deserialize the critical
    ///   data by Standard Game Lib in order to reduce the safe storage footprint.
    /// </remarks>
    /// <seealso cref = "ICompactSerializable" />
    public partial class CompactSerializer
    {
        #region Nested Classes

        /// <summary>
        ///   This enumeration is used to represent the base types
        ///   supported by Compact Serializer.
        /// </summary>
        private enum TypeId : byte
        {
            TypeNotSupported,
            TypeSerializable,
            TypeBool,
            TypeByte,
            TypeChar,
            TypeShort,
            TypeInt,
            TypeLong,
            TypeUshort,
            TypeUint,
            TypeUlong,
            TypeFloat,
            TypeDouble,
            TypeDecimal,
            TypeNullableBool,
            TypeNullableByte,
            TypeNullableChar,
            TypeNullableShort,
            TypeNullableInt,
            TypeNullableLong,
            TypeNullableUshort,
            TypeNullableUint,
            TypeNullableUlong,
            TypeNullableFloat,
            TypeNullableDouble,
            TypeNullableDecimal,
            TypeString,
            TypeByteArray,
            TypeEnum,
            TypeNestedCollection,
        }

        /// <summary>
        ///   This enumeration is used to represent the generic
        ///   collection types supported by Compact Serializer.
        /// </summary>
        private enum CollectionId : byte
        {
            CollectionNone,
            CollectionList,
            CollectionDictionary,
            CollectionReadOnlyCollection,
        }

        /// <summary>
        ///   This structure encapsulates the information of a compact data type.
        /// </summary>
        private struct CompactType
        {
            /// <summary>
            ///   Whether the type is a generic collection.
            /// </summary>
            public CollectionId Collection;

            /// <summary>
            ///   The basic data type or the first generic type used.
            /// </summary>
            public DataType BaseType;

            /// <summary>
            ///   The second generic type used (if needed).
            /// </summary>
            public DataType SecondType;
        }

        /// <summary>
        ///   This structure encapsulates the data type information.
        /// </summary>
        private struct DataType
        {
            /// <summary>
            ///   Whether the data type, or the type of the collection
            ///   element is a supported type.
            /// </summary>
            public TypeId TypeId;

            /// <summary>
            ///   The data type which represents the implementation of Type.
            /// </summary>
            public Type ImplementationType;
        }

        #endregion

        #region Static Fields

        /// <summary>
        ///   Lookup table to get the id of a given supported data type.
        /// </summary>
        private static readonly Dictionary<Type, TypeId> SupportedTypes =
            new Dictionary<Type, TypeId>
                {
                    {typeof(bool), TypeId.TypeBool},
                    {typeof(byte), TypeId.TypeByte},
                    {typeof(char), TypeId.TypeChar},
                    {typeof(short), TypeId.TypeShort},
                    {typeof(int), TypeId.TypeInt},
                    {typeof(long), TypeId.TypeLong},
                    {typeof(ushort), TypeId.TypeUshort},
                    {typeof(uint), TypeId.TypeUint},
                    {typeof(ulong), TypeId.TypeUlong},
                    {typeof(float), TypeId.TypeFloat},
                    {typeof(double), TypeId.TypeDouble},
                    {typeof(decimal), TypeId.TypeDecimal},
                    {typeof(bool?), TypeId.TypeNullableBool},
                    {typeof(byte?), TypeId.TypeNullableByte},
                    {typeof(char?), TypeId.TypeNullableChar},
                    {typeof(short?), TypeId.TypeNullableShort},
                    {typeof(int?), TypeId.TypeNullableInt},
                    {typeof(long?), TypeId.TypeNullableLong},
                    {typeof(ushort?), TypeId.TypeNullableUshort},
                    {typeof(uint?), TypeId.TypeNullableUint},
                    {typeof(ulong?), TypeId.TypeNullableUlong},
                    {typeof(float?), TypeId.TypeNullableFloat},
                    {typeof(double?), TypeId.TypeNullableDouble},
                    {typeof(decimal?), TypeId.TypeNullableDecimal},
                    {typeof(string), TypeId.TypeString},
                    {typeof(byte[]), TypeId.TypeByteArray},
                };

        /// <summary>
        ///   Lookup table to get the id of a given supported collection type.
        /// </summary>
        private static readonly Dictionary<Type, CollectionId> SupportedCollections =
            new Dictionary<Type, CollectionId>
                {
                    {typeof(List<>), CollectionId.CollectionList},
                    {typeof(IList<>), CollectionId.CollectionList},
                    {typeof(IReadOnlyList<>), CollectionId.CollectionList},
                    {typeof(Dictionary<,>), CollectionId.CollectionDictionary},
                    {typeof(IDictionary<,>), CollectionId.CollectionDictionary},
                    {typeof(IReadOnlyDictionary<,>), CollectionId.CollectionDictionary},
                    {typeof(ReadOnlyCollection<>), CollectionId.CollectionReadOnlyCollection}
                };

        /// <summary>
        ///   Constant to write in place of a null string or a null array.
        /// </summary>
        private const int NullLength = -1;

        /// <summary>
        ///   Type for doing interface compatibility checks.
        /// </summary>
        private static readonly Type CompactSerializableType = typeof(ICompactSerializable);

        /// <summary>
        /// The cached map from the real type to the according compact type.
        /// </summary>
        /// <remarks>
        /// This static field is local to a thread, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static Dictionary<Type, CompactType> compactTypeMap;

        #endregion

        #region Public Methods

        /// <summary>
        ///   Check if a given data type is supported by Compact Serializer.
        /// </summary>
        /// <param name = "dataType">The data type to check.</param>
        /// <returns>True if the type is supported.  False otherwise.</returns>
        public static bool Supports(Type dataType)
        {
            var compactType = GetCompactType(dataType);
            return compactType.BaseType.TypeId != TypeId.TypeNotSupported;
        }

        /// <summary>
        ///   Serialize a data to a stream.
        /// </summary>
        /// <param name = "stream">The stream where the data is serialized into.</param>
        /// <param name = "data">The data to be serialized.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when <paramref name = "data" /> is of an unsupported type.
        /// </exception>
        public static void Serialize<T>(Stream stream, T data)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var typeOfDeclaration = typeof(T);
            var typeUnderlying = typeOfDeclaration.IsValueType || data == null ? typeOfDeclaration : data.GetType();
            var compactType = GetCompactType(typeUnderlying);

            switch(compactType.Collection)
            {
                case CollectionId.CollectionNone:
                    WriteBaseType(stream, data, compactType.BaseType.TypeId);
                    break;

                case CollectionId.CollectionReadOnlyCollection:
                case CollectionId.CollectionList:
                    WriteList(stream, data as IList, compactType.BaseType.ImplementationType);
                    break;

                case CollectionId.CollectionDictionary:
                    WriteDictionary(stream, data as IDictionary, compactType.BaseType.ImplementationType,
                                    compactType.SecondType.ImplementationType);
                    break;

                default:
                    throw new CompactSerializationException(
                        $"Type {typeUnderlying} is not supported by CompactSerializer.");
            }
        }

        /// <summary>
        ///   Deserialize data from a stream.
        /// </summary>
        /// <typeparam name = "T">The type of the data to be deserialized.</typeparam>
        /// <param name = "stream">The stream where the data to be read from.</param>
        /// <returns>The data after deserialization.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when <typeparamref name = "T" /> is an unsupported type.
        /// </exception>
        public static T Deserialize<T>(Stream stream)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            return (T) Deserialize(stream, typeof(T));
        }

        /// <summary>
        /// Deserialize the data from a stream.
        /// </summary>
        /// <typeparam name = "T">The type of the data to be deserialized.</typeparam>
        /// <param name = "data">The data to be filled.</param>
        /// <param name = "stream">The stream where the data to be read from.</param>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when <typeparamref name="T"/> is an unsupported type.
        /// </exception>
        /// <remarks>
        /// If <paramref name="data"/> is not null and implements ICompactSerializable, an in-place deserialization
        /// will be performed; otherwise, a new instance will be created and assigned to <paramref name="data"/> after
        /// the deserialization.
        /// </remarks>
        public static void Deserialize<T>(ref T data, Stream stream)
        {
            var dataType = typeof(T);

            if(typeof(ICompactSerializable).IsAssignableFrom(dataType) && data != null)
            {
                var serializableObject = (ICompactSerializable)data;
                Deserialize(ref serializableObject, stream, dataType);
                data = (T)serializableObject;
            }
            else
            {
                data = Deserialize<T>(stream);
            }
        }

        /// <summary>
        ///   Deserialize data from a stream.
        /// </summary>
        /// <param name = "stream">The stream where the data to be read from.</param>
        /// <param name="dataType">The type of the data to be deserialized.</param>
        /// <returns>The data after deserialization.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "stream" /> is null.
        /// </exception>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when <paramref name = "dataType" /> is an unsupported type.
        /// </exception>
        public static object Deserialize(Stream stream, Type dataType)
        {
            object result;
            var compactType = GetCompactType(dataType);

            switch(compactType.Collection)
            {
                case CollectionId.CollectionNone:
                    result = ReadBaseType(stream, compactType.BaseType);

                    break;

                case CollectionId.CollectionList:
                    result = compactType.BaseType.TypeId != TypeId.TypeNestedCollection ? 
                        ReadCollectionList(stream, compactType) : 
                        InvokeReadNestedList(compactType.BaseType.ImplementationType, stream);
                    break;

                case CollectionId.CollectionDictionary:
                    //This is an invoke method because the ReadDictionary function requires a determined input type at compile time.
                    result = InvokeReadDictionary(
                        dataType,
                        new[]
                        {
                            compactType.BaseType.ImplementationType,
                            compactType.SecondType.ImplementationType
                        },
                        stream);
                    break;

                case CollectionId.CollectionReadOnlyCollection:
                    result = InvokeReadReadOnlyCollection(compactType.BaseType.ImplementationType, stream);
                    break;

                default:
                    throw new CompactSerializationException(
                        $"Type {dataType} is not supported by CompactSerializer.");
            }

            return result;
        }

        #endregion

        #region Private Methods

        /// <summary>
        ///   Get the information on a data type that can be utilized
        ///   by Compact Serializer.
        /// </summary>
        /// <param name = "dataType">The type whose information to be retrieved.</param>
        /// <returns>The compact type information.</returns>
        /// <exception cref = "ArgumentNullException">
        ///   Thrown when <paramref name = "dataType" /> is null.
        /// </exception>
        private static CompactType GetCompactType(Type dataType)
        {
            if(dataType == null)
            {
                throw new ArgumentNullException(nameof(dataType));
            }

            if(compactTypeMap == null)
            {
                compactTypeMap = new Dictionary<Type, CompactType>();
            }

            if(!compactTypeMap.ContainsKey(dataType))
            {
                var result = new CompactType();

                // If it is a base type, set the base type id and
                // leave the collection id as None.
                if(SupportedTypes.ContainsKey(dataType))
                {
                    result.BaseType.TypeId = SupportedTypes[dataType];
                    result.BaseType.ImplementationType = dataType;
                }
                else if(dataType.IsEnum)
                {
                    result.BaseType.TypeId = TypeId.TypeEnum;
                    result.BaseType.ImplementationType = dataType;
                }
                else if(CompactSerializableType.IsAssignableFrom(dataType))
                {
                    result.BaseType.TypeId = TypeId.TypeSerializable;
                    result.BaseType.ImplementationType = dataType;
                }
                // If it is a close constructed generic type, and...
                else if(dataType.IsGenericType &&
                        !dataType.ContainsGenericParameters)
                {
                    var typeDefinition = dataType.GetGenericTypeDefinition();

                    // it is a supported collection type, and...
                    if(typeDefinition != null && SupportedCollections.ContainsKey(typeDefinition))
                    {
                        var genericArgs = dataType.GetGenericArguments();

                        switch(genericArgs.Length)
                        {
                            // it has only one generic type argument...
                            case 1:
                            {
                                result.BaseType = ExtractDataTypeFromType(genericArgs[0]);
                                if(result.BaseType.TypeId != TypeId.TypeNotSupported)
                                {
                                    result.Collection = SupportedCollections[typeDefinition];
                                }

                                break;
                            }
                            
                            // it has two generic type arguments ...
                            case 2:
                            {
                                result.BaseType = ExtractDataTypeFromType(genericArgs[0]);
                                result.SecondType = ExtractDataTypeFromType(genericArgs[1]);
                                if(result.BaseType.TypeId != TypeId.TypeNotSupported &&
                                   result.SecondType.TypeId != TypeId.TypeNotSupported &&
                                   result.BaseType.TypeId != TypeId.TypeNestedCollection)
                                {
                                    result.Collection = SupportedCollections[typeDefinition];
                                }
                                else
                                {
                                    // Reset both to not supported since this data type is partially unsupported
                                    result.BaseType.TypeId = TypeId.TypeNotSupported;
                                    result.SecondType.TypeId = TypeId.TypeNotSupported;
                                }

                                break;
                            }
                        }
                    }
                }
                compactTypeMap[dataType] = result;
            }

            return compactTypeMap[dataType];
        }

        /// <summary>
        /// Extract the data type information from the input type.
        /// </summary>
        /// <param name="type">The input type.</param>
        /// <returns>The extracted data type information.</returns>
        private static DataType ExtractDataTypeFromType(Type type)
        {
            var extractedDataType = new DataType
                                    {
                                        TypeId = GetTypeIdFromType(type)
                                    };

            if(extractedDataType.TypeId != TypeId.TypeNotSupported)
            {
                extractedDataType.ImplementationType = type;
            }

            return extractedDataType;
        }

        /// <summary>
        ///   Determine the matching type ID from a System.Type.
        /// </summary>
        /// <param name = "type">The System.Type to examine.</param>
        /// <returns>The matching type id otherwise TypeNotSupported if there is no match.</returns>
        private static TypeId GetTypeIdFromType(Type type)
        {
            if(SupportedTypes.ContainsKey(type))
            {
                return SupportedTypes[type];
            }
            if(CompactSerializableType.IsAssignableFrom(type))
            {
                return TypeId.TypeSerializable;
            }
            if(type.IsEnum)
            {
                return TypeId.TypeEnum;
            }

            if(type.IsGenericType &&
               !type.ContainsGenericParameters)
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                if(typeDefinition != null && SupportedCollections.ContainsKey(typeDefinition))
                {
                    return TypeId.TypeNestedCollection;
                }
            }
            return TypeId.TypeNotSupported;
        }

        /// <summary>
        ///   Write a supported base type data to the stream.
        /// </summary>
        /// <param name = "stream">The stream where the data is written to.</param>
        /// <param name = "data">The data to be written.</param>
        /// <param name = "baseType">The base type id.</param>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when the write operation for <paramref name = "baseType" /> is not implemented yet.
        /// </exception>
        private static void WriteBaseType(Stream stream, object data, TypeId baseType)
        {
            switch(baseType)
            {
                case TypeId.TypeBool:
                    Write(stream, (bool) data);
                    break;

                case TypeId.TypeByte:
                    Write(stream, (byte) data);
                    break;

                case TypeId.TypeChar:
                    Write(stream, (char) data);
                    break;

                case TypeId.TypeShort:
                    Write(stream, (short) data);
                    break;

                case TypeId.TypeInt:
                    Write(stream, (int) data);
                    break;

                case TypeId.TypeLong:
                    Write(stream, (long) data);
                    break;

                case TypeId.TypeUshort:
                    Write(stream, (ushort) data);
                    break;

                case TypeId.TypeUint:
                    Write(stream, (uint) data);
                    break;

                case TypeId.TypeUlong:
                    Write(stream, (ulong) data);
                    break;

                case TypeId.TypeFloat:
                    Write(stream, (float) data);
                    break;

                case TypeId.TypeDouble:
                    Write(stream, (double) data);
                    break;

                case TypeId.TypeDecimal:
                    Write(stream, (decimal)data);
                    break;

                case TypeId.TypeNullableBool:
                    Write(stream, (bool?)data);
                    break;

                case TypeId.TypeNullableByte:
                    Write(stream, (byte?)data);
                    break;

                case TypeId.TypeNullableChar:
                    Write(stream, (char?)data);
                    break;

                case TypeId.TypeNullableShort:
                    Write(stream, (short?)data);
                    break;

                case TypeId.TypeNullableInt:
                    Write(stream, (int?)data);
                    break;

                case TypeId.TypeNullableLong:
                    Write(stream, (long?)data);
                    break;

                case TypeId.TypeNullableUshort:
                    Write(stream, (ushort?)data);
                    break;

                case TypeId.TypeNullableUint:
                    Write(stream, (uint?)data);
                    break;

                case TypeId.TypeNullableUlong:
                    Write(stream, (ulong?)data);
                    break;

                case TypeId.TypeNullableFloat:
                    Write(stream, (float?)data);
                    break;

                case TypeId.TypeNullableDouble:
                    Write(stream, (double?)data);
                    break;

                case TypeId.TypeNullableDecimal:
                    Write(stream, (decimal?)data);
                    break;

                case TypeId.TypeString:
                    Write(stream, (string) data);
                    break;

                case TypeId.TypeByteArray:
                    Write(stream, (byte[]) data);
                    break;

                case TypeId.TypeSerializable:
                    Write(stream, data as ICompactSerializable);
                    break;

                case TypeId.TypeEnum:
                    Write(stream, (Enum) data);
                    break;

                default:
                    throw new CompactSerializationException(
                        $"Type {data.GetType()} is not supported by CompactSerializer.");
            }
        }

        /// <summary>
        ///   Read a supported base type data from the stream.
        /// </summary>
        /// <param name = "stream">The stream where the data is read from.</param>
        /// <param name = "baseType">The base type id.</param>
        /// <returns>The data read from the stream.</returns>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when the read operation for <paramref name = "baseType" /> is not implemented yet.
        /// </exception>
        private static object ReadBaseType(Stream stream, DataType baseType)
        {
            object result;

            switch(baseType.TypeId)
            {
                case TypeId.TypeBool:
                    result = ReadBool(stream);
                    break;

                case TypeId.TypeByte:
                    result = ReadByte(stream);
                    break;

                case TypeId.TypeChar:
                    result = ReadChar(stream);
                    break;

                case TypeId.TypeShort:
                    result = ReadShort(stream);
                    break;

                case TypeId.TypeInt:
                    result = ReadInt(stream);
                    break;

                case TypeId.TypeLong:
                    result = ReadLong(stream);
                    break;

                case TypeId.TypeUshort:
                    result = ReadUshort(stream);
                    break;

                case TypeId.TypeUint:
                    result = ReadUint(stream);
                    break;

                case TypeId.TypeUlong:
                    result = ReadUlong(stream);
                    break;

                case TypeId.TypeFloat:
                    result = ReadFloat(stream);
                    break;

                case TypeId.TypeDouble:
                    result = ReadDouble(stream);
                    break;

                case TypeId.TypeDecimal:
                    result = ReadDecimal(stream);
                    break;

                case TypeId.TypeNullableBool:
                    result = ReadNullable<bool>(stream);
                    break;

                case TypeId.TypeNullableByte:
                    result = ReadNullable<byte>(stream);
                    break;

                case TypeId.TypeNullableChar:
                    result = ReadNullable<char>(stream);
                    break;

                case TypeId.TypeNullableShort:
                    result = ReadNullable<short>(stream);
                    break;

                case TypeId.TypeNullableInt:
                    result = ReadNullable<int>(stream);
                    break;

                case TypeId.TypeNullableLong:
                    result = ReadNullable<long>(stream);
                    break;

                case TypeId.TypeNullableUshort:
                    result = ReadNullable<ushort>(stream);
                    break;

                case TypeId.TypeNullableUint:
                    result = ReadNullable<uint>(stream);
                    break;

                case TypeId.TypeNullableUlong:
                    result = ReadNullable<ulong>(stream);
                    break;

                case TypeId.TypeNullableFloat:
                    result = ReadNullable<float>(stream);
                    break;

                case TypeId.TypeNullableDouble:
                    result = ReadNullable<double>(stream);
                    break;

                case TypeId.TypeNullableDecimal:
                    result = ReadNullable<decimal>(stream);
                    break;

                case TypeId.TypeString:
                    result = ReadString(stream);
                    break;

                case TypeId.TypeByteArray:
                    result = ReadByteArray(stream);
                    break;

                case TypeId.TypeSerializable:
                    result = ReadSerializable(stream, baseType.ImplementationType);
                    break;

                case TypeId.TypeEnum:
                    result = ReadEnum(stream, baseType.ImplementationType);
                    break;

                default:
                    throw new CompactSerializationException(
                        $"Type {baseType.ImplementationType} is not supported by CompactSerializer.");
            }

            return result;
        }

        /// <summary>
        ///   Read a generic List from the stream.
        /// </summary>
        /// <param name = "stream">The stream where the list is read from.</param>
        /// <param name = "compactType">The compact type information of the list.</param>
        /// <returns>
        ///   The list read from the stream.
        /// </returns>
        /// <exception cref = "CompactSerializationException">
        ///   Thrown when the read operation for list of the base type defined in
        ///   <paramref name = "compactType" /> is not implemented yet.
        /// </exception>
        private static object ReadCollectionList(Stream stream, CompactType compactType)
        {
            object result;

            switch(compactType.BaseType.TypeId)
            {
                case TypeId.TypeBool:
                    result = ReadListBool(stream);
                    break;

                case TypeId.TypeByte:
                    result = ReadListByte(stream);
                    break;

                case TypeId.TypeChar:
                    result = ReadListChar(stream);
                    break;

                case TypeId.TypeShort:
                    result = ReadListShort(stream);
                    break;

                case TypeId.TypeInt:
                    result = ReadListInt(stream);
                    break;

                case TypeId.TypeLong:
                    result = ReadListLong(stream);
                    break;

                case TypeId.TypeUshort:
                    result = ReadListUshort(stream);
                    break;

                case TypeId.TypeUint:
                    result = ReadListUint(stream);
                    break;

                case TypeId.TypeUlong:
                    result = ReadListUlong(stream);
                    break;

                case TypeId.TypeFloat:
                    result = ReadListFloat(stream);
                    break;

                case TypeId.TypeDouble:
                    result = ReadListDouble(stream);
                    break;

                case TypeId.TypeDecimal:
                    result = ReadListDecimal(stream);
                    break;

                case TypeId.TypeNullableBool:
                    result = ReadListNullable<bool>(stream);
                    break;

                case TypeId.TypeNullableByte:
                    result = ReadListNullable<byte>(stream);
                    break;

                case TypeId.TypeNullableChar:
                    result = ReadListNullable<char>(stream);
                    break;

                case TypeId.TypeNullableShort:
                    result = ReadListNullable<short>(stream);
                    break;

                case TypeId.TypeNullableInt:
                    result = ReadListNullable<int>(stream);
                    break;

                case TypeId.TypeNullableLong:
                    result = ReadListNullable<long>(stream);
                    break;

                case TypeId.TypeNullableUshort:
                    result = ReadListNullable<ushort>(stream);
                    break;

                case TypeId.TypeNullableUint:
                    result = ReadListNullable<uint>(stream);
                    break;

                case TypeId.TypeNullableUlong:
                    result = ReadListNullable<ulong>(stream);
                    break;

                case TypeId.TypeNullableFloat:
                    result = ReadListNullable<float>(stream);
                    break;

                case TypeId.TypeNullableDouble:
                    result = ReadListNullable<double>(stream);
                    break;

                case TypeId.TypeNullableDecimal:
                    result = ReadListNullable<decimal>(stream);
                    break;

                case TypeId.TypeString:
                    result = ReadListString(stream);
                    break;

                case TypeId.TypeByteArray:
                    result = ReadListByteArray(stream);
                    break;

                case TypeId.TypeSerializable:
                    result = InvokeReadListSerializable(compactType.BaseType.ImplementationType, stream);
                    break;

                case TypeId.TypeEnum:
                    result = InvokeReadListEnum(compactType.BaseType.ImplementationType, stream);
                    break;

                default:
                    throw new CompactSerializationException(
                        $"Type {compactType.BaseType.ImplementationType} is not supported by CompactSerializer.");
            }

            return result;
        }

        #endregion
    }
}
