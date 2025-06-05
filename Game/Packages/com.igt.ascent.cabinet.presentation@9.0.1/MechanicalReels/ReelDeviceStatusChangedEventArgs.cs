//-----------------------------------------------------------------------
// <copyright file = "ReelDeviceStatusChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;

    /// <summary>
    /// Event arguments which contain reel device online status changed information.
    /// </summary>
    public class ReelDeviceStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The device the event is for.
        /// </summary>
        public Hardware HardwareId
        {
            get;
        }

        #region Constructor

        /// <summary>
        /// Constructs an instance of <see cref="ReelDeviceStatusChangedEventArgs"/>.
        /// </summary>
        public ReelDeviceStatusChangedEventArgs(Hardware hardwareHardwareId)
        {
            HardwareId = hardwareHardwareId;
        }

        #endregion
    }
}
