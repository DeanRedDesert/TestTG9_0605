//-----------------------------------------------------------------------
// <copyright file = "ITiltController.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces.TiltControl
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Tilts;

    /// <summary>
    /// An interface that allows for the posting and clearing of <see cref="IActiveTilt"/> implementation.
    /// </summary>
    public interface ITiltController
    {
        /// <summary>
        /// Posts a tilt that implements the <see cref="IActiveTilt"/> interface.
        /// </summary>
        /// <param name="tilt">The tilt object that will be posted.</param>
        /// <param name="key">The key that will be used to track the posted tilt.</param>
        /// <param name="titleFormat">Any format elements the title of the tilt requires.</param>
        /// <param name="messageFormat">Any format elements the message of the tilt requires.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tilt"/> is null.</exception>
        /// <returns>Returns true if the tilt was succesfully posted.</returns>
        bool PostTilt(IActiveTilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat);

        /// <summary>
        /// Clears a tilt.
        /// </summary>
        /// <param name="key">The key that was used to track a tilt.</param>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        bool ClearTilt(string key);

        /// <summary>
        /// Returns the tilt status of the game.
        /// </summary>
        /// <returns>Returns true if at least one tilt is currently posted.</returns>
        bool IsTilted();

        /// <summary>
        /// Returns the tilt status of a particular tilt key.
        /// </summary>
        /// <param name="key">The key that was used to track a tilt.</param>
        /// <returns>Returns true if the tilt is currently posted.</returns>
        bool TiltPresent(string key);

        /// <summary>
        /// The event that is triggered when a tilt is cleared by an attendant.
        /// </summary>
        /// <remarks>
        /// This event is only raised if the "AttendantClear" attribute of the tilt is true.
        /// </remarks>
        event EventHandler<TiltClearedByAttendantEventArgs> TiltClearedByAttendantEvent;
    }
}
