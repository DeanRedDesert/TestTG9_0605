//-----------------------------------------------------------------------
// <copyright file = "IExtensionTiltController.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces.TiltControl
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Tilts;

    /// <summary>
    /// An interface that allows extensions to post and clear tilts.
    /// </summary>
    public interface IExtensionTiltController
    {
        /// <summary>
        /// Posts a tilt that implements the <see cref="IActiveTilt"/> interface.
        /// </summary>
        /// <param name="tilt">The tilt object that will be posted.</param>
        /// <param name="extensionGuid">The Guid of the extension that wants to post a tilt.</param>
        /// <param name="key">The key that will be used to track the posted tilt. The maximum allowed key length is 39 characters.</param>
        /// <param name="titleFormat">Any format elements the title of the tilt requires.</param>
        /// <param name="messageFormat">Any format elements the message of the tilt requires.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tilt"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="extensionGuid"/> is empty.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="key"/> is null or empty or exceeds the limit of 39 characters.
        /// </exception>
        /// <returns>Returns true if the tilt was succesfully posted.</returns>
        bool PostTilt(IActiveTilt tilt, Guid extensionGuid, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat);

        /// <summary>
        /// Clears a tilt.
        /// </summary>
        /// <param name="extensionGuid">The Guid of the extension that wants to clear a tilt.</param>
        /// <param name="key">The key that was used to track a tilt.</param>
        /// <returns>Returns true if the tilt was successfully cleared.</returns>
        bool ClearTilt(Guid extensionGuid, string key);

        /// <summary>
        /// Returns the tilt status of the game.
        /// </summary>
        /// <returns>Returns true if at least one tilt is currently posted by an extension.</returns>
        bool IsTilted(Guid extensionGuid);

        /// <summary>
        /// Returns the tilt status of a particular tilt key from an extension.
        /// </summary>
        /// <param name="extensionGuid">The Guid of the extension that wants to check if a tilt is present.</param>
        /// <param name="key">The key that was used to track a tilt.</param>
        /// <returns>Returns true if the tilt is currently posted.</returns>
        bool TiltPresent(Guid extensionGuid, string key);

        /// <summary>
        /// The event that is triggered when a tilt is cleared by an attendant.
        /// </summary>
        /// <remarks>
        /// This event is only raised if the "AttendantClear" attribute of the tilt is true.
        /// </remarks>
        event EventHandler<ExtensionTiltClearedByAttendantEventArgs> ExtensionTiltClearedByAttendantEvent;

    }
}