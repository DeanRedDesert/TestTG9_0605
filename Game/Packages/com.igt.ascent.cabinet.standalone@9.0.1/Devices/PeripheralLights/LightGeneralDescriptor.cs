//-----------------------------------------------------------------------
// <copyright file = "LightGeneralDescriptor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;
    using IgtUsbDevice;

    /// <summary>
    /// This data structure stores general information on a light device.
    /// </summary>
    [Serializable]
    internal class LightGeneralDescriptor : IUnpackable
    {
        /// <summary>
        /// The hard coded total size of all fields in this data structure.
        /// </summary>
        private const int LocalSize = sizeof(ushort) + sizeof(ushort);

        /// <summary>
        /// Get the number of lights on the device.
        /// </summary>
        public ushort LightCount { get; private set; }

        /// <summary>
        /// Get the attributes of the light device.
        /// </summary>
        public ushort Attributes { get; private set; }

        /// <summary>
        /// Get the flag indicating whether the light device
        /// has RGB support.
        /// </summary>
        public bool IsRgb => (Attributes & (ushort)LightAttributes.RgbControl) != 0;

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return $"Light Count ({LightCount}) / Attributes (0x{Attributes:X4})";
        }

        #region IUnpackable Members

        /// <inheritdoc/>
        public virtual int DataSize => LocalSize;

        /// <inheritdoc/>
        public virtual void Unpack(byte[] buffer, int offset)
        {
            if(buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if(offset < 0 || offset >= buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(offset));
            }

            if(buffer.Length - offset < DataSize)
            {
                throw new InsufficientDataBufferException(
                    "Data buffer is not big enough to unpack LightGeneralDescriptor.");
            }

            LightCount = BitConverter.ToUInt16(buffer, offset);

            offset += sizeof(ushort);
            Attributes = BitConverter.ToUInt16(buffer, offset);
        }

        #endregion
    }
}
