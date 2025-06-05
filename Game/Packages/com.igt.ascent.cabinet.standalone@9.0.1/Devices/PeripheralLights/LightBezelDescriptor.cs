//-----------------------------------------------------------------------
// <copyright file = "LightBezelDescriptor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;
    using System.Text;
    using IgtUsbDevice;

    /// <summary>
    /// This data structure stores information on a light bezel device.
    /// </summary>
    [Serializable]
    internal class LightBezelDescriptor : LightGeneralDescriptor
    {
        /// <summary>
        /// The hard coded total size of all fields in this data structure.
        /// </summary>
        private const int LocalSize = 4;

        /// <summary>
        /// Get the number of lights on the top.
        /// </summary>
        public byte TopLightCount { get; private set; }

        /// <summary>
        /// Get the number of lights on the bottom.
        /// </summary>
        public byte BottomLightCount { get; private set; }

        /// <summary>
        /// Get the number of lights on the left.
        /// </summary>
        public byte LeftLightCount { get; private set; }

        /// <summary>
        /// Get the number of lights on the right.
        /// </summary>
        public byte RightLightCount { get; private set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Light Bezel Descriptor -");
            builder.AppendLine("\t" + base.ToString());
            builder.AppendLine("\tTop Light Count:\t" + TopLightCount);
            builder.AppendLine("\tBottom Light Count:\t" + BottomLightCount);
            builder.AppendLine("\tLeft Light Count:\t" + LeftLightCount);
            builder.AppendLine("\tRight Light Count:\t" + RightLightCount);

            return builder.ToString();
        }

        #region IUnpackable Overrides

        /// <inheritdoc/>
        public override int DataSize => base.DataSize + LocalSize;

        /// <inheritdoc/>
        /// <exception cref="InvalidUsbDeviceDataException">
        /// Thrown when an error occurred when <paramref name="buffer"/>
        /// contains invalid data.
        /// </exception>
        public override void Unpack(byte[] buffer, int offset)
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
                    "Data buffer is not big enough to unpack LightBezelDescriptor.");
            }

            base.Unpack(buffer, offset);

            offset += base.DataSize;

            TopLightCount = buffer[offset];
            BottomLightCount = buffer[offset + 1];
            LeftLightCount = buffer[offset + 2];
            RightLightCount = buffer[offset + 3];

            var totalLightCount = TopLightCount + BottomLightCount + LeftLightCount + RightLightCount;
            if(LightCount != totalLightCount)
            {
                throw new InvalidUsbDeviceDataException(
                    $"Light count {LightCount} does not match the sum of light counts in all bezels {totalLightCount}.");
            }
        }

        #endregion
    }
}
