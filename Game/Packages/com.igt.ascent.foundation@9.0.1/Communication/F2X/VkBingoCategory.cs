//-----------------------------------------------------------------------
// <copyright file = "VkBingoCategory.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
// <auto-generated>
//     This code was generated by C3G.
//
//     Changes to this file may cause incorrect behavior
//     and will be lost if the code is regenerated.
// </auto-generated>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2X
{
    using System;
    using System.Collections.Generic;
    using F2XTransport;
    using Schemas.Internal.Types;
    using Schemas.Internal.VKBingo;
    using Version = Schemas.Internal.Types.Version;

    /// <summary>
    /// Implementation of the F2X <see cref="VKBingo"/> category.
    /// Video King Bingo / VK Bingo category of messages.
    /// Category: 3020; Major Version: 1
    /// </summary>
    public class VkBingoCategory : F2XTransactionalCategoryBase<VKBingo>, IVkBingoCategory, IMultiVersionSupport
    {
        #region Fields

        /// <summary>
        /// Object which implements the VkBingoCategory callbacks.
        /// </summary>
        private readonly IVkBingoCategoryCallbacks callbackHandler;

        private const string MethodPlayerCashoutRequest = "PlayerCashoutRequest";

        /// <summary>
        /// A look-up table for the methods that are NOT available in all supported versions.
        /// Keyed by the method name, the value is the version where the method becomes available.
        /// </summary>
        private readonly Dictionary<string, Version> methodSupportingVersions = new Dictionary<string, Version>
        {
            { MethodPlayerCashoutRequest, new Version(1, 1) }
        };

        /// <summary>
        /// All versions supported by this category class.
        /// </summary>
        private readonly List<Version> supportedVersions = new List<Version>
        {
            new Version(1, 0),
            new Version(1, 1)
        };

        /// <summary>
        /// The version to use for communications by this category.
        /// Initialized to 0.0. Will be set by <see cref="SetVersion"/>.
        /// </summary>
        private Version effectiveVersion = new Version(0, 0);

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="VkBingoCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// VkBingoCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public VkBingoCategory(IF2XTransport transport, IVkBingoCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));
            AddMessagehandler<DisplayStateChangedSend>(HandleDisplayStateChanged);
            AddMessagehandler<GamePlayEndMeteringResponseSend>(HandleGamePlayEndMeteringResponse);
            AddMessagehandler<LockOutResponseSend>(HandleLockOutResponse);
            AddMessagehandler<UnLockResponseSend>(HandleUnLockResponse);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.VkBingo;

        /// <inheritdoc/>
        public override uint MajorVersion => effectiveVersion.MajorVersion;

        /// <inheritdoc/>
        public override uint MinorVersion => effectiveVersion.MinorVersion;

        #endregion

        #region IMultiVersionSupport Members

        /// <inheritdoc/>
        public void SetVersion(uint major, uint minor)
        {
            var version = new Version(major, minor);

            if(!supportedVersions.Contains(version))
            {
                throw new ArgumentException(
                    $"{version} is not supported by VkBingoCategory class.");
            }

            effectiveVersion = version;
        }

        #endregion

        #region IVkBingoCategory Members

        /// <inheritdoc/>
        public bool PlacePurchase(PurchaseRequestState requestState, Amount amount)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<PlacePurchaseSend>();
            var content = (PlacePurchaseSend)request.Message.Item;
            content.RequestState = requestState;
            content.Amount = amount;

            var reply = SendMessageAndGetReply<PlacePurchaseReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.Accepted;
        }

        /// <inheritdoc/>
        public bool PlayerCashoutRequest()
        {
            if(IsMethodSupported(MethodPlayerCashoutRequest))
            {
                Transport.MustHaveHeavyweightTransaction();
                var request = CreateTransactionalRequest<PlayerCashoutRequestSend>();

                var reply = SendMessageAndGetReply<PlayerCashoutRequestReply>(Channel.Foundation, request);
                CheckReply(reply.Exception);
                return reply.Content.Accepted;
            }

            return default(bool);
        }

        /// <inheritdoc/>
        public bool RequestGamePlayEndMetering(GamePlayOutcomeType winStatus, Amount amount)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<RequestGamePlayEndMeteringSend>();
            var content = (RequestGamePlayEndMeteringSend)request.Message.Item;
            content.WinStatus = winStatus;
            content.Amount = amount;

            var reply = SendMessageAndGetReply<RequestGamePlayEndMeteringReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.Accepted;
        }

        /// <inheritdoc/>
        public bool RequestLockOut(string accountNumber)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<RequestLockOutSend>();
            var content = (RequestLockOutSend)request.Message.Item;
            content.AccountNumber = accountNumber;

            var reply = SendMessageAndGetReply<RequestLockOutReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.Accepted;
        }

        /// <inheritdoc/>
        public bool RequestUnLock(UnLockingReason reason, string accountNumber)
        {
            Transport.MustHaveHeavyweightTransaction();
            var request = CreateTransactionalRequest<RequestUnLockSend>();
            var content = (RequestUnLockSend)request.Message.Item;
            content.Reason = reason;
            content.AccountNumber = accountNumber;

            var reply = SendMessageAndGetReply<RequestUnLockReply>(Channel.Foundation, request);
            CheckReply(reply.Exception);
            return reply.Content.Accepted;
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the DisplayStateChangedSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleDisplayStateChanged(DisplayStateChangedSend message)
        {
            var errorMessage = callbackHandler.ProcessDisplayStateChanged(message.DisplayState);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<DisplayStateChangedReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        /// <summary>
        /// Handler for the GamePlayEndMeteringResponseSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleGamePlayEndMeteringResponse(GamePlayEndMeteringResponseSend message)
        {
            var errorMessage = callbackHandler.ProcessGamePlayEndMeteringResponse(message.Status);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<GamePlayEndMeteringResponseReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        /// <summary>
        /// Handler for the LockOutResponseSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleLockOutResponse(LockOutResponseSend message)
        {
            var errorMessage = callbackHandler.ProcessLockOutResponse(message.AccountNumber, message.Success);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<LockOutResponseReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        /// <summary>
        /// Handler for the UnLockResponseSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleUnLockResponse(UnLockResponseSend message)
        {
            var errorMessage = callbackHandler.ProcessUnLockResponse(message.AccountNumber, message.Reason, message.Success);
            var errorCode = string.IsNullOrEmpty(errorMessage) ? 0 : 1;
            var replyMessage = CreateReply<UnLockResponseReply>(errorCode, errorMessage);
            SendFoundationChannelResponse(replyMessage);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if a method is supported by the effective version of the category.
        /// </summary>
        /// <param name="methodName">
        /// The name of the method to check.
        /// </param>
        /// <returns>
        /// True if the method is supported. False otherwise.
        /// </returns>
        private bool IsMethodSupported(string methodName)
        {
            // Methods not in the dictionary are available in all versions.
            var result = true;

            if(methodSupportingVersions.ContainsKey(methodName))
            {
                result = effectiveVersion >= methodSupportingVersions[methodName];
            }

            return result;
        }

        #endregion

    }

}

