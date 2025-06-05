//-----------------------------------------------------------------------
// <copyright file = "IBankSynchronizationSettings.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using CSI.Schemas;

    /// <summary>
    /// Interface for the bank synchronization feature settings.
    /// </summary>
    public interface IBankSynchronizationSettings
    {
        /// <summary>
        /// Gets or sets if the feature is enabled or not.
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the time precision.
        /// </summary>
        TimeFramePrecisionLevel Precision { get; set; }

        /// <summary>
        /// Gets or sets the bank position. This value is 1-based.
        /// </summary>
        uint BankPosition { get; set; }

        /// <summary>
        /// Gets or sets the total number of machines in the bank.
        /// </summary>
        uint TotalMachinesInBank { get; set; }
    }
}
