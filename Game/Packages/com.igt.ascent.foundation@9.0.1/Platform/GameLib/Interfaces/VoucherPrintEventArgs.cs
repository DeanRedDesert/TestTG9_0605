//-----------------------------------------------------------------------
// <copyright file = "VoucherPrintEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// Event payload for voucher print notification.
    /// </summary>
    [Serializable]
    public class VoucherPrintEventArgs : EventArgs
    {
        /// <summary>
        /// Get the value of the amount, in base units, that appears on the voucher being printed.
        /// </summary>
        public long Value { get; private set; }

        /// <summary>
        /// Get the type of voucher print event.
        /// </summary>
        public VoucherPrintEvent VoucherPrintEvent { get; private set; }

        /// <summary>
        /// Construct a VoucherPrintEventArgs with voucher print event type and amount value.
        /// </summary>
        /// <param name="printEvent">The type of voucher print event.</param>
        /// <param name="amount">The value of the amount in base units.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="amount"/> is less than zero.
        /// </exception>
       public VoucherPrintEventArgs (VoucherPrintEvent printEvent, long amount)
       {
           // Check the parameter.
           if(amount < 0)
           {
               throw new ArgumentOutOfRangeException(nameof(amount), "Amount value is less than zero, please verify it.");
           }

           VoucherPrintEvent = printEvent;
           Value = amount;
       }

       /// <summary>
       /// Override base implementation to provide better information.
       /// </summary>
       /// <returns>A string describing the object.</returns>
       public override string ToString()
       {
           var builder = new StringBuilder();

           builder.AppendLine("VoucherPrintEventArgs -");
           builder.AppendLine("\t Voucher Print Event = " + VoucherPrintEvent);
           builder.AppendLine("\t Amount Value = " + Value);

           return builder.ToString();
       }
    }
}
