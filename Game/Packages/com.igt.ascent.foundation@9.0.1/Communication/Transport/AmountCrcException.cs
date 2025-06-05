//-----------------------------------------------------------------------
// <copyright file = "AmountCrcException.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

 namespace IGT.Game.Core.Communication.Foundation.Transport
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Exception which indicates that there was a CRC error in
    /// an instance of AmountType.
    /// </summary>
    public class AmountCrcException : Exception
    {
        /// <summary>
        /// Format for the exception message.
        /// </summary>
        private const string MessageFormat =
            "Amount has Invalid CRC. Value: {0} CRC: {1} Calculated CRC: {2}";

        /// <summary>
        /// The value the amount.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public long Value { get; private set; }

        /// <summary>
        /// The CRC contained in the amount.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public uint AmountCrc { get; private set; }

        /// <summary>
        /// A CRC calculated based on the amount value.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public uint CalculatedCrc { get; private set; }

        /// <summary>
        /// Construct an instance of AmountCrcException.
        /// </summary>
        /// <param name="value">The value of the amount.</param>
        /// <param name="amountCrc">The CRC contained in the amount.</param>
        /// <param name="calculatedCrc">A CRC calculated based on the amount value.</param>
        public AmountCrcException(long value, uint amountCrc, uint calculatedCrc)
            : base(string.Format(CultureInfo.InvariantCulture, MessageFormat, value, amountCrc, calculatedCrc))
        {
            Value = value;
            AmountCrc = amountCrc;
            CalculatedCrc = calculatedCrc;
        }
    }
}
