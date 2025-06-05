//-----------------------------------------------------------------------
// <copyright file = "SolenoidCategory.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
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
    /// Category for managing the handle solenoid.
    /// </summary>
    internal class SolenoidCategory : CategoryBase<CsiSolenoid>, ISolenoid
    {
        #region CategoryBase<CsiSolenoid> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiSolenoid;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => 1;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            // No events currently for Solenoids, but we check anything that arrives to make sure
            // it's in the right category. And since there are no event handlers for solenoids,
            // we throw an exception anyways.
            var solenoidMessage = message as CsiSolenoid;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(solenoidMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiSolenoid), message.GetType()));
            }

            if(solenoidMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Solenoid message contained no event, request or response.");
            }

            throw new UnhandledEventException("Event not handled: " + solenoidMessage.Item.GetType());
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            var solenoidMessage = message as CsiSolenoid;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if(solenoidMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiSolenoid), message.GetType()));
            }
            if(solenoidMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Solenoid message contained no event, request or response.");
            }

            throw new UnhandledRequestException("Request not handled: " + solenoidMessage.Item.GetType());
        }

        #endregion CategoryBase<CsiSolenoid> Implementation

        #region Events

        // Solenoid messages currently have no events defined.

        #endregion Events

        #region ISolenoid Implementation

        /// <inheritdoc/>
        public void SetStateToLocked()
        {
            var request = new CsiSolenoid {
                                    Item = new SolenoidRequest
                                    {
                                        SolenoidActionType = SolenoidActionType.Lock,
                                        SolenoidActionState = true
                                    }};

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            CheckResponse(GetSolenoidResponse<SolenoidResponse>());
        }

        /// <inheritdoc/>
        public void SetStateToUnlocked()
        {
            var request = new CsiSolenoid {
                                    Item = new SolenoidRequest
                                    {
                                        SolenoidActionType = SolenoidActionType.Lock,
                                        SolenoidActionState = false
                                    }};

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            CheckResponse(GetSolenoidResponse<SolenoidResponse>());
        }

        /// <inheritdoc/>
        public void ClickSolenoid()
        {
            var request = new CsiSolenoid {
                                    Item = new SolenoidRequest {
                                        SolenoidActionType = SolenoidActionType.Click,
                                        SolenoidActionState = true
                                    }};

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));
            CheckResponse(GetSolenoidResponse<SolenoidResponse>());
        }

        #endregion ISolenoid Implementation

        #region Private Methods

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetSolenoidResponse<TResponse>() where TResponse : class
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
        /// <exception cref="SolenoidCategoryException">Thrown if the response indicates that there was an error.</exception>
        /// <remarks>Need to ask for the CSI to be updated to have different names for the responses.</remarks>
        private static void CheckResponse(SolenoidResponse response)
        {
            if(response.ErrorCode != SolenoidErrorCode.NONE)
            {
                throw new SolenoidCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        #endregion Private Methods
    }
}
