//-----------------------------------------------------------------------
// <copyright file = "ReelStatusCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;
    using Cabinet.MechanicalReels;

    /// <summary>
    /// The status code in the UsbStatusMessage received from
    /// the reel device driver.
    /// </summary>
    [Serializable]
    internal enum ReelStatusCode : ushort
    {
        #region Statuses

        /// <summary>
        /// Reel is idle at a known stop.
        /// </summary>
        Stopped = 0x0000,

        /// <summary>
        /// Reel is idle at an unknown stop.
        /// </summary>
        StoppedAtUnknownStop = 0x2000,

        /// <summary>
        /// Reel is acceleration from a stop.
        /// </summary>
        Accelerating = 0x2001,

        /// <summary>
        /// Reel is decelerating to a stop.
        /// </summary>
        Decelerating = 0x2002,

        /// <summary>
        /// Reel is spinning at a constant speed.
        /// </summary>
        ConstantSpeed = 0x2003,

        /// <summary>
        /// Reel is in slow spin.
        /// </summary>
        SlowSpin = 0x2004,

        /// <summary>
        /// Reel is moving in a way not described above,
        /// e.g. changing speed or shaking.
        /// </summary>
        MovingIrregularly = 0x2005,

        /// <summary>
        /// Reel is executing the Halt command.
        /// </summary>
        Halt = 0x2006,

        /// <summary>
        /// A recent stop command specified a stop position that
        /// was either <see cref="SpinProfile.NoStop"/> or a value
        /// different from a previously requested stop.
        /// </summary>
        InvalidStopSpecified = 0x2008,

        #endregion

        #region Tilts

        /// <summary>
        /// Reserved.  Do not use.
        /// </summary>
        Reserved = 0x8000,

        /// <summary>
        /// Reel moved when it should have been stationary.
        /// </summary>
        MovedFromStationary = 0x8001,

        /// <summary>
        /// Reel stalled when it should have been moving.
        /// </summary>
        StalledWhileMoving = 0x8002,

        /// <summary>
        /// Reel could not find the requested stop position.
        /// </summary>
        StopNotFound = 0x8003,

        /// <summary>
        /// Reel had optic sequence errors during deceleration.
        /// </summary>
        OpticSequenceError = 0x8004,

        /// <summary>
        /// Reel is disconnected.
        /// </summary>
        Disconnected = 0x8005

        #endregion
    }
}
