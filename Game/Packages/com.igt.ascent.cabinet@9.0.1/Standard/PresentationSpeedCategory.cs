//-----------------------------------------------------------------------
// <copyright file = "PresentationSpeedCategory.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using System.Globalization;
    using CSI.Schemas.Internal;
    using Foundation.Transport;
    using CsiTransport;

    /// <summary>
    /// Category for managing presentation speed.
    /// </summary>
    internal class PresentationSpeedCategory : CategoryBase<CsiPresentationSpeed>, IPresentationSpeed
    {
        #region Constructors

        /// <summary>
        /// Conditionally create an instance of the class.
        /// </summary>
        /// <param name="currentFoundationTarget">The current foundation target.</param>
        /// <returns>An instance of the class.</returns>
        public static PresentationSpeedCategory CreateInstance(FoundationTarget currentFoundationTarget)
        {
            return new PresentationSpeedCategory();
        }

        #endregion Constructors

        #region CategoryBase<CsiPresentationSpeed> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiPresentationSpeed;

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

            if(!(message is CsiPresentationSpeed csiPresentationSpeedMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiPresentationSpeed), message.GetType()));
            }
            if(csiPresentationSpeedMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Presentation speed message contained no event, request or response.");
            }

            //Currently the presentation speed category does not have any events.
            throw new UnhandledEventException("Event not handled: " + csiPresentationSpeedMessage.Item.GetType());
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(!(message is CsiPresentationSpeed csiPresentationSpeedMessage))
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiPresentationSpeed), message.GetType()));
            }
            if(csiPresentationSpeedMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Presentation speed message contained no event, request or response.");
            }

            //Currently the presentation speed category does not handle any requests.
            throw new UnhandledRequestException("Request not handled: " + csiPresentationSpeedMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiPresentationSpeed> Implementation

        #region IPresentationSpeed Implementation

        /// <inheritdoc/>
        public PresentationSpeedInfo GetPresentationSpeedInfo()
        {
            var request = new CsiPresentationSpeed { Item = new GetPresentationSpeedRequest() };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPresentationSpeedResponse<GetPresentationSpeedResponse>();
            CheckResponse(response.PresentationSpeedResponse);
            return new PresentationSpeedInfo(response.DefaultPresentationSpeed, response.PresentationSpeed);
        }

        /// <inheritdoc/>
        public void SetPresentationSpeed(uint presentationSpeed)
        {
            // restrict the set level between 0 and 100
            presentationSpeed = presentationSpeed > 100 ? 100 : presentationSpeed;

            var request = new CsiPresentationSpeed
            {
                Item = new SetPresentationSpeedRequest
                {
                    PresentationSpeed = presentationSpeed
                }
            };
            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            var response = GetPresentationSpeedResponse<SetPresentationSpeedResponse>();
            CheckResponse(response.PresentationSpeedResponse);
        }

        #endregion IPresentationSpeed Implementation

        #region Private Methods

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetPresentationSpeedResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Check the response to see if there are any errors.
        /// </summary>
        /// <param name="response">The response to check.</param>
        /// <exception cref="PresentationSpeedCategoryException">
        /// Thrown if the response indicates that there was an error.
        /// </exception>
        private static void CheckResponse(PresentationSpeedResponse response)
        {
            if(response.ErrorCode != PresentationSpeedErrorCode.NONE)
            {
                throw new PresentationSpeedCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        #endregion Private Methods
    }
}
