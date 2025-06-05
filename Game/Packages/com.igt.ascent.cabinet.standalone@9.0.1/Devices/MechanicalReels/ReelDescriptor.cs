//-----------------------------------------------------------------------
// <copyright file = "ReelDescriptor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;
    using IgtUsbDevice;

    /// <summary>
    /// This data structure stores information on a reel.
    /// </summary>
    [Serializable]
    internal class ReelDescriptor : IUnpackable
    {
        /// <summary>
        /// The hard coded total size of all fields in this data structure.
        /// </summary>
        private const int LocalSize = 2;

        /// <summary>
        /// The number of stops the reel has.
        /// </summary>
        public byte NumberOfStops { get; private set; }

        /// <summary>
        /// The maximum time, in seconds, the motor will take to accelerate, find the desired position,
        /// and decelerate to a stop if issued a spin command with no "extra" movement.
        /// </summary>
        public byte MaximumSeekTime { get; private set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Number of Stops ({NumberOfStops}) / Max Seek Time ({MaximumSeekTime} seconds))";
        }

        #region IUnpackable Members

        /// <inheritdoc/>
        public int DataSize => LocalSize;

        /// <inheritdoc/>
        public void Unpack(byte[] buffer, int offset)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if(offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if(buffer.Length - offset < LocalSize)
            {
                throw new InsufficientDataBufferException(
                    "Data buffer is not big enough to unpack ReelDescriptor.");
            }

            NumberOfStops = buffer[offset];
            MaximumSeekTime = buffer[offset + 1];
        }

        #endregion
    }
}
