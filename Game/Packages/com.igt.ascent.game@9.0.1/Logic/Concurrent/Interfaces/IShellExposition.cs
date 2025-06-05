// -----------------------------------------------------------------------
// <copyright file = "IShellExposition.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Communication.Platform.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Money;

    /// <summary>
    /// This interface provides APIs for coplayers to receive notifications from the Shell
    /// and query config data and properties values that are manged by the Shell.
    /// </summary>
    public interface IShellExposition
    {
        #region Config Data

        /// <summary>
        /// Gets the machine wide bet constraints.
        /// </summary>
        MachineWideBetConstraints MachineWideBetConstraints { get; }

        /// <summary>
        /// Gets the credit formatter.
        /// </summary>
        CreditFormatter CreditFormatter { get; }

        /// <summary>
        /// Gets the minimum base game presentation time in milliseconds.
        /// A value of 0 means there is no minimum time requirement.
        /// </summary>
        int MinimumBaseGameTime { get; }

        /// <summary>
        /// Gets the minimum time for a single slot free spin cycle in milliseconds.
        /// A value of 0 means there is no minimum time requirement.
        /// </summary>
        int MinimumFreeSpinTime { get; }

        /// <summary>
        /// Gets the behavior that the credit meter should exhibit.
        /// </summary>
        CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; }

        /// <summary>
        /// Gets the max bet button behavior.
        /// </summary>
        MaxBetButtonBehavior MaxBetButtonBehavior { get; }

        /// <summary>
        /// Gets the flag that indicates whether a video reel presentation should be displayed for a stepper game.
        /// </summary>
        bool DisplayVideoReelsForStepper { get; }

        #endregion

        #region Property Data

        /// <summary>
        /// Gets the bank related property values.
        /// </summary>
        BankProperties BankProperties { get; }

        #endregion

        #region Property Data Update Events

        /// <summary>
        /// Occurs when one of the bank properties has changed.
        /// </summary>
        event EventHandler<BankPropertiesUpdateEventArgs> BankPropertiesUpdateEvent;

        #endregion
    }
}