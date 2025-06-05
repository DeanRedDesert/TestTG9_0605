//-----------------------------------------------------------------------
// <copyright file = "HapticCategory.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using CSI.Schemas;
    using CSI.Schemas.Internal;
    using CsiTransport;
    using Foundation.Transport;
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    /// <summary>
    /// Class for handling the haptic devices category.
    /// </summary>
    internal class HapticCategory : CategoryBase<CsiHaptic>, IHaptic
    {
        # region Constructor

        /// <summary>
        /// Conditionally create an instance of the HaptiocCategory.
        /// </summary>
        /// <param name="currentFoundationTarget">The current foundation level.</param>
        /// <returns>An instance of the class if AscentISeriesCds or greater, otherwise null.</returns>
        public static HapticCategory CreateInstance(FoundationTarget currentFoundationTarget)
        {
            // Only supported for foundations equal to or greater than AscentISeriesCds.
            return currentFoundationTarget.IsEqualOrNewer(FoundationTarget.AscentISeriesCds) ?
                new HapticCategory() :
                null;
        }

        #endregion

        #region implementation of CategoryBase base class

        /// <inheritdoc/>
        public override Category Category => Category.CsiHaptic;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => 1;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiHaptic hapticCategoryMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                    "Expected type: {0} Received type: {1}",
                    typeof(CsiHaptic), message.GetType()));
            }
            if(hapticCategoryMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. haptic category message contained no event, request or response.");
            }

            // Currently the haptic category does not have any events.
            throw new UnhandledEventException("Event not handled: " + hapticCategoryMessage.Item.GetType());
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiHaptic hapticCategoryMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                    "Expected type: {0} Received type: {1}",
                    typeof(CsiHaptic), message.GetType()));
            }
            if(hapticCategoryMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Haptic category message contained no event, request or response.");
            }

            // Currently the haptic category does not handle any requests.
            throw new UnhandledRequestException("Request not handled: " + hapticCategoryMessage.Item.GetType());
        }

        #endregion

        #region implementation of IHaptic interface

        /// <inheritdoc/>
        public IEnumerable<HapticDevice> GetAvailableHapticDevices()
        {
            var request = new CsiHaptic { Item = new GetHapticDevicesRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetHapticResponse<GetHapticDevicesResponse>();
            CheckResponse(response.HapticResponse);

            return response.HapticDevices;
        }

        /// <inheritdoc/>
        public HapticDeviceStatus GetHapticDeviceStatus(string deviceId)
        {
            var request = new CsiHaptic { Item = new GetHapticDeviceStatusRequest() { DeviceId = deviceId } };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetHapticResponse<GetHapticDeviceStatusResponse>();
            CheckResponse(response.HapticResponse);

            return response.HapticDeviceStatusResponse.HapticDeviceStatus;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="HapticCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(HapticResponse response)
        {
            if(response.ErrorCode != HapticErrorCode.NONE)
            {
                // most likely reasons are a comm failure or unknown device id.
                throw new HapticCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
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
        private TResponse GetHapticResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        #endregion
    }
}
