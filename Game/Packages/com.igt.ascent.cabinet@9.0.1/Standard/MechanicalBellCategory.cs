//-----------------------------------------------------------------------
// <copyright file = "MechanicalBellCategory.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
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
    /// Category for managing the mechanical bell.
    /// </summary>
    internal class MechanicalBellCategory : CategoryBase<CsiMechanicalBell>, IMechanicalBell
    {
        # region Constructor

        /// <summary>
        /// Conditionally create an instance of the MechanicalBellCategory.
        /// </summary>
        /// <param name="currentFoundationTarget">The current foundation level.</param>
        /// <returns>An instance of the class if the foundation target is acceptable, otherwise null.</returns>
        public static MechanicalBellCategory CreateInstance(FoundationTarget currentFoundationTarget)
        {
            // Only supported for the foundation target shown below.
            return (currentFoundationTarget == FoundationTarget.AscentHSeriesCds) ||
                   currentFoundationTarget.IsEqualOrNewer(FoundationTarget.AscentKSeriesCds)
                       ? new MechanicalBellCategory()
                       : null;
        }

        #endregion

        #region CategoryBase<CsiMechanicalBell> Implementation

        /// <inheritdoc/>
        public override Category Category => Category.CsiMechanicalBell;

        /// <inheritdoc/>
        public override ushort VersionMajor => 1;

        /// <inheritdoc/>
        public override ushort VersionMinor => 0;

        /// <inheritdoc/>
        public override void HandleEvent(object message)
        {
            // The mechanical bell doesn't have any events.  Verify that events
            // are in the right category and throw an exception if they're not.
            var mechanicalBellMessage = message as CsiMechanicalBell;

            if(message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if(mechanicalBellMessage == null)
            {
                throw new InvalidMessageException(string.Format(CultureInfo.InvariantCulture,
                                                                "Expected type: {0} Received type: {1}",
                                                                typeof(CsiMechanicalBell),
                                                                message.GetType()));
            }

            if(mechanicalBellMessage.Item == null)
            {
                throw new InvalidMessageException(
                    "Invalid message body. Mechanical bell message contained no event, request or response.");
            }

            throw new UnhandledEventException("Event not handled: " + mechanicalBellMessage.Item.GetType());
        }

        /// <inheritdoc/>
        public override void HandleRequest(object message, ulong requestId)
        {
            // The mechanical bell doesn't have any events. Call the simpler
            // version of HandleRequest() to check the message type.
            HandleEvent(message);
        }

        #endregion

        #region Events

        // Mechanical bell messages currently have no events defined.

        #endregion

        #region IMechanicalBell Implementation

        /// <inheritdoc/>
        public void Ring(string deviceId, uint ringDurationMilliseconds, uint pauseDurationMilliseconds)
        {
            var request = new CsiMechanicalBell
                          {
                              Item = new MechanicalBellRingRequest
                                     {
                                         FeatureId = deviceId,
                                         Count = 1,
                                         RingDurationMilliseconds = ringDurationMilliseconds,
                                         PauseDurationMilliseconds = pauseDurationMilliseconds
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetMechanicalBellResponse<MechanicalBellRingResponse>();
            CheckResponseErrorCode(response.MechanicalBellResponse);
        }

        /// <inheritdoc/>
        public void Ring(string deviceId, uint count, uint ringDurationMilliseconds, uint pauseDurationMilliseconds)
        {
            var request = new CsiMechanicalBell
                          {
                              Item = new MechanicalBellRingRequest
                                     {
                                         FeatureId = deviceId,
                                         Count = count,
                                         RingDurationMilliseconds = ringDurationMilliseconds,
                                         PauseDurationMilliseconds = pauseDurationMilliseconds
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetMechanicalBellResponse<MechanicalBellRingResponse>();
            CheckResponseErrorCode(response.MechanicalBellResponse);
        }

        /// <inheritdoc/>
        public void Stop(string deviceId)
        {
            var request = new CsiMechanicalBell
                          {
                              Item = new MechanicalBellStopRequest
                                     {
                                         FeatureId = deviceId
                                     }
                          };

            Transport.SendRequest(MakeCsiMessageFromRequest(MessageSerializer, Category, request));

            var response = GetMechanicalBellResponse<MechanicalBellStopResponse>();
            CheckResponseErrorCode(response.MechanicalBellResponse);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Wait for a specific response.
        /// </summary>
        /// <typeparam name="TResponse">The type of response to wait for.</typeparam>
        /// <returns>The requested response.</returns>
        /// <exception cref="UnexpectedReplyTypeException">
        /// Thrown when the response is not the type expected.
        /// </exception>
        private TResponse GetMechanicalBellResponse<TResponse>() where TResponse : class
        {
            var response = GetResponse();

            if(!(response.Item is TResponse innerResponse))
            {
                throw new UnexpectedReplyTypeException("Unexpected response.", typeof(TResponse), response.GetType());
            }

            return innerResponse;
        }

        /// <summary>
        /// Check the response error code, and throw an exception if it needs to.
        /// </summary>
        /// <param name="response">The <see cref="MechanicalBellResponse"/> to check.</param>
        /// <remarks>
        /// The error code MechanicalBellErrorCode.NONE is suppressed since it indicates that no
        /// no error occurred. The error code MechanicalBellErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE
        /// is suppressed since the loss of the mechanical bell resource should not break client logic.
        /// </remarks>
        /// <exception cref="MechanicalBellCategoryException">
        /// Thrown if <paramref name="response"/>'s errorCode is not one of the suppressed error codes.
        /// </exception>
        private static void CheckResponseErrorCode(MechanicalBellResponse response)
        {
            var shouldThrow = false;

            switch(response.ErrorCode)
            {
                case MechanicalBellErrorCode.NONE:
                case MechanicalBellErrorCode.UNKNOWN_DRIVER_ERROR:
                case MechanicalBellErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE:
                case MechanicalBellErrorCode.TOO_MANY_PENDING_RING_REQUESTS:
                    break;

                default:
                    shouldThrow = true;
                    break;
            }

            if(shouldThrow)
            {
                throw new MechanicalBellCategoryException(response.ErrorCode.ToString(), response.ErrorDescription);
            }
        }

        #endregion
    }
}
