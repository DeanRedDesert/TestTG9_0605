//-----------------------------------------------------------------------
// <copyright file = "ReelCommandResult.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.MechanicalReels
{
    /// <summary>
    /// Reel command results.
    /// </summary>
    public enum ReelCommandResult
    {
        /// <summary>
        /// No issues with command.
        /// </summary>
        Success,

        /// <summary>
        /// General failure.
        /// </summary>
        Failed,

        /// <summary>
        /// The ChangeSpeed command (enhanced reel spin control) had one or more reels that returned an error.
        /// These error codes can usually be ignored by the SDK as the foundation will tilt if the
        /// errors are deemed serious enough. 
        /// </summary>
        ChangeSpeedResultedInOneOrMoreReelErrors,

        /// <summary>
        /// Command was ignored because the reel device is not acquired.
        /// </summary>
        CommandIgnoredAsReelDeviceIsNotAcquired,

        /// <summary>
        /// Command was ignored because the reel device is tilted.
        /// </summary>
        CommandIgnoredAsReelDeviceIsTilted,

        /// <summary>
        /// A foundation of the correct version is not attached.
        /// </summary>
        CommandIgnoredFoundationNotCorrectVersion,

        /// <summary>
        /// The reel count specified in a parameter is 0 or exceeds the available reels in the device.
        /// </summary>
        InvalidReelCountParameter,

        /// <summary>
        /// The reel index specified in a parameter is invalid for the device.
        /// </summary>
        InvalidReelIndexParameter,

        /// <summary>
        /// Can't spin reels specified in command because the reels or reel device are/is not in the correct state.
        /// </summary>
        SpinFailed,

        /// <summary>
        /// Spin reels ignored because a spin command has already been processed; reels must reset to a stopped state first.
        /// </summary>
        SpinIgnoredAlreadySpunForThisStateCycle,

        /// <summary>
        /// Can't stop reels specified in command because the reels or reel device are/is not in the correct state.
        /// </summary>
        StopFailed,

        /// <summary>
        /// Stop reels ignored because a stop command has already been processed; reels must reset to a spinning state first.
        /// </summary>
        StopIgnoredAlreadyStoppedForThisStateCycle,

        /// <summary>
        /// Can't set reel stops specified in command because the reels or reel device are/is not in the correct state.
        /// </summary>
        SetStopsFailed,

        /// <summary>
        /// Can't sync spin reels specified in command because no specified reels would be affected.
        /// </summary>
        SyncSpinFailedNoReelsAffected,

        /// <summary>
        /// Can't sync spin reels specified in command because the reels or reel device are/is not in the correct state.
        /// </summary>
        SyncSpinFailedNotAllInStoppedState,

        /// <summary>
        /// Sync spin reels ignored because a sync spin command has already been processed; reels must reset to a stopped state first.
        /// </summary>
        SyncSpinIgnoredAlreadySyncSpunForThisStateCycle,

        /// <summary>
        /// Can't sync stop reels specified in command because no specified reels would be affected.
        /// </summary>
        SyncStopFailedNoReelsAffected,

        /// <summary>
        /// Can't sync stop reels specified in command because reels are not in the correct state.
        /// </summary>
        SyncStopFailedShelfNotInCorrectState,

        /// <summary>
        /// Can't sync stop reels specified in command because the reels or reel device are/is not in the correct state.
        /// Stop command will be queued up until the reels are ready.
        /// </summary>
        SyncStopQueuedUntilReelsInCorrectSpinningState,

        /// <summary>
        /// Sync stop reels ignored because a sync stop command has already been processed or queued; reels must reset to a spinning state first.
        /// </summary>
        SyncStopIgnoredAlreadySyncStoppedForThisStateCycle,

        /// <summary>
        /// Can't set sync stop reels specified in command because the reels or reel device are/is not in the correct state.
        /// </summary>
        SetSyncStopsFailedReelsNotSpinning,

        /// <summary>
        /// Command was ignored because the reel device is not enabled.
        /// </summary>
        CommandIgnoredAsReelDeviceIsNotEnabled
    }
}
