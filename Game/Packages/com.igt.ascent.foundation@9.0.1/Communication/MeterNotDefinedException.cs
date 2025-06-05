//-----------------------------------------------------------------------
// <copyright file = "MeterNotDefinedException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.Globalization;

    /// <summary>
    /// This exception indicates that a meter being accessed
    /// is not defined.
    /// </summary>
    [Serializable]
    public class MeterNotDefinedException : Exception
    {
        /// <summary>
        /// The scope of the meter being accessed when the exception is thrown.
        /// </summary>
        public string ScopeName { get; private set; }

        /// <summary>
        /// The id of the meter being accessed when the exception is thrown.
        /// </summary>
        public string MeterId { get; private set; }

        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string messageFormat = "Meter {0} in the scope {1} is not defined";

        /// <summary>
        /// Construct an MeterNotDefinedException with a general message.
        /// </summary>
        /// <param name="meterScope">The scope of the meter.</param>
        /// <param name="meterId">A unique string identifier used to locate a meter.</param>
        public MeterNotDefinedException(MeterScope meterScope, string meterId)
            : this(string.Format(CultureInfo.InvariantCulture, messageFormat, meterId, meterScope), meterScope, meterId)
        {
        }

        /// <summary>
        /// Construct an MeterNotDefinedException with a general message and an inner exception.
        /// </summary>
        /// <param name="meterScope">The scope of the meter.</param>
        /// <param name="meterId">A unique string identifier used to locate a meter.</param>
        /// <param name="ex">The inner exception.</param>
        public MeterNotDefinedException(MeterScope meterScope, string meterId, Exception ex)
            : this(string.Format(CultureInfo.InvariantCulture, messageFormat, meterId, meterScope), meterScope, meterId, ex)
        {
        }

        /// <summary>
        /// Construct an MeterNotDefinedException with a specified message.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="meterScope">The scope of the meter.</param>
        /// <param name="meterId">A unique string identifier used to locate a meter.</param>
        public MeterNotDefinedException(string message, MeterScope meterScope, string meterId)
            : base(message)
        {
            ScopeName = meterScope.ToString();
            MeterId = meterId;
        }

        /// <summary>
        /// Construct an MeterNotDefinedException with a specified message and an inner exception.
        /// </summary>
        /// <param name="message">The message for the exception.</param>
        /// <param name="meterScope">The scope of the meter.</param>
        /// <param name="meterId">A unique string identifier used to locate a meter.</param>
        /// <param name="ex">The inner exception.</param>
        public MeterNotDefinedException(string message, MeterScope meterScope, string meterId, Exception ex)
            : base(message, ex)
        {
            ScopeName = meterScope.ToString();
            MeterId = meterId;
        }
    }
}
