//-----------------------------------------------------------------------
// <copyright file = "StreamingLightHardwareMismatchException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;

    /// <summary>
    /// Thrown if the streaming lights hardware is not the type that was expected.
    /// </summary>
    [Serializable]
    public class StreamingLightHardwareMismatchException : Exception
    {
        private const string MessageFormat = "{0}{1}Expected Hardware Device: {2}{1}Actual Hardware Device: {3}";

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="expected">The hardware device expected.</param>
        /// <param name="actual">The actual hardware device.</param>
        public StreamingLightHardwareMismatchException(string message, Hardware expected,
            Hardware actual)
            : this(message, expected, actual, null)
        {

        }

        /// <summary>
        /// Construct a new instance.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="expected">The hardware device expected.</param>
        /// <param name="actual">The actual hardware device.</param>
        /// <param name="innerException">The inner exception.</param>
        public StreamingLightHardwareMismatchException(string message, Hardware expected,
            Hardware actual, Exception innerException)
            : base(string.Format(MessageFormat, message, Environment.NewLine, expected, actual), innerException)
        {
            ExpectedHardware = expected;
            ActualHardware = actual;
        }

        /// <summary>
        /// The expected hardware device.
        /// </summary>
        public Hardware ExpectedHardware
        {
            get;
            private set;
        }

        /// <summary>
        /// The actual hardware device.
        /// </summary>
        public Hardware ActualHardware
        {
            get;
            private set;
        }
    }
}
