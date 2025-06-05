// -----------------------------------------------------------------------
// <copyright file = "CriticalDataChunk.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization.Formatters.Binary;
    using Ascent.Communication.Platform.Interfaces;
    using CompactSerialization;

    /// <summary>
    /// Implementation of <see cref="ICriticalDataChunk"/> to provide the APIs for
    /// accessing one piece of critical data at a time.
    /// </summary>
    /// <remarks>
    /// It maintains a collection of multiple pieces of critical data to support
    /// accessing a specified piece of data as needed.
    /// </remarks>
    internal class CriticalDataChunk : ICriticalDataChunk
    {
        #region Private Fields

        /// <summary>
        /// The table of critical data indexed by selector.
        /// </summary>
        private readonly IDictionary<CriticalDataSelector, byte[]> criticalDataItems;

        /// <summary>
        /// The flag that indicates whether is reading compressed critical data.
        /// </summary>
        private readonly bool isDataCompressed;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates an instance of <see cref="CriticalDataChunk"/>.
        /// </summary>
        /// <param name="criticalDataItems">The critical data.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="criticalDataItems"/> is null.
        /// </exception>
        public CriticalDataChunk(IDictionary<CriticalDataSelector, byte[]> criticalDataItems)
        {
            if(criticalDataItems == null)
            {
                throw new ArgumentNullException("criticalDataItems");
            }

            this.criticalDataItems = criticalDataItems;
        }

        /// <summary>
        /// Instantiates an instance of <see cref="CriticalDataChunk"/>.
        /// </summary>
        /// <param name="criticalDataItems">The critical data.</param>
        /// <param name="isDataCompressed">The flag indicating whether is reading compressed critical data.</param>
        public CriticalDataChunk(IDictionary<CriticalDataSelector, byte[]> criticalDataItems, bool isDataCompressed)
            : this(criticalDataItems)
        {
            this.isDataCompressed = isDataCompressed;
        }

        #endregion

        #region ICriticalDataChunk Members

        /// <inheritdoc />
        public T RetrieveCriticalData<T>(CriticalDataSelector criticalDataSelector)
        {
            if(criticalDataSelector == null)
            {
                throw new ArgumentNullException("criticalDataSelector");
            }

            if(!ContainsSelector(criticalDataSelector))
            {
                throw new ArgumentException(criticalDataSelector + " does not exist.");
            }

            return DeserializeCriticalData<T>(criticalDataItems[criticalDataSelector]);
        }

        /// <inheritdoc />
        public bool ContainsSelector(CriticalDataSelector criticalDataSelector)
        {
            if(criticalDataSelector == null)
            {
                throw new ArgumentNullException("criticalDataSelector");
            }

            return criticalDataItems.ContainsKey(criticalDataSelector);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// The method reads data from stream, and if the data is compressed then before read
        /// it should be decompressed.
        /// </summary>
        /// <typeparam name="T">The type of the data to be de-serialized.</typeparam>
        /// <param name="data">The data to be de-serialized.</param>
        /// <returns>The data after de-serialization.</returns>
        private T DeserializeCriticalData<T>(byte[] data)
        {
            var deserializationResult = default(T);

            if(data != null && data.Length > 0)
            {
                using(var memoryStream = new MemoryStream(data))
                {
                    if(isDataCompressed)
                    {
                        using(var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                        {
                            deserializationResult = ReadDataFromStream<T>(gZipStream);
                        }
                    }
                    else
                    {
                        deserializationResult = ReadDataFromStream<T>(memoryStream);
                    }
                }
            }

            return deserializationResult;
        }

        /// <summary>
        /// Reads data from the stream as the specified type.
        /// </summary>
        /// <param name="stream">Stream to read data from.</param>
        /// <returns>
        /// The data read from the stream.
        /// </returns>
        private static T ReadDataFromStream<T>(Stream stream)
        {
            T returnData;
            if(CompactSerializer.Supports(typeof(T)))
            {
                returnData = CompactSerializer.Deserialize<T>(stream);
            }
            else
            {
                var binaryFormatter = new BinaryFormatter();
                returnData = (T)binaryFormatter.Deserialize(stream);
            }
            return returnData;
        }

        #endregion
    }
}