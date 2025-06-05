//-----------------------------------------------------------------------
// <copyright file = "AmountType.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace IGT.Game.Core.Communication.Foundation.F2L.Schemas.Internal
{
    using System.IO;
    using CompactSerialization;
    using Transport;

    /// <summary>
    /// This class contains methods which extend the functionality
    /// of the automatically generated type AmountType.
    /// </summary>
    public partial class AmountType : ICompactSerializable
    {
        #region Constructors

        /// <summary>
        /// Construct an instance of AmountType with default value 0.
        /// </summary>
        public AmountType()
            : this(0)
        {
        }

        /// <summary>
        /// Construct an instance of AmountType with an amount value,
        /// calculate the CRC of the value for the instance.
        /// </summary>
        /// <param name="value">The amount value.</param>
        public AmountType(long value)
        {
            Value = value;
            CRC = Crc32.Calculate(Value);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Verify if this amount's CRC is correct.
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

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return Value.ToString();
        }

        #endregion

        #region Implicit Conversion Operators

        /// <summary>
        /// Implicitly convert an amount value of type long to
        /// an AmountType instance.
        /// </summary>
        /// <param name="value">The amount value.</param>
        /// <returns>An instance of AmountType.</returns>
        public static implicit operator AmountType(long value)
        {
            return new AmountType(value);
        }

        /// <summary>
        /// Implicitly convert an AmountType instance to an
        /// amount value of type long.  Verify the amount's
        /// CRC before conversion.
        /// </summary>
        /// <param name="amount">The instance of AmountType.</param>
        /// <returns>The amount value.</returns>
        public static implicit operator long(AmountType amount)
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
