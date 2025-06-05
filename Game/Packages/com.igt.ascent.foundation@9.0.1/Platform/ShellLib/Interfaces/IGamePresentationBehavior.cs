// -----------------------------------------------------------------------
// <copyright file = "IGamePresentationBehavior.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for presentation configuration items.
    /// </summary>
    public interface IGamePresentationBehavior
    {
        /// <summary>
        /// The minimum base game presentation time in milliseconds.
        /// A value of 0 means there is no minimum time requirement.
        /// </summary>
        int MinimumBaseGameTime{ get; }

        /// <summary>
        /// The minimum time for a single slot free spin cycle in milliseconds.
        /// A value of 0 means there is no minimum time requirement.
        /// </summary>
        int MinimumFreeSpinTime { get; }

        /// <summary>
        /// The behavior that the credit meter should exhibit.
        /// </summary>
        CreditMeterDisplayBehaviorMode CreditMeterBehavior { get; }

        /// <summary>
        /// The max bet button behavior.
        /// </summary>
        MaxBetButtonBehavior MaxBetButtonBehavior { get; }

        /// <summary>
        /// Boolean flag that indicates whether a video reel presentation should be displayed for a stepper game.
        /// </summary>
        bool DisplayVideoReelsForStepper { get; }
    }
}