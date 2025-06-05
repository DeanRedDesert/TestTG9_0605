//-----------------------------------------------------------------------
// <copyright file = "IMechanicalBell.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Interface for Mechanical Bell related functionality of the cabinet.
    /// </summary>
    public interface IMechanicalBell
    {
        /// <summary>
        /// Ring the mechanical bell once, in addition to any previous rings requested.
        /// </summary>
        /// <param name="deviceId">The id of the mechanical bell device.</param>
        /// <param name="ringDurationMilliseconds">
        /// The number of milliseconds that a single ring should last.
        /// </param>
        /// <param name="pauseDurationMilliseconds">
        /// The number of milliseconds that a pause between rings should last.
        /// </param>
        void Ring(string deviceId, uint ringDurationMilliseconds, uint pauseDurationMilliseconds);

        /// <summary>
        /// Ring the mechanical bell the given number of times, in addition to any previous rings requested.
        /// </summary>
        /// <param name="deviceId">The id of the mechanical bell device.</param>
        /// <param name="count">The number of times to ring.</param>
        /// <param name="ringDurationMilliseconds">
        /// The number of milliseconds that a single ring should last.
        /// </param>
        /// <param name="pauseDurationMilliseconds">
        /// The number of milliseconds that a pause between rings should last.
        /// </param>
        void Ring(string deviceId, uint count, uint ringDurationMilliseconds, uint pauseDurationMilliseconds);

        /// <summary>
        /// Stop ringing the mechanical bell.
        /// </summary>
        /// <param name="deviceId">The id of the mechanical bell device.</param>
        void Stop(string deviceId);
    }
}
