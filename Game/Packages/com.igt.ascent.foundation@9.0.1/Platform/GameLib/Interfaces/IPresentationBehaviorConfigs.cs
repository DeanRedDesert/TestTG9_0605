// -----------------------------------------------------------------------
// <copyright file = "IPresentationBehaviorConfigs.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// The config data related to game presentation behaviors, whose values
    /// are usually determined by jurisdictional requirements.
    /// </summary>
    public interface IPresentationBehaviorConfigs
    {
        /// <summary>
        /// The minimum base game presentation time in milliseconds.
        /// A value of 0 means there is no minimum time requirement.
        /// </summary>
        int MinimumBaseGameTime { get; }

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
        /// The EGM wide top screen advertisement type.
        /// </summary>
        TopScreenGameAdvertisementType TopScreenGameAdvertisement { get; }

        /// <summary>
        /// Boolean flag that indicates whether a video reel presentation should be displayed for a stepper game.
        /// </summary>
        bool DisplayVideoReelsForStepper { get; }

        /// <summary>
        /// The settings for the Single Option Auto Advance (SOAA) feature in a Bonus.
        /// Null if no settings is provided by the Foundation, in which case it is up to the game
        /// to make sure that all jurisdictional requirements are met.
        /// </summary>
        BonusSoaaSettings BonusSoaaSettings { get; }
    }
}