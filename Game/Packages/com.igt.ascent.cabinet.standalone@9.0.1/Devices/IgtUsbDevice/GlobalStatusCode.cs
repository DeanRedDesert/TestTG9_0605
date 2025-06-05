//-----------------------------------------------------------------------
// <copyright file = "GlobalStatusCode.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Status code applicable to all devices.
    /// </summary>
    [Serializable]
    internal enum GlobalStatusCode : ushort
    {
        #region Statuses

        /// <summary>
        /// Normal status.  Not tilted,
        /// not in or exiting self test,
        /// not disabled.
        /// </summary>
        Normal = 0x7FFF,

        /// <summary>
        /// Self test is in progress.
        /// May occur while tilted.
        /// </summary>
        SelfTestInProgress = 0x7FFE,

        /// <summary>
        /// Self test has exited.
        /// </summary>
        SelfTestExited = 0x7FFD,

        #endregion

        #region Tilts

        /// <summary>
        /// Tilted because of a communication timeout.
        /// </summary>
        CommunicationTimedOut = 0xFFFF,

        /// <summary>
        /// Tilted because the device did not receive a
        /// sequence message processed acknowledgement.
        /// </summary>
        SequenceMessageAckNotReceived = 0xFFFE,

        #endregion

    }
}
