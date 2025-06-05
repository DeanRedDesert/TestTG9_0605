// -----------------------------------------------------------------------
// <copyright file = "Amount.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types
{
    using System.IO;
    using CompactSerialization;
    using Transport;

    /// <summary>
    /// This type represents a monetary value.
    /// </summary>
    public partial class Amount : ICompactSerializable
    {
        #region Constructors

        /// <summary>
        /// Construct the instance with a default monetary value in base units.
        /// </summary>
        public Amount()
            : this(0)
        {
        }

        /// <summary>
        /// Construct the instance with a specific monetary value in base units.
        /// </summary>
        /// <param name="value">The monetary value in base units.</param>
        public Amount(long value)
        {
            Value = value;
            CRC = Crc32.Calculate(Value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Verify the CRC32 over the amount value.
        /// </summary>
        /// <exception cref="AmountCrcException">
        /// Thrown when there is a CRC error in the amount.
        /// </exception>
        public void VerifyCrc()
        {
            var calculatedCrc = Crc32.Calculate(Value);

            if(CRC != calculatedCrc)
            {
                throw new AmountCrcException(Value, CRC, calculatedCrc);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        #region Implicit Conversion Operators

        /// <summary>
        /// Implicitly convert a monetary value in base units to an
        /// <see cref="Amount"/> instance.
        /// </summary>
        /// <param name="value">The monetary value in base units.</param>
        /// <returns>The equivalent <see cref="Amount"/> instance.</returns>
        public static implicit operator Amount(long value)
        {
            return new Amount(value);
        }

        /// <summary>
        /// Implicitly convert a nullable long monetary value to an
        /// <see cref="Amount"/> instance if value is non-null, else return null.
        /// </summary>
        /// <param name="value">The monetary value as a nullable long.</param>
        /// <returns>The equivalent <see cref="Amount"/> instance.</returns>
        public static implicit operator Amount(long? value)
        {
            return value.HasValue ? new Amount((long)value) : null;
        }

        /// <summary>
        /// Implicitly convert an <see cref="Amount"/> instance to a
        /// monetary value in the type of <see cref="long"/> in base units.
        /// Verify the amount's CRC before conversion.
        /// </summary>
        /// <param name="amount">The instance of <see cref="Amount"/>.</param>
        /// <returns>The monetary value in base units.</returns>
        public static implicit operator long(Amount amount)
        {
            long result = 0;

            if(amount != null)
            {
                amount.VerifyCrc();
                result = amount.Value;
            }

            return result;
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Value);
            CompactSerializer.Write(stream, CRC);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Value = CompactSerializer.ReadLong(stream);
            CRC = CompactSerializer.ReadUint(stream);
        }

        #endregion
    }
}
