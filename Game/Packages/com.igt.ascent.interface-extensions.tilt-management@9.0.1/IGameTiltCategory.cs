//-----------------------------------------------------------------------
// <copyright file = "IGameTiltCategory.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.TiltManagement
{
    using System;
    using System.Collections.Generic;
    using F2L.Schemas.Internal;

    /// <summary>
    /// Interface for the game tilt category of the F2L.
    /// </summary>
    public interface IGameTiltCategory
    {
        /// <summary>
        /// Post a tilt with the foundation.  The game is responsible for maintaining
        /// this tilt on power hit and inactivation.
        /// </summary>
        /// <param name="tiltName">The name the tilt will be registered with on the foundation.</param>
        /// <param name="hardTilt">True if tilt is "hard", which blocks gameplay.</param>
        /// <param name="notifyProtocols">True if the foundation should notify 
        /// an attendant about the tilt via a protocol.</param>
        /// <param name="progressiveLinkDown">True if the foundation should treat this tilt as a
        /// progressive link down tilt, which may need special handling.</param>
        /// <param name="tiltLocalizations">A list of localizations for the tilt. The game
        /// is expected to supply a localization for each supported culture.</param>
        /// <returns>Returns false if the tilt could not be posted with the foundation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltName"/> or <paramref name="tiltLocalizations"/> are null or empty.
        /// </exception>
        bool PostTilt(string tiltName, bool hardTilt, bool notifyProtocols, bool progressiveLinkDown,
                      ICollection<TiltLocalization> tiltLocalizations);

        /// <summary>
        /// Clear a tilt with the foundation.
        /// </summary>
        /// <param name="tiltName">The name the tilt had been registered with on the foundation.</param>
        /// <returns>Returns false if the tilt could not be cleared with the foundation.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="tiltName"/> is null or empty.
        /// </exception>
        bool ClearTilt(string tiltName);
    }
}