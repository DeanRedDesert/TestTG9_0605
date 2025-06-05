//-----------------------------------------------------------------------
// <copyright file = "ReelStop.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    using System;
    using System.IO;
    using CompactSerialization;

    /// <summary>
    /// Class which encapsulates a reel and its stop.
    /// </summary>
    [Serializable]
    public class ReelStop : ICompactSerializable
    {
        /// <summary>
        /// Gets/Sets the reel to stop.
        /// </summary>
        public byte ReelNumber { get; set; }

        /// <summary>
        /// The stop at which to stop the reel.
        /// </summary>
        public byte Stop { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Reel {ReelNumber} Stop {Stop}";
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        void ICompactSerializable.Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, ReelNumber);
            CompactSerializer.Write(stream, Stop);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(Stream stream)
        {
            ReelNumber = CompactSerializer.ReadByte(stream);
            Stop = CompactSerializer.ReadByte(stream);
        }

        #endregion
    }
}
