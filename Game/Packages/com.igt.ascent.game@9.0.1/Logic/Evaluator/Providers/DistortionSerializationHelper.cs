//-----------------------------------------------------------------------
// <copyright file = "DistortionSerializationHelper.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Helper class to have more efficient serialization for DistortionPaytableProvider.
    /// If possible, types will be replaced by smaller types for serialization and deserialization.
    /// </summary>
    internal static class DistortionSerializationHelper
    {
        /// <summary>
        /// Serialize a list of int. 
        /// If possible, all items of the list will be serialized using byte or short.
        /// </summary>
        /// <param name="stream">Stream to serialize to.</param>
        /// <param name="list">The list be serialized.</param>
        public static void WriteList(System.IO.Stream stream, IList<int> list)
        {
            WriteList(stream, MinBytesPerItem(list), list);
        }

        /// <summary>
        /// Serialize a dictionary of int key and double value. 
        /// If possible, all items of the list will be serialized using smaller data types.
        /// </summary>
        /// <param name="stream">Stream to serialize to.</param>
        /// <param name="dictionary">The dictionary to be serialized.</param>
        public static void WriteDictionary(System.IO.Stream stream, IDictionary<int, double> dictionary)
        {
            var keys = dictionary.Select(element => element.Key).ToList();
            var values = dictionary.Select(element => element.Value).ToList();
            WriteList(stream, keys);
            WriteList(stream, MinBytesPerItem(values), values);
        }

        /// <summary>
        /// Deserialize a list of int that was compressed. 
        /// It takes into account if the original list has been serialized using byte or short.
        /// </summary>
        /// <param name="stream">Stream to serialize from.</param>
        public static List<int> ReadListInt(System.IO.Stream stream)
        {
            return ReadList<int>(stream);
        }

        /// <summary>
        /// Deserialize a dictionary that was compressed. 
        /// It takes into account if the original dictionary has been serialized using smaller data types.
        /// </summary>
        /// <param name="stream">Stream to serialize from.</param>
        /// <exception cref="System.Exception">
        /// Thrown if reading the dictionary gets corrupt data.
        /// </exception>
        public static Dictionary<int, double> ReadDictionary(System.IO.Stream stream)
        {
            var listKeys = ReadListInt(stream);
            var listValue = ReadList<double>(stream);
            var dictionary = new Dictionary<int, double>();

            if(listKeys.Count != listValue.Count)
            {
                throw new Exception("Error when reading a dictionary. Keys and Values have different sizes.");
            }

            for(int index = 0; index < listKeys.Count; ++index)
            {
                dictionary.Add(listKeys[index], listValue[index]);
            }
            return dictionary;
        }

        #region Private methods

        /// <summary>
        /// Generic method to serialize a list. 
        /// All items of the list will be serialized using the datatype defined by the bytesPerItem parameter.
        /// </summary>
        /// <typeparam name="T">The type of the list to be serialized.</typeparam>
        /// <param name="stream">Stream to serialize to.</param>
        /// <param name="bytesPerItem">The bytes per item the list shoudl be casted to.</param>
        /// <param name="list">The list to be serialized.</param>
        private static void WriteList<T>(System.IO.Stream stream, byte bytesPerItem, IList<T> list)
        {
            CompactSerializer.Write(stream, bytesPerItem);
            switch(bytesPerItem)
            {
                case sizeof(byte):
                    // ReSharper disable once PossibleNullReferenceException
                    CompactSerializer.WriteList(stream, list.Select(item => (byte)Convert.ChangeType(item, typeof(byte))).ToList());
                    break;

                case sizeof(ushort):
                    // ReSharper disable once PossibleNullReferenceException
                    CompactSerializer.WriteList(stream, list.Select(item => (ushort)Convert.ChangeType(item, typeof(ushort))).ToList());
                    break;

                case sizeof(int):
                    // ReSharper disable once PossibleNullReferenceException
                    CompactSerializer.WriteList(stream, list.Select(item => (int)Convert.ChangeType(item, typeof(int))).ToList());
                    break;

                case sizeof(double):
                    // ReSharper disable once PossibleNullReferenceException
                    CompactSerializer.WriteList(stream, list.Select(item => (double)Convert.ChangeType(item, typeof(double))).ToList());
                    break;

                case 0: // empty list.
                    break;

                default:
                    throw new Exception("Unknown data size when writing a list to a stream.");
            }
        }

        /// <summary>
        /// Deserialize a list of double that was compressed. 
        /// It takes into account if the original list has been serialized using short.
        /// </summary>
        /// <param name="stream">Stream to serialize from.</param>
        /// <exception cref="System.Exception">
        /// Thrown if reading the list gets corrupt data.
        /// </exception>
        private static List<T> ReadList<T>(System.IO.Stream stream)
        {
            byte bytesPerItem = CompactSerializer.ReadByte(stream);
            List<T> newList;
            switch(bytesPerItem)
            {
                case sizeof(byte):
                    {
                        var miniStrip = new List<byte>();
                        newList = new List<T>();
                        miniStrip.AddRange(CompactSerializer.ReadListByte(stream));
                        miniStrip.ForEach(element => newList.Add((T)Convert.ChangeType(element, typeof(T))));
                    }
                    break;

                case sizeof(ushort):
                    {
                        var miniStrip = new List<ushort>();
                        newList = new List<T>();
                        miniStrip.AddRange(CompactSerializer.ReadListUshort(stream));
                        miniStrip.ForEach(element => newList.Add((T)Convert.ChangeType(element, typeof(T))));
                    }
                    break;

                case sizeof(int):
                    {
                        var miniStrip = new List<int>();
                        newList = new List<T>();
                        miniStrip.AddRange(CompactSerializer.ReadListInt(stream));
                        miniStrip.ForEach(element => newList.Add((T)Convert.ChangeType(element, typeof(T))));
                    }
                    break;

                case sizeof(double):
                    {
                        var miniStrip = new List<double>();
                        newList = new List<T>();
                        miniStrip.AddRange(CompactSerializer.ReadListDouble(stream));
                        miniStrip.ForEach(element => newList.Add((T)Convert.ChangeType(element, typeof(T))));
                    }
                    break;

                case 0:// an empty list.
                    newList = new List<T>();
                    break;

                default:
                    throw new Exception("Unknown data size when reading a list from a stream.");
            }
            return newList;
        }

        /// <summary>
        /// Defines the number of bytes per item all items of this list of int can be reduced to.
        /// </summary>
        /// <param name="list">List of int.</param>
        /// <returns>Number of bytes of one item that would be sufficient.</returns>
        private static byte MinBytesPerItem(IList<int> list)
        {
            int bytesPerItem = 0;
            if(list.Any())
            {
                int min = list.Min();
                int max = list.Max();
                bytesPerItem = min < 0 || max > ushort.MaxValue 
                    ? sizeof(int) 
                    : max > byte.MaxValue ? sizeof(ushort) : sizeof(byte);
            }
            return (byte)bytesPerItem;
        }

        /// <summary>
        /// Defines the number of bytes per item all items of this list of double can be reduced to.
        /// </summary>
        /// <param name="list">List of int.</param>
        /// <returns>Number of bytes of one item that would be sufficient without loss of precision.</returns>
        private static byte MinBytesPerItem(IList<double> list)
        {
            int bytesPerItem = list.Any(val => Math.Abs(val - (int)val) > double.Epsilon || val > int.MaxValue || val < int.MinValue) 
                    ? sizeof(double) 
                    : MinBytesPerItem(list.Select(val => (int)val).ToList());

            return (byte)bytesPerItem;
        }

        #endregion
    }
}
