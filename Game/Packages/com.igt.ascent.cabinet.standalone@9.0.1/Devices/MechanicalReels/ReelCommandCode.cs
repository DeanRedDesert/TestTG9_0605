//-----------------------------------------------------------------------
// <copyright file = "ReelCommandCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.MechanicalReels
{
    using System;

    /// <summary>
    /// Command code to control IGT reel devices.
    /// </summary>
    [Serializable]
    internal enum ReelCommandCode : byte
    {
        /// <summary>
        /// Set all characteristics for the reel to the default values.
        /// </summary>
        SetDefaults = 0x00,

        /// <summary>
        /// Set the acceleration profile for the reel.
        /// </summary>
        SetAcceleration = 0x01,

        /// <summary>
        /// Set the deceleration profile for the reel.
        /// </summary>
        SetDeceleration = 0x02,

        /// <summary>
        /// Set the speed for the reel.
        /// </summary>
        SetSpeed = 0x03,

        /// <summary>
        /// Set the spin duration (acceleration + constant speed) for the reel.
        /// </summary>
        SetDuration = 0x04,

        /// <summary>
        /// Set the spin direction of the reel.
        /// </summary>
        SetDirection = 0x05,

        /// <summary>
        /// Set the desired stop position for the reel.
        /// </summary>
        SetStop = 0x06,

        /// <summary>
        /// Set the motion attribute for the reel.
        /// </summary>
        SetAttribute = 0x07,

        /// <summary>
        /// Apply the attributes immediately, as opposed to when
        /// the feature receives the <see cref="Spin"/> command.
        /// </summary>
        ApplyAttributes = 0x08,

        /// <summary>
        /// Spin the reel using the current configuration values.
        /// </summary>
        Spin = 0x09,

        /// <summary>
        /// Stop the reel at the specified stop as soon as possible
        /// using the configuration settings that apply.
        /// </summary>
        Stop = 0x0A,

        /// <summary>
        /// Stop all activity on the reel and put the reel into
        /// safe state operation.
        /// </summary>
        SlowSpin = 0x0B,

        /// <summary>
        /// This command is legal during a tilt.
        /// It stop the reel at a valid stop as soon as possible.
        /// </summary>
        Halt = 0x0C,

        /// <summary>
        /// Inform the driver whether the reel is mounted in a
        /// non-standard orientation.
        /// </summary>
        SetOrientation = 0x0D,

        /// <summary>
        /// Force the reels to stop in a particular order,
        /// regardless of other factors.
        /// </summary>
        SetStopOrder = 0x0E,

        /// <summary>
        /// Spin two or more reels.  It is used to synchronize the reels
        /// so that they can stop at their respective stop positions at
        /// the same time.
        /// </summary>
        SynchronousSpin = 0x0F,

        /// <summary>
        /// Assign a stop position for a reel previously spun using the
        /// <see cref="SynchronousSpin"/> command with a stop of 0xFF.
        /// </summary>
        SetSynchronousStops = 0x10,

        /// <summary>
        /// Stop the specified reels spun using the <see cref="SynchronousSpin"/>
        /// command as soon as possible.
        /// </summary>
        SynchronousStop = 0x11,

        /// <summary>
        /// Change the speed or direction of a set of spinning reels.
        /// </summary>
        ChangeSpeed = 0x12
    }
}