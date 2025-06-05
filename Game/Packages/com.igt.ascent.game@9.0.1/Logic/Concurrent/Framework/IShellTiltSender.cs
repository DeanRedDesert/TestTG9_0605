// -----------------------------------------------------------------------
// <copyright file = "IShellTiltSender.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Tilts;

    /// <summary>
    /// Interface for coplayers to send tilts to the shell.
    /// </summary>
    internal interface IShellTiltSender
    {
        /// <summary>
        /// Posts a tilt that implements the <see cref="ITilt"/> interface.
        /// </summary>
        /// <param name="tilt">The tilt object that will be posted.</param>
        /// <param name="key">The key that will be used to track the posted tilt.</param>
        /// <param name="titleFormat">Any format elements the title of the tilt requires.</param>
        /// <param name="messageFormat">Any format elements the message of the tilt requires.</param>
        /// <param name="coplayerId">The id for the coplayer posting this tilt.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tilt"/> is null.</exception>
        /// <returns>Returns true if the tilt was successfully posted.</returns>
        bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat, int coplayerId);

        /// <summary>
        /// Clears a tilt.
        /// </summary>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <param name="coplayerId">
        /// The id for the coplayer that originally posted this tilt.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is null or empty.
        /// </exception>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        bool ClearTilt(string key, int coplayerId);

        /// <summary>
        /// Clears all current tilts for the given coplayer.
        /// </summary>
        /// <param name="coplayerId">
        /// The id for the coplayer that originally posted this tilt.
        /// </param>
        /// <returns>Returns true if all of the tilts were successfully cleared.</returns>
        bool ClearAllTilts(int coplayerId);

        /// <summary>
        /// Returns the tilt status of the identified coplayer.
        /// </summary>
        /// <param name="coplayerId">
        /// The id for the coplayer that we are checking the tilt status for.
        /// </param>
        /// <returns>
        /// Returns true if at least one tilt is currently posted.
        /// </returns>
        bool IsTilted(int coplayerId);

        /// <summary>
        /// Returns the tilt status of a particular tilt key.
        /// </summary>
        /// <param name="key">
        /// The key that was used to track a tilt.
        /// </param>
        /// <param name="coplayerId">
        /// The id for the coplayer that wants to check if a tilt is present.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is null or empty.
        /// </exception>
        /// <returns>
        /// Returns true if the tilt is currently posted.
        /// </returns>
        bool TiltPresent(string key, int coplayerId);
    }
}