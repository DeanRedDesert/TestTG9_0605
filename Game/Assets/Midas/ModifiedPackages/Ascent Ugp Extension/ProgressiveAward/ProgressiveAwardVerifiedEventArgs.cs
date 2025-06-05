//-----------------------------------------------------------------------
// <copyright file = "ProgressiveAwardVerifiedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using System.Text;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for progressive award being verified.
    /// </summary>
    [Serializable]
    public class ProgressiveAwardVerifiedEventArgs : TransactionalEventArgs
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
        /// Gets or sets the verified amount of progressive award.
        /// </summary>
        public long VerifiedAmount { get; set; }

        /// <summary>
        /// Gets or sets the pay type of progressive award.
        /// </summary>
        public ProgressiveAwardPayType PayType { get; set; }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("UGP Progressive Award Verified Event -");
            builder.AppendLine("\t ProgressiveAwardIndex = " + ProgressiveAwardIndex);
            builder.AppendLine("\t ProgressiveLevelId = " + ProgressiveLevelId);
            builder.AppendLine("\t VerifiedAmount = " + VerifiedAmount);
            builder.AppendLine("\t PayType = " + PayType);

            return builder.ToString();
        }
    }
}
