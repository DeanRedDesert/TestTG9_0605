//-----------------------------------------------------------------------
// <copyright file = "IMeterSource.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Interface which abstracts access to common meters.
    /// </summary>
    public interface IMeterSource
    {
        /// <summary>
        /// Represents the amount of money available for player betting
        /// that is suitable for display to the player, in cents.
        /// </summary>
        long CreditMeter { get; }

        /// <summary>
        /// Banked Credits mode - Represents the amount of money available for player betting
        /// that is suitable for display to the player, in cents.
        /// </summary>
        long WagerableMeter { get; }

        /// <summary>
        /// Represents the win is pending or not, true means the current win
        /// has not been added into credit meter.
        /// </summary>
        bool IsWinPending { get;}

        /// <summary>
        /// Represents the amount paid to the player during the last/current cashout
        /// and is suitable for display to the player, in cents.
        /// This meter will be reset to zero at the appropriate times
        /// (e.g. at game start or cashout).
        /// </summary>
        /// <remarks>
        /// In environments where there isn't a traditional cashout, for instance web or mobile, this value may be 0.
        /// </remarks>
        long PaidMeter { get; }

        /// <summary>
        /// The current display behavior for the credit meter. Specifies if the meter should be shown only as currency,
        /// or if it should be selectable by the player defaulting to either currency or credits.
        /// </summary>
        CreditMeterDisplayBehaviorMode CreditMeterDisplayBehavior { get; }

        /// <summary>
        /// Event posted when meter source is updated.
        /// </summary>
        event EventHandler<MeterSourceUpdatedEventArgs> MeterSourceUpdatedEvent;
    }
}