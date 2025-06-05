// -----------------------------------------------------------------------
// <copyright file = "IStreamingDeviceInformation.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using Communication.Cabinet;

    /// <summary>
    /// The information for a streaming device.
    /// </summary>
    public interface IStreamingDeviceInformation : IDeviceInformation
    {
        /// <summary>
        /// Device notification events.
        /// </summary>
        event EventHandler<StreamingLightsNotificationEventArgs> NotificationEvent;
    }
}