//-----------------------------------------------------------------------
// <copyright file = "DepthCameraCategory.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;
    using CSI.Schemas;

    /// <summary>
    /// Class for handling the CSI depth camera category.
    /// </summary>
    internal class DepthCameraCategory : CategoryBase<CsiDepthCamera>, IDepthCamera
    {
        #region Constructors

        /// <summary>
        /// Conditionally create an instance of the class.
        /// </summary>
        /// <returns>An instance of the class.</returns>
        public static DepthCameraCategory CreateInstance()
        {
            return new DepthCameraCategory();
        }

        #endregion Constructors

        #region CategoryBase<CsiDepthCamera> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiDepthCamera;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => 0;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiDepthCamera depthCameraMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                    "Expected type: {0} Received type: {1}",
                    typeof(CsiDepthCamera), message.GetType()));
            }
            if(depthCameraMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Depth camera message contained no event, request or response.");
            }

            //Currently the depth camera category does not have any events.
            throw new UnhandledEventException("Event not handled: " + depthCameraMessage.Item.GetType());
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiDepthCamera depthCameraMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                    "Expected type: {0} Received type: {1}",
                    typeof(CsiDepthCamera), message.GetType()));
            }
            if(depthCameraMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Depth camera message contained no event, request or response.");
            }

            //Currently the depth camera category does not handle any requests.
            throw new UnhandledRequestException("Request not handled: " + depthCameraMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiDepthCamera> Implementation

        #region IDepthCamera Implementation

        /// <inheritdoc/>
        public IEnumerable<DepthCameraDevice> GetAvailableDepthCameras()
        {
            var request = new CsiDepthCamera { Item = new GetDepthCameraDevicesRequest() };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetDepthCameraResponse<GetDepthCameraDevicesResponse>();
            CheckResponse(response.DepthCameraResponse);

            return response.DepthCameraDevices;
        }

        #endregion IDepthCamera Implementation

        #region Private Methods

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="DepthCameraCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(DepthCameraResponse response)
        {
            if(response.ErrorCode != DepthCameraErrorCode.NONE)
            {
                throw new DepthCameraCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
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
        private TResponse GetDepthCameraResponse<TResponse>() where TResponse : class
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
