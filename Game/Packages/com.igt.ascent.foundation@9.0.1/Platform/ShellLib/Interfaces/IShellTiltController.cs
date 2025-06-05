// -----------------------------------------------------------------------
// <copyright file = "IShellTiltController.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Tilts;

    /// <summary>
    /// An interface that allows a shell and its coplayers to post and clear tilts.
    /// </summary>
    public interface IShellTiltController
    {
        /// <summary>
        /// Posts a tilt that implements the <see cref="ITilt"/> interface.
        /// </summary>
        /// <param name="tilt">The tilt object that will be posted.</param>
        /// <param name="key">The key that will be used to track the posted tilt.</param>
        /// <param name="titleFormat">Any format elements the title of the tilt requires.</param>
        /// <param name="messageFormat">Any format elements the message of the tilt requires.</param>
        /// <param name="coplayerId">The id for the coplayer posting this tilt, null if posted by the shell.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tilt"/> is null.</exception>
        /// <returns>Returns true if the tilt was successfully posted.</returns>
        bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat, int? coplayerId = null);

        /// <summary>
        /// Clears a tilt.
        /// </summary>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <param name="coplayerId">
        /// The id for the coplayer that originally posted this tilt, null if posted by the shell.
        /// </param>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        bool ClearTilt(string key, int? coplayerId = null);

        /// <summary>
        /// Clears all current tilts for the given coplayer or shell.
        /// </summary>
        /// <param name="coplayerId">
        /// The id for the coplayer that originally posted this tilt, null if posted by the shell.
        /// </param>
        /// <returns>Returns true if all of the tilts were successfully cleared.</returns>
        bool ClearAllTilts(int? coplayerId = null);

        /// <summary>
        /// Returns the tilt status of the shell or identified coplayer.
        /// </summary>
        /// <param name="coplayerId">
        /// The id for the coplayer that we are checking the tilt status for, null if we are checking for the shell.
        /// </param>
        /// <returns>
        /// Returns true if at least one tilt is currently posted.
        /// </returns>
        bool IsTilted(int? coplayerId = null);

        /// <summary>
        /// Returns whether or not the shell or any coplayer is tilted.
        /// </summary>
        /// <returns>
        /// Returns true if at least one tilt is currently posted.
        /// </returns>
        bool IsAnyTilted();

        /// <summary>
        /// Returns the tilt status of a particular tilt key.
        /// </summary>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <param name="coplayerId">
        /// The id for the coplayer that wants to check if a tilt is present, null if shell.
        /// </param>
        /// <returns>
        /// Returns true if the tilt is currently posted.
        /// </returns>
        bool TiltPresent(string key, int? coplayerId = null);

        /// <summary>
        /// The event that is triggered when a tilt is cleared by an attendant.
        /// </summary>
        /// <remarks>
        /// This event is only raised if the "AttendantClear" attribute of the tilt is true.
        /// </remarks>
        event EventHandler<ShellTiltClearedByAttendantEventArgs> ShellTiltClearedByAttendantEvent;
    }
}