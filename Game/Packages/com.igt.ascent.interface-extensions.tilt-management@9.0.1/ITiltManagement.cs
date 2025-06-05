//-----------------------------------------------------------------------
// <copyright file = "ITiltManagement.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.TiltManagement
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces.TiltControl;
    using CompactSerialization;
    using Tilts;

    /// <summary>
    /// An interface that allows for the posting and clearing of <see cref="ITilt"/> implementation.
    /// </summary>
    public interface ITiltManagement
    {
        /// <summary>
        /// Post a tilt that implements both <see cref="ITilt"/>
        /// and <see cref="ICompactSerializable"/> interfaces.
        /// </summary>
        /// <param name="tilt">The ITilt and ICompactSerializable object that will be posted.</param>
        /// <param name="key">The key that will be used to track the posted tilt.</param>
        /// <param name="titleFormat">Any format elements the title of the tilt requires.</param>
        /// <param name="messageFormat">Any format elements the message of the tilt requires.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="key"/> is null or empty.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="tilt"/> is null.</exception>
        /// <exception cref="TiltCommunicationException">
        /// Thrown if the tilt category provider cannot handle the post tilt request.
        /// </exception>
        /// <returns>Returns true if the tilt was successfully posted.</returns>
        bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat);

        /// <summary>
        /// Clear a tilt.
        /// </summary>
        /// <param name="key">The key that was used to track a tilt.</param>
        /// <exception cref="TiltCommunicationException">
        /// Thrown if the tilt category provider cannot handle the clear tilt request.
        /// </exception>
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
    }
}