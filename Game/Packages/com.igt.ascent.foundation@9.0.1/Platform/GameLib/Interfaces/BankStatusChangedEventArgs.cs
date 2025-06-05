//-----------------------------------------------------------------------
// <copyright file = "BankStatusChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// Event indicating that the bank status has been changed.
    /// </summary>
    [Serializable]
    public class BankStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The Bank Status after the change.
        /// </summary>
        public BankStatus Status { get; private set; }

        /// <summary>
        /// Constructs a BankStatusChangedEventArgs with given Bank Status.
        /// </summary>
        /// <param name="status"></param>
        public BankStatusChangedEventArgs(BankStatus status)
        {
            Status = status;
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("BankStatusChangedEvent -");
            builder.AppendLine("\t New Status = " + Status);

            return builder.ToString();
        }
    }
}
