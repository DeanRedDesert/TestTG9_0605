//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAwardPaidEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for progressive award being paid.
    /// </summary>
    [Serializable]
    public class ProgressiveAwardPaidEventArgs : TransactionalEventArgs
    {
        /// <summary>
        /// Gets or sets the index of progressive award.
        /// </summary>
        public int ProgressiveAwardIndex { get; set; }

        /// <summary>
        /// Gets or sets the ID of progressive level.
        /// </summary>
        public string ProgressiveLevelId { get; set; }

        /// <summary>
        /// Gets or sets the paid amount of progressive award.
        /// </summary>
        public long PaidAmount { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UGP Progressive Award Paid Event -");
            builder.AppendLine("\t ProgressiveAwardIndex = " + ProgressiveAwardIndex);
            builder.AppendLine("\t ProgressiveLevelId = " + ProgressiveLevelId);
            builder.AppendLine("\t PaidAmount = " + PaidAmount);

            return builder.ToString();
        }
    }
}
