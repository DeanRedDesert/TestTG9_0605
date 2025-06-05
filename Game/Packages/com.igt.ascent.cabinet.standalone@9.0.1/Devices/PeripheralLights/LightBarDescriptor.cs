//-----------------------------------------------------------------------
// <copyright file = "LightBarDescriptor.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.PeripheralLights
{
    using System;
    using System.Linq;
    using System.Text;
    using IgtUsbDevice;

    /// <summary>
    /// This data structure stores information on a light bar device.
    /// </summary>
    [Serializable]
    internal class LightBarDescriptor : LightGeneralDescriptor
    {
        /// <summary>
        /// Get the number of light bars.
        /// </summary>
        public byte BarCount { get; private set; }

        /// <summary>
        /// Get the the number of lights for each light bar.
        /// </summary>
        public byte[] LightCountPerBars { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="LightBarDescriptor"/>
        /// with default values.
        /// </summary>
        public LightBarDescriptor()
        {
            LightCountPerBars = new byte[0];
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("Light Bar Descriptor -");
            builder.AppendLine("\t" + base.ToString());
            builder.AppendLine("\tNumber of Bars = " + LightCountPerBars.Length);
            builder.Append("\tLight Count Per Bars:\t");
            foreach(var lightCountPerBar in LightCountPerBars)
            {
                builder.Append(" " + lightCountPerBar);
            }

            return builder.ToString();
        }

        #region IUnpackable Overrides

        /// <inheritdoc/>
        public override int DataSize => base.DataSize + 1 + LightCountPerBars.Length;

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

            base.Unpack(buffer, offset);

            offset += base.DataSize;

            byte[] lightCountPerBars = null;

            if(buffer.Length - offset >= 1)
            {
                BarCount = buffer[offset];
                offset++;

                if(buffer.Length - offset >= BarCount)
                {
                    lightCountPerBars = new byte[BarCount];

                    Array.Copy(buffer, offset, lightCountPerBars, 0, BarCount);

                    var totalLightCount = lightCountPerBars.Sum(lightCount => (int)lightCount);
                    if(LightCount != totalLightCount)
                    {
                        throw new InvalidUsbDeviceDataException(
                            $"Light count {LightCount} does not match the sum of light counts in all bars {totalLightCount}.");
                    }
                }
            }

            LightCountPerBars = lightCountPerBars ?? throw new InsufficientDataBufferException(
                                    "Data buffer is not big enough to unpack LightBarDescriptor.");
        }

        #endregion
    }
}
