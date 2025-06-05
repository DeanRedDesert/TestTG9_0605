//-----------------------------------------------------------------------
// <copyright file = "DataItems.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.CommunicationLib
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Permissions;
    using CompactSerialization;

    /// <summary>
    ///   List of data that specified during negotiation for a state.
    ///   Key: is the service provider name the data is located in.
    ///   Value: is a list of data that belongs to provider in key.
    ///   Key: is the result of a ServiceAccessor identifier property
    ///   that is used to uniquely identify the service.
    ///   Value: is the actual data
    /// </summary>
    [Serializable]
    public class DataItems : Dictionary<string, Dictionary<int, object>>, ICompactSerializable
    {
        /// <summary>
        /// The key to use to retrieve the history data dictionary from the data items.
        /// </summary>
        private const string HistoryDataKey = "History";

        /// <summary>
        /// Gets/sets the History data. This property will return null if no history data is present.
        /// </summary>
        public Dictionary<int, object> HistoryData
        {
            get { return ContainsKey(HistoryDataKey) ? this[HistoryDataKey] : null; }
            set { this[HistoryDataKey] = value; }
        }

        /// <summary>
        ///   Create an instance of the DataItems class.
        /// </summary>
        public DataItems()
        {
        }

        /// <summary>
        ///   Serialization constructor, deserialize DataItems.
        /// </summary>
        /// <param name = "info">Serialization info to construct the object from.</param>
        /// <param name = "context">Serialization context to use.</param>
        /// <DevDoc>
        ///   Mono and Microsoft's implementation of binary dictionary serialization is not
        ///   compatible. Therefore if a type implements dictionary and it needs to be
        ///   serialized and sent between the different .net implementations then the
        ///   serialization routines must be overridden.
        /// </DevDoc>
        protected DataItems(SerializationInfo info, StreamingContext context)
        {
            // Retrieve lists used to store dictionaries.
            var keyList = (List<string>) info.GetValue("KeyList", typeof(List<string>));
            var dictionaryKey = (List<List<int>>) info.GetValue("DictionaryKey", typeof(List<List<int>>));
            var dictionaryValue = (List<List<object>>) info.GetValue("DictionaryValue", typeof(List<List<object>>));

            // Populate Dictionaries from lists.
            for(var outerDictCount = 0;
                outerDictCount < dictionaryKey.Count && outerDictCount < dictionaryValue.Count &&
                outerDictCount < keyList.Count;
                outerDictCount++)
            {
                if(dictionaryKey[outerDictCount] == null || dictionaryValue[outerDictCount] == null)
                {
                    Add(keyList[outerDictCount], null);
                }
                else
                {
                    var dictionary = new Dictionary<int, object>();
                    for(var innerDictCount = 0;
                        innerDictCount < dictionaryKey[outerDictCount].Count &&
                        innerDictCount < dictionaryValue[outerDictCount].Count;
                        innerDictCount++)
                    {
                        dictionary.Add(dictionaryKey[outerDictCount][innerDictCount],
                                       dictionaryValue[outerDictCount][innerDictCount]);
                    }
                    Add(keyList[outerDictCount], dictionary);
                }
            }
        }

        /// <summary>
        /// Calculates the difference between the new items and the items in this instance.
        /// </summary>
        /// <param name="newItems">The <see cref="DataItems"/> containing the new data.</param>
        /// <returns>
        /// A new <see cref="DataItems"/> containing the items that appear in the new data but not in this instance,
        /// as well as any updated values.
        /// </returns>
        /// <remarks>
        /// The <see cref="DiffWith(DataItems)"/> operation allows the calculation of the set of values that have been added
        /// or changed in <paramref name="newItems"/> using this instance as the base of the comparison. This allows one to store a
        /// sequence of updates that, when applied in order, can be used to reconstruct a snapshot of the data at a
        /// particular point in time (i.e. when the diff was calculated for the latest update.)
        /// 
        /// This operation does not calculate removals, so it is only possible to accumulate data when reconstructing
        /// a snapshot.
        /// </remarks>
        public DataItems DiffWith(DataItems newItems)
        {
            var result = new DataItems();
            foreach(var provider in newItems)
            {
                if(!ContainsKey(provider.Key))
                {
                    result.Add(provider.Key, new Dictionary<int, object>(provider.Value));
                }
                else
                {
                    var newOrUpdatedEntries = new Dictionary<int, object>();
                    var existingProvider = this[provider.Key];
                    foreach(var kvp in provider.Value)
                    {
                        if(!existingProvider.ContainsKey(kvp.Key) || existingProvider[kvp.Key] != kvp.Value)
                        {
                            newOrUpdatedEntries.Add(kvp.Key, kvp.Value);
                        }
                    }
                    if(newOrUpdatedEntries.Count > 0)
                    {
                        result.Add(provider.Key, newOrUpdatedEntries);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Merge the specific data item to this object.
        /// </summary>
        /// <param name="dataToMerge">DataItems to merge.</param>
        public void Merge(DataItems dataToMerge)
        {
            // Check reference equality to avoid changing the collection by iterators in the case
            // of A.Merge(A);
            if(dataToMerge == null || this == dataToMerge)
            {
                return;
            }

            foreach(var provider in dataToMerge.Keys)
            {
                if(!ContainsKey(provider))
                {
                    this[provider] = new Dictionary<int, object>(dataToMerge[provider]);
                }
                // Check reference equality to avoid changing the collection by iterators,
                // in the case of
                //     A.Merge(B);
                //     A.Merge(B);
                else if(this[provider] != dataToMerge[provider])
                {
                    foreach(var identifier in dataToMerge[provider].Keys)
                    {
                        this[provider][identifier] = dataToMerge[provider][identifier];
                    }
                }
            }
        }

        /// <summary>
        /// Merge the specific serialized data item to this object.
        /// </summary>
        /// <param name="dataToMerge">Data to merge to this object.
        ///   Key: is the service provider name the data is located in.
        ///   Value: is a Dictionary of data that belongs to provider in key.
        ///   Key: is the result of a ServiceAccessor identifier property
        ///   that is used to uniquely identify the service.
        ///   Value: is the binary serialized service value.
        /// </param>
        public void MergeSerialized(Dictionary<string, Dictionary<int, byte[]>> dataToMerge)
        {
            if(dataToMerge == null)
            {
                return;
            }

            foreach(var pair in dataToMerge)
            {
                if(!ContainsKey(pair.Key))
                {
                    Add(pair.Key, new Dictionary<int, object>());
                }
                foreach(var bytesPair in pair.Value)
                {
                    if(bytesPair.Value == null)
                    {
                        this[pair.Key][bytesPair.Key] = null;
                    }
                    else
                    {
                        using(var stream = new MemoryStream(bytesPair.Value))
                        {
                            var formatter = new BinaryFormatter();
                            this[pair.Key][bytesPair.Key] = formatter.Deserialize(stream);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///   ISerializable implementation of GetObjectData, serialize DataItems.
        /// </summary>
        /// <param name = "info">Serialization info to construct the object from.</param>
        /// <param name = "context">Serialization context to use.</param>
        /// <DevDoc>
        ///   Mono and Microsoft's implementation of binary dictionary serialization is not
        ///   compatible. Therefore if a type implements dictionary and it needs to be
        ///   serialized and sent between the different .net implementations then the
        ///   serialization routines must be overridden.
        /// </DevDoc>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            // Create Lists to store Dictionary Data.
            // Outer Dictionary Keys.
            var keyList = new List<string>();
            // Inner Dictionary Keys.
            var dictionaryKey = new List<List<int>>();
            // Inner Dictionary Values.
            var dictionaryValue = new List<List<object>>();

            var dictionaryCount = 0;
            foreach(var pair in this)
            {
                //Add String key
                keyList.Add(pair.Key);

                // Create inner dictionary lists
                if(pair.Value == null)
                {
                    dictionaryKey.Add(null);
                    dictionaryValue.Add(null);
                }
                else
                {
                    //Create an empty list for the inner dictionary key and value.
                    dictionaryKey.Add(new List<int>());
                    dictionaryValue.Add(new List<object>());

                    // Populate inner dictionary.
                    foreach(var dictionary in pair.Value)
                    {
                        dictionaryKey[dictionaryCount].Add(dictionary.Key);
                        dictionaryValue[dictionaryCount].Add(dictionary.Value);
                    }
                }
                dictionaryCount++;
            }

            // Add created lists to serialization info.
            info.AddValue("KeyList", keyList, typeof(List<string>));
            info.AddValue("DictionaryKey", dictionaryKey, typeof(List<List<int>>));
            info.AddValue("DictionaryValue", dictionaryValue, typeof(List<List<object>>));
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();

            // Write the count of the outer dictionary.
            CompactSerializer.Write(stream, Count);

            // Write the entries of the outer dictionary.
            foreach(var entry in this)
            {
                CompactSerializer.Write(stream, entry.Key);

                if(entry.Value == null)
                {
                    // The inner dictionary is null.
                    CompactSerializer.Write(stream, true);
                }
                else
                {
                    // The inner dictionary is not null.
                    CompactSerializer.Write(stream, false);

                    var innerDictionary = entry.Value;

                    // Write the count of the inner dictionary.
                    CompactSerializer.Write(stream, innerDictionary.Count);

                    // Iterate and create a list of used types.  Fully qualified names are long so we shouldn't store them
                    // every time we encounter them.
                    var typeList = (from pair in innerDictionary
                                    where pair.Value != null && CompactSerializer.Supports(pair.Value.GetType())
                                    select pair.Value.GetType().AssemblyQualifiedName).Distinct().ToList();

                    CompactSerializer.WriteList(stream, typeList);

                    // Write the entries of the inner dictionary.
                    foreach(var pair in innerDictionary)
                    {
                        CompactSerializer.Write(stream, pair.Key);

                        if(pair.Value == null)
                        {
                            // The object is null.
                            CompactSerializer.Write(stream, true);
                        }
                        else
                        {
                            // The object is not null.
                            CompactSerializer.Write(stream, false);
                            var dataType = pair.Value.GetType();

                            if(CompactSerializer.Supports(dataType))
                            {
                                CompactSerializer.Write(stream, true);
                                CompactSerializer.Write(stream, typeList.IndexOf(dataType.AssemblyQualifiedName));
                                CompactSerializer.Serialize(stream, pair.Value);
                            }
                            else
                            {
                                CompactSerializer.Write(stream, false);
                                #if DEBUG
                                Logging.Log.WriteWarning(
                                    string.Format(
                                        "Unable to compact serialize {0} in data items. Consider implementing ICompactSerializable on the type.",
                                        dataType.FullName));
                                #endif
                                // Use Binary Formatter to serialize object types.
                                binaryFormatter.Serialize(stream, pair.Value);
                            }
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            var binaryFormatter = new BinaryFormatter();

            Clear();

            // Read the count of the outer dictionary.
            var outerCount = CompactSerializer.ReadInt(stream);

            // Read the entries of the outer dictionary.
            for(var i = 0; i < outerCount; i++)
            {
                var entryKey = CompactSerializer.ReadString(stream);
                Dictionary<int, object> innerDictionary = null;

                // The inner dictionary is not null.)
                if(!CompactSerializer.ReadBool(stream))
                {
                    // Read the count of the inner dictionary.
                    var innerCount = CompactSerializer.ReadInt(stream);

                    innerDictionary = new Dictionary<int, object>(innerCount);
                    var typeList = CompactSerializer.ReadListString(stream);

                    // Read the entries of the inner dictionary.
                    for(var j = 0; j < innerCount; j++)
                    {
                        var pairKey = CompactSerializer.ReadInt(stream);
                        object pairValue = null;

                        // The object is not null.
                        if(!CompactSerializer.ReadBool(stream))
                        {
                            if(CompactSerializer.ReadBool(stream))
                            {
                                var typeIndex = CompactSerializer.ReadInt(stream);

                            #if UNITY_WEBGL
                                var dataType = Assembly.Load("Assembly-CSharp").GetType(typeList[typeIndex]);
                            #else
                                var dataType = Type.GetType(typeList[typeIndex]);
                            #endif
                                pairValue = CompactSerializer.Deserialize(stream, dataType);
                            }
                            else
                            {
                                // Use Binary Formatter to deserialize object types.
                                pairValue = binaryFormatter.Deserialize(stream);
                            }
                        }

                        innerDictionary.Add(pairKey, pairValue);
                    }
                }

                Add(entryKey, innerDictionary);
            }
        }

        #endregion
    }
}
