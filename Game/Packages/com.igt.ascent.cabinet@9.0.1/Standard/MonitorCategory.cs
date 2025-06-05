//-----------------------------------------------------------------------
// <copyright file = "MonitorCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Cabinet;
    using CsiTransport;
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using Foundation.Transport;

    /// <summary>
    /// Class for handling the CSI monitor category.
    /// </summary>
    internal class MonitorCategory : CategoryBase<CsiMonitor>, IMonitor
    {
        #region Private Fields

        /// <summary>
        /// The <see cref="FoundationTarget"/> that this <see cref="MonitorCategory"/> was constructed with.
        /// </summary>
        private readonly FoundationTarget foundationTarget;

        #endregion Private Fields

        #region Constructors

        /// <summary>
        /// Construct an instance of the Monitor category.
        /// </summary>
        public MonitorCategory(FoundationTarget currentFoundationTarget)
        {
            foundationTarget = currentFoundationTarget;
        }

        #endregion Constructors

        #region CategoryBase<CsiMonitor> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiMonitor;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor
        {
            get
            {
                if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentR2Series))
                {
                    return 6;
                }
                else if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentQ3Series))
                {
                    return 5;
                }
                else if(foundationTarget.IsEqualOrNewer(FoundationTarget.AscentQ1Series))
                {
                    return 3;
                }

                // Version 1.2 is only leveraged by emulated media controller which does not use SDK
                return 1;
            }
        }

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiMonitor monitorMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiMonitor),
                                                                message.GetType()));
            }

            if(monitorMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Monitor message contained no event, request or response.");
            }

            // Currently the monitor category does not have any events.
            throw new UnhandledEventException("Event not handled: " + monitorMessage.Item.GetType());
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiMonitor monitorMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiMonitor),
                                                                message.GetType()));
            }

            if(monitorMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Monitor message contained no event, request or response.");
            }

            // Currently the monitor category does not handle any requests.
            throw new UnhandledRequestException("Request not handled: " + monitorMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiMonitor> Implementation

        #region IMonitor Implementation

        /// <inheritdoc/>
        public MonitorComposition GetComposition()
        {
            var request = new CsiMonitor { Item = new GetConfigurationRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<GetConfigurationResponse>();
            CheckResponse(response.MonitorResponse);

            // Unity does not natively support dual GPUs. Spanning windows across GPUs will
            // severely impact performance. Thus only expose monitors attached to the EGM's
            // primary GPU.
            var primaryGpuMonitors = new List<Monitor>();
            foreach(var monitor in response.MonitorConfiguration)
            {
                if(monitor.ParentGpu == null || monitor.ParentGpu.PrimaryAdapter)
                {
                    primaryGpuMonitors.Add(monitor);
                }
            }

            return new MonitorComposition(primaryGpuMonitors, response.Desktop);
        }

        /// <inheritdoc/>
        public void SetColorProfile(string deviceId, ColorProfileSetting setting)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            if(setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }

            var request = new CsiMonitor
                          {
                              Item = new SetColorProfileRequest
                                     {
                                         ColorProfile = setting.ProfileType,
                                         DeviceId = deviceId,
                                         ProfilePath = setting.ProfilePath
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<SetColorProfileResponse>();
            CheckResponse(response.MonitorResponse);
        }

        /// <inheritdoc/>
        public ColorProfileSetting GetActiveColorProfile(string deviceId)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", paramName: nameof(deviceId));
            }

            var request = new CsiMonitor { Item = new GetCurrentColorProfileRequest { DeviceId = deviceId } };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<GetCurrentColorProfileResponse>();
            CheckResponse(response.MonitorResponse);

            return response.ColorProfile == ColorProfile.Custom
                       ? new ColorProfileSetting(response.ProfilePath)
                       : new ColorProfileSetting(response.ColorProfile);
        }

        /// <inheritdoc/>
        public void EnableStereoscopyDisplay(string deviceId)
        {
            if(VersionMinor < 1)
            {
                throw new MonitorCategoryException("Feature not supported",
                                                   "Stereoscopic Monitor must be accessed using the monitor category version 1.1 or newer.");
            }

            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            var request = new CsiMonitor { Item = new EnableStereoscopyRequest { DeviceId = deviceId } };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<EnableStereoscopyResponse>();
            CheckResponse(response.MonitorResponse);
        }

        /// <inheritdoc/>
        public void DisableStereoscopyDisplay(string deviceId)
        {
            if(VersionMinor < 1)
            {
                throw new MonitorCategoryException("Feature not supported",
                                                   "Stereoscopic Monitor must be accessed using the monitor category version 1.1 or newer.");
            }

            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            var request = new CsiMonitor { Item = new DisableStereoscopyRequest { DeviceId = deviceId } };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<DisableStereoscopyResponse>();
            CheckResponse(response.MonitorResponse);
        }

        /// <inheritdoc/>
        public StereoscopyState GetStereoscopyDisplayState(string deviceId)
        {
            if(VersionMinor < 1)
            {
                throw new MonitorCategoryException("Feature not supported",
                                                   "Stereoscopic Monitor must be accessed using the monitor category version 1.1 or newer.");
            }

            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            var request = new CsiMonitor { Item = new GetStereoscopyStateRequest { DeviceId = deviceId } };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<GetStereoscopyStateResponse>();
            CheckResponse(response.MonitorResponse);

            return response.State;
        }

        /// <inheritdoc/>
        public void SetTransmissiveSupport(string deviceId, Cabinet.TransmissiveSupport transmissiveSupport)
        {
            if(VersionMajor >= 2 || VersionMinor >= 3)
            {
                throw new MonitorCategoryException("Feature not supported",
                                                   $"Transmissive support requires CsiMonitor v1.3 or newer. You are using version {VersionMajor}.{VersionMinor}." +
                                                   " Please refer to the documentation to determine the minimum foundation to target.");
            }

            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            var request = new CsiMonitor
                          {
                              Item = new SetTransmissiveSupportRequest
                                     {
                                         DeviceId = deviceId,
                                         Support = (CSI.Schemas.Internal.TransmissiveSupport)transmissiveSupport
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<SetTransmissiveSupportResponse>();
            CheckResponse(response.MonitorResponse);
        }

        /// <inheritdoc/>
        public MonitorRole GetPreferredUIDisplay()
        {
            if((VersionMajor < 1) ||
               ((VersionMajor == 1) && (VersionMinor < 5)))
            {
                throw new MonitorCategoryException("Feature not supported",
                                                   "GetPreferredUIDisplay must be accessed using the monitor category version 1.5 or newer.");
            }

            var request = new CsiMonitor { Item = new GetPreferredUIDisplayRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetMonitorResponse<GetPreferredUIDisplayResponse>();
            CheckResponse(response.MonitorResponse);
            return response.Role;
        }

        #endregion IMonitor Implementation

        #region Private Methods

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="MonitorCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(MonitorResponse response)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case MonitorErrorCode.NONE:
                case MonitorErrorCode.COLOR_PROFILE_NOT_SUPPORTED:
                case MonitorErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                throw new MonitorCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetMonitorResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        #endregion Private Methods
    }
}
