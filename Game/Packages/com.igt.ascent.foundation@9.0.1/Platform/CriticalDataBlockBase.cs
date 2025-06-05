// -----------------------------------------------------------------------
// <copyright file = "CriticalDataBlockBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.Serialization.Formatters.Binary;
    using Game.Core.CompactSerialization;
    using Interfaces;

    /// <summary>
    /// Base class for all critical data block classes.
    /// It contains the commonly used serialization methods.
    /// </summary>
    public abstract class CriticalDataBlockBase
    {
        #region Private Fields

        /// <summary>
        /// The flag that indicates whether or not it is compressed critical data.
        /// </summary>
        private readonly bool isDataCompressed;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates an empty instance of <see cref="CriticalDataBlockBase"/>.
        /// </summary>
        /// <param name="isDataCompressed">
        /// The flag indicating whether it is compressed critical data block.
        /// This parameter is optional.  If not specified, it defaults to false.
        /// </param>
        protected CriticalDataBlockBase(bool isDataCompressed = false)
        {
            this.isDataCompressed = isDataCompressed;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the human readable string that represents the data.
        /// </summary>
        /// <param name="name">
        /// The key of the critical data.
        /// </param>
        /// <returns>
        /// The rep string of the data, if available. Null if the rep string is not available or
        /// the feature is not supported by the critical data block.
        /// </returns>
        internal virtual string GetRepString(CriticalDataName name)
        {
            return null;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// The method reads data from the serialized stream data, and if the data block is compressed then before read
        /// it should be decompressed.
        /// </summary>
        /// <typeparam name="T">The type of the data to be deserialized.</typeparam>
        /// <param name="data">The serialized stream data to be deserialized.</param>
        /// <returns>The data after deserialization.</returns>
        protected T DeserializeCriticalData<T>(byte[] data)
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
        /// The method writes data to the serialized stream data, and if the data block is compressed then before write
        /// it should be compressed.
        /// </summary>
        /// <typeparam name="T">The type of the data to be deserialized.</typeparam>
        /// <param name="data">The serialized stream data to be deserialized.</param>
        /// <returns>The data after deserialization.</returns>
        protected byte[] SerializeCriticalData<T>(T data)
        {
            using(var memoryStream = new MemoryStream())
            {
                if(isDataCompressed)
                {
                    using(var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
                    {
                        WriteDataToStream(gZipStream, data);
                    }
                }
                else
                {
                    WriteDataToStream(memoryStream, data);
                }
                return memoryStream.GetBuffer();
            }
        }

        #endregion

        #region Private Methods

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

        /// <summary>
        /// Writes data to the stream as the specified type.
        /// </summary>
        /// <param name="stream">Stream to write data to.</param>
        /// <param name="data">Data to write.</param>
        private static void WriteDataToStream<T>(Stream stream, T data)
        {
            if(CompactSerializer.Supports(typeof(T)))
            {
                CompactSerializer.Serialize(stream, data);
            }
            else
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(stream, data);
            }
        }

        #endregion
    }
}