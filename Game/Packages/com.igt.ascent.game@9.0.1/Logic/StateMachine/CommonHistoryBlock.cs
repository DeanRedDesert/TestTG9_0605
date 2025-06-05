//-----------------------------------------------------------------------
// <copyright file = "CommonHistoryBlock.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine
{
    using System;
    using System.IO;
    using Communication.CommunicationLib;
    using CompactSerialization;

    /// <summary>
    /// Class for storing common history information.
    /// </summary>
    [Serializable]
    public class CommonHistoryBlock : ICompactSerializable
    {
        /// <summary>
        /// The state name for the history block.
        /// </summary>
        public string StateName { private set; get; }

        /// <summary>
        /// The data to save for history.
        /// </summary>
        public DataItems Data { private set; get; }

        /// <summary>
        /// The data to save for the history of a bonus extension.
        /// </summary>
        public byte[] BonusExtensionData { private set; get; }

        /// <summary>
        /// Create a history block with game data only.
        /// </summary>
        /// <param name="stateName">The state the data is for.</param>
        /// <param name="data">The data to store for history.</param>
        public CommonHistoryBlock(string stateName, DataItems data)
            : this(stateName, data, null)
        {
        }

        /// <summary>
        /// Create a history block with both game data and bonus extension data.
        /// </summary>
        /// <param name="stateName">The state the data is for.</param>
        /// <param name="data">The data to store for history.</param>
        /// <param name="bonusExtensionData">The data to store for the history of a bonus extension.</param>
        public CommonHistoryBlock(string stateName, DataItems data, byte[] bonusExtensionData)
        {
            StateName = stateName;
            Data = data;
            BonusExtensionData = bonusExtensionData;
        }

        /// <summary>
        /// Construct the common history block with the byte array data.
        /// </summary>
        /// <param name="byteData">
        /// The byte array which was serialized from the <see cref="CommonHistoryBlock"/> object.
        /// </param>
        /// <exception cref="CompactSerializationException">
        /// This exception is thrown if failed to construct the <see cref="CommonHistoryBlock"/> object 
        /// with the specified byte array. 
        /// </exception>
        public CommonHistoryBlock(byte[] byteData)
        {
            try
            {
                using(var memStream = new MemoryStream(byteData))
                {
                    Deserialize(memStream);
                }
            }
            catch(Exception exception)
            {
                throw new CompactSerializationException("Failed to construct the common history block with the " +
                                         "specified byte array. One of the causes could be that the byte array " +
                                         "was not serialized from a common history block object.", exception);
            }
        }

        /// <summary>
        /// Return a byte array that is serialized from this common history block.
        /// </summary>
        /// <returns>The byte array which was serialized from the <see cref="CommonHistoryBlock"/> object.</returns>
        public byte[] ToBytes()
        {
            using(var memStream = new MemoryStream())
            {
                Serialize(memStream);
                return memStream.ToArray();
            }
        }

        #region ICompactSerializable Members

        /// <summary>
        /// This parameter-less constructor is required by ICompactSerializable
        /// interface, and should not be used for any purpose other than
        /// deserialization.
        /// </summary>
        public CommonHistoryBlock()
        {
        }

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, StateName);
            CompactSerializer.Write(stream, Data);
            CompactSerializer.Write(stream, BonusExtensionData);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            StateName = CompactSerializer.ReadString(stream);
            Data = CompactSerializer.ReadSerializable<DataItems>(stream);
            BonusExtensionData = CompactSerializer.ReadByteArray(stream);
        }

        #endregion
    }
}
