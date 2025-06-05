//-----------------------------------------------------------------------
// <copyright file = "CompactSerializer.Dictionary.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.CompactSerialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;

    /// <summary>
    /// This class provides the functions of reading and writing
    /// dictionaries. These functions can be used by other classes that
    /// implement the ICompactSerializable interface.
    /// </summary>
    public partial class CompactSerializer
    {
        #region Writing Dictionary

        /// <summary>
        /// Write a dictionary to the stream.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="dictionary">The dictionary to write.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
        /// <exception cref="IGT.Game.Core.CompactSerialization.CompactSerializationException">
        /// Thrown if <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not supported by CompactSerializer.
        /// </exception>
        public static void WriteDictionary<TKey, TValue>(Stream stream, Dictionary<TKey, TValue> dictionary)
        {
            WriteDictionary(stream, dictionary, typeof (TKey), typeof (TValue));
        }
        
        /// <summary>
        /// Write a read only dictionary to the stream.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="dictionary">The read-only dictionary to write.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
        /// <exception cref="IGT.Game.Core.CompactSerialization.CompactSerializationException">
        /// Thrown if <typeparamref name="TKey"/> or <typeparamref name="TValue"/> is not supported by CompactSerializer.
        /// </exception>
        public static void WriteDictionary<TKey, TValue>(Stream stream, IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            WriteDictionary(stream, (IDictionary)dictionary, typeof (TKey), typeof (TValue));
        }        

        /// <summary>
        /// Write a dictionary to the stream.
        /// </summary>
        /// <param name="keyType">The type of the keys in the dictionary.</param>
        /// <param name="valueType">The type of the values in the dictionary.</param>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="dictionary">The dictionary to write.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
        /// <exception cref="IGT.Game.Core.CompactSerialization.CompactSerializationException">
        /// Thrown if <paramref name="keyType"/> or <paramref name="valueType"/> is not supported by CompactSerializer.
        /// </exception>
        private static void WriteDictionary(Stream stream, IDictionary dictionary, Type keyType, Type valueType)
        {
            if(stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var compactType = GetCompactType(keyType);

            // Check if the collection's element type is supported.
            // No nested collection is allowed.
            if(compactType.BaseType.TypeId == TypeId.TypeNotSupported ||
               compactType.Collection != CollectionId.CollectionNone)
            {
                throw new CompactSerializationException(
                    $"Dictionary key element type {keyType} is not supported by CompactSerializer.");
            }

            compactType = GetCompactType(valueType);

            // Check if the collection's element type is supported.
            // No nested collection is allowed.
            if(compactType.BaseType.TypeId == TypeId.TypeNotSupported)
            {
                throw new CompactSerializationException(
                    $"Dictionary key element type {valueType} is not supported by CompactSerializer.");
            }
            
            if(dictionary == null)
            {
                // The dictionary is null
                Write(stream, true);
            }
            else
            {
                // The dictionary is not null.
                Write(stream, false);

                // This method of creating a IList is not supported by Mono 2.6.3 so i had to use ArrayList.
                var keyList = new ArrayList(dictionary.Keys);
                WriteList(stream, keyList, keyType);
                var valuesList = new ArrayList(dictionary.Values);
                WriteList(stream, valuesList, valueType);
            }
        }

        #endregion

        #region Reading Dictionary

        /// <summary>
        /// Read a dictionary from the stream.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="stream">The stream to read from.</param>
        /// <returns>The dictionary read from the stream.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
        /// <exception cref="IGT.Game.Core.CompactSerialization.CompactSerializationException">
        /// Thrown in the case that there is data inconsistency when reading the dictionary.
        /// </exception>
        public static Dictionary<TKey, TValue> ReadDictionary<TKey, TValue>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            Dictionary<TKey, TValue> result = null;

            if (!ReadBool(stream))
            {
                var keyList = (List<TKey>)ReadCollectionList(stream, GetCompactType(typeof(TKey)));
                var valueCompactType = GetCompactType(typeof (TValue));
                List<TValue> valueList;
                if(valueCompactType.Collection != CollectionId.CollectionNone)
                {
                    valueList = ReadNestedList<TValue>(stream);
                }
                else
                {
                    valueList = (List<TValue>)ReadCollectionList(stream, valueCompactType);
                }

                if (keyList.Count != valueList.Count)
                {
                    throw new CompactSerializationException("The key list and value list are not the same size.");
                }

                result = new Dictionary<TKey, TValue>();
                for(var index = 0; index < keyList.Count; index++)
                {
                    result.Add(keyList[index], valueList[index]);
                }
            }

            return result;
        }

        #endregion

        #region Invoking Generic Dictionary Read

        /// <summary>
        /// The cached reflection of the method <see cref="ReadDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <remarks>
        /// This static field is thread local, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static MethodInfo readDictionaryReflection;

        /// <summary>
        /// The dynamically emitted, specifically typed methods from the generic method <see cref="ReadDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <remarks>
        /// This static field is thread local, thus thread safe.
        /// </remarks>
        [ThreadStatic]
        private static Dictionary<Type, MethodInfo> methodReadDictionaryImpls;

        /// <summary>
        /// Invoke the method <see cref="ReadDictionary{TKey,TValue}"/> with the given type parameters.
        /// </summary>
        /// <param name="dictionaryType">The real type of the dictionary.</param>
        /// <param name="typeParameters">The generic type parameters of the dictionary.</param>
        /// <param name="arguments"> The arguments indicates the stream to read from.</param>
        /// <exception cref="CompactSerializationException">Thrown if there is a problem creating a dictionary or its
        /// read method reference during reflection, or if there is a problem with deserializing passed in data and arguments.</exception>
        /// <returns>The dictionary read from the stream.</returns>>
        private static object InvokeReadDictionary(Type dictionaryType, Type[] typeParameters, params object[] arguments)
        {
            methodReadDictionaryImpls = methodReadDictionaryImpls ?? 
                                        (methodReadDictionaryImpls = new Dictionary<Type, MethodInfo>());

            readDictionaryReflection = readDictionaryReflection ?? 
                                       typeof(CompactSerializer).GetMethod(nameof(ReadDictionary));
            
            if(readDictionaryReflection == null)
            {
                throw new CompactSerializationException($@"Error creating deserializer: {nameof(readDictionaryReflection)} is null.
                                                                  Ensure that the deserialization method exists.");
            }

            if(!methodReadDictionaryImpls.ContainsKey(dictionaryType))
            {
                try
                {
                    methodReadDictionaryImpls[dictionaryType] = readDictionaryReflection.MakeGenericMethod(typeParameters);
                }
                catch(Exception ex)
                {
                    throw new CompactSerializationException($@"Error creating deserialization read method for type {dictionaryType}
                                                                      with passed in parameters; {ex.Message}.");
                }
            }

            object readResult;
            try
            {
                readResult = methodReadDictionaryImpls[dictionaryType].Invoke(null, arguments);
            }
            catch(Exception ex)
            {
                throw new CompactSerializationException($@"Error deserializing with passed in arguments; {ex.Message}.");
            }

            return readResult;

        }

        #endregion
    }
}