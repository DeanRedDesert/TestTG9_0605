//-----------------------------------------------------------------------
// <copyright file = "StandaloneMechanicalBell.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    /// <summary>
    /// Provide a virtual implementation of the mechanical bell category for use in standalone.
    /// </summary>
    internal class StandaloneMechanicalBell : IMechanicalBell
    {
        #region IMechanicalBell Implementation

        /// <inheritdoc/>
        public void Ring(string deviceId, uint ringDurationMilliseconds, uint pauseDurationMilliseconds) { }

        /// <inheritdoc/>
        public void Ring(string deviceId, uint count, uint ringDurationMilliseconds, uint pauseDurationMilliseconds) { }

        /// <inheritdoc/>
        public void Stop(string deviceId) { }

        #endregion
    }
}
