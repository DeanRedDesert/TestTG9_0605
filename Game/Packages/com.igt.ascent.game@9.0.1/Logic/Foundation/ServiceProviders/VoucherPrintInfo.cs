//-----------------------------------------------------------------------
// <copyright file = "VoucherPrintInfo.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.IO;
    using System.Text;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Cloneable;
    using CompactSerialization;
    using Session;

    /// <summary>
    /// This class gives the information of voucher print notification.
    /// </summary>
    /// <remarks>
    /// This class is used for the payload in the game service.
    /// </remarks>
    [Serializable]
    public class VoucherPrintInfo : ICompactSerializable, IDeepCloneable
    {
        /// <summary>
        /// Get voucher print event type.
        /// </summary>
        public VoucherPrintEvent VoucherPrintEvent { get; private set; }

        /// <summary>
        /// Get voucher print amount value in base units.
        /// </summary>
        public long Amount { get; private set; }

        /// <summary>
        /// Get voucher print session. Before using the payload,
        /// game service consumer need check if it is a new session id.
        /// </summary>
        public UniqueIdentifier VoucherPrintSession { get; private set; }

        /// <summary>
        /// Initialize an instance of <see cref="VoucherPrintInfo"/> with a unique identifier
        /// which indicates the voucher print event type and amount value in base units.
        /// </summary>
        /// <param name="voucherPrintEvent">The voucher print event type.</param>
        /// <param name="amountValue">The voucher print amount value in base units.</param>
        /// <param name="uniqueIdentifier">
        /// The unique identifier identifying this voucher print notification occurrence.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amountValue"/> is less than zero.
        /// </exception>
        public VoucherPrintInfo(VoucherPrintEvent voucherPrintEvent, long amountValue, UniqueIdentifier uniqueIdentifier)
        {
            // Check the parameter.
            if(amountValue < 0)
            {
                throw new ArgumentOutOfRangeException("amountValue",
                                                      "Amount value is less than zero, please verify it.");
            }

            VoucherPrintEvent = voucherPrintEvent;
            Amount = amountValue;
            VoucherPrintSession = uniqueIdentifier;
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        /// <remarks>
        /// The default constructor which is used for deserialization purpose and not supposed to be invoked
        /// by the user code.
        /// </remarks>
        public VoucherPrintInfo()
            : this(VoucherPrintEvent.VoucherPrintingInitiated, 0, null)
        {
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("VoucherPrintInfo -");
            builder.AppendLine("\t Voucher Print Event = " + VoucherPrintEvent);
            builder.AppendLine("\t Amount = " + Amount);
            builder.AppendLine(VoucherPrintSession == null ? "\t  VoucherPrintSession is null" : VoucherPrintSession.ToString());
            return builder.ToString();
        }

        #region IDeepCloneable Members

        /// <inheritDoc/>
        public object DeepClone()
        {
            // This type is supposed to be immutable. However, the invoking to Deserialize() of an existing instance could 
            // corrupt its immutibility. Thus, we must disallow such invoking for an instance already in use.
            return this;
        }

        #endregion

        #region ICompactSerializable Members

        /// <inheritDoc/>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, VoucherPrintEvent);
            CompactSerializer.Write(stream, Amount);
            CompactSerializer.Write(stream, VoucherPrintSession);
        }

        /// <inheritDoc/>
        public void Deserialize(Stream stream)
        {
            VoucherPrintEvent = CompactSerializer.ReadEnum<VoucherPrintEvent>(stream);
            Amount = CompactSerializer.ReadLong(stream);
            VoucherPrintSession = CompactSerializer.ReadSerializable<UniqueIdentifier>(stream);
        }

        #endregion
    }
}
