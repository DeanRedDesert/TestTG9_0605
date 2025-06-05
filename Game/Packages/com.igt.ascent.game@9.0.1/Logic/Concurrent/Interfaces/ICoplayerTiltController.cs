// -----------------------------------------------------------------------
// <copyright file = "IShellTiltController.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Tilts;

    /// <summary>
    /// An interface used for the coplayers to interact with tilts.
    /// </summary>
    public interface ICoplayerTiltController
    {
        /// <summary>
        /// Posts a tilt that implements the <see cref="ITilt"/> interface.
        /// </summary>
        /// <param name="tilt">The tilt object that will be posted.</param>
        /// <param name="key">The key that will be used to track the posted tilt.</param>
        /// <param name="titleFormat">Any format elements the title of the tilt requires.</param>
        /// <param name="messageFormat">Any format elements the message of the tilt requires.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tilt"/> is null.</exception>
        /// <returns>Returns true if the tilt was successfully posted.</returns>
        /// <remarks>
        /// This requires that this coplayer is not making this action within a heavyweight
        /// transaction as that would cause a deadlock.
        /// </remarks>
        bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat);

        /// <summary>
        /// Clears a tilt.
        /// </summary>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        /// <remarks>
        /// This requires that this coplayer is not making this action within a heavyweight
        /// transaction as that would cause a deadlock.
        /// </remarks>
        bool ClearTilt(string key);

        /// <summary>
        /// Clears all current tilts for this coplayer.
        /// </summary>
        /// <returns>Returns true if all of the tilts were successfully cleared.</returns>
        /// <remarks>
        /// This requires that this coplayer is not making this action within a heavyweight
        /// transaction as that would cause a deadlock.
        /// </remarks>
        bool ClearAllTilts();

        /// <summary>
        /// Returns the tilt status of this coplayer.
        /// </summary>
        /// <returns>
        /// Returns true if at least one tilt is currently posted.
        /// </returns>
        bool IsTilted();

        /// <summary>
        /// Returns the tilt status of a particular tilt key.
        /// </summary>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <returns>
        /// Returns true if a tilt with the specified key is currently posted.
        /// </returns>
        bool TiltPresent(string key);

        /// <summary>
        /// The event that is triggered when a tilt is cleared by an attendant.
        /// </summary>
        /// <remarks>
        /// This event is only raised if the "AttendantClear" attribute of the tilt is true.
        /// </remarks>
        // ReSharper disable once EventNeverSubscribedTo.Global
        event EventHandler<ShellTiltClearedByAttendantEventArgs> TiltClearedByAttendantEvent;
    }
}