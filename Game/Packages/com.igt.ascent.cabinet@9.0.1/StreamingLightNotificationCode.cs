//-----------------------------------------------------------------------
// <copyright file = "StreamingLightNotificationCode.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Notification code from the streaming light device.
    /// </summary>
    public enum StreamingLightNotificationCode
    {

        /// <remarks/>
        None,

        /// <remarks/>
        UnknownDriverError,

        /// <remarks/>
        InvalidFeatureId,

        /// <remarks/>
        InvalidLightGroup,

        /// <remarks/>
        FileNotFound,

        /// <remarks/>
        InvalidSequence,

        /// <remarks/>
        ClientDoesNotOwnResource,

        /// <remarks/>
        QueueEmpty,

        /// <remarks/>
        SequenceComplete,

        /// <remarks/>
        DeviceInTiltState,

        /// <remarks/>
        QueueFull,

        /// <remarks/>
        InvalidFrame,

        /// <remarks/>
        InvalidCommand
    }
}
