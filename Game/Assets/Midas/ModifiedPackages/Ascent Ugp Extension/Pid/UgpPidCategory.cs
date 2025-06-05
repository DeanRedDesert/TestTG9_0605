//-----------------------------------------------------------------------
// <copyright file = "UgpPidCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using F2X;
    using F2XTransport;
    using F2X.Schemas.Internal.Types;

    /// <summary>
    /// Implementation of the F2X UgpPid category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpPidCategory : F2XTransactionalCategoryBase<UgpPidCategoryInternal>, IUgpPidCategory
    {
        #region Fields

        /// <summary>
        /// Object which implements the UgpPidCategory callbacks.
        /// </summary>
        private readonly IUgpPidCategoryCallbacks callbackHandler;

        #endregion

        #region Constructor

        /// <summary>
        /// Instantiates a new <see cref="UgpPidCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// UgpPidCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public UgpPidCategory(IF2XTransport transport, IUgpPidCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));

            // Configure the handler table for all messages this category can handle.
            AddMessagehandler<UgpPidCategoryForceActivationSend>(HandlePidActivation);
            AddMessagehandler<UgpPidCategoryConfigurationChangedSend>(HandlePidConfigurationChanged);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the UgpPidCategoryConfigurationChangedSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandlePidConfigurationChanged(UgpPidCategoryConfigurationChangedSend message)
        {
            callbackHandler.ProcessPidConfigurationChanged();

            var reply = CreateReply<UgpPidCategoryConfigurationChangedReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        /// <summary>
        /// Handler for the UgpPidCategoryForceActivationSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandlePidActivation(UgpPidCategoryForceActivationSend message)
        {
            callbackHandler.ProcessPidActivation(message.IsActive);

            var reply = CreateReply<UgpPidCategoryForceActivationReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpPid;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 0;

        #endregion

        #region IUgpPidCategory Implementation

        /// <inheritdoc/>
        public bool IsServiceRequested { get; private set; }

        /// <inheritdoc/>
        public void StartTracking()
        {
            var request = CreateBasicRequest<UgpPidCategoryStartTrackingSend>();
            var reply = SendMessageAndGetReply<UgpPidCategoryStartTrackingReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void StopTracking()
        {
            var request = CreateBasicRequest<UgpPidCategoryStopTrackingSend>();
            var reply = SendMessageAndGetReply<UgpPidCategoryStopTrackingReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public PidSessionDataInfo GetSessionData()
        {
            var request = CreateBasicRequest<UgpPidCategoryGetSessionDataSend>();
            var reply = SendMessageAndGetReply<UgpPidCategoryGetSessionDataReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });

            var pidSessionData = new PidSessionDataInfo
            {
                IsSessionTrackingActive = reply.IsTracking,
                CreditMeterAtStart = reply.AvailableCashAtStart.Value,
                AvailableCredits = reply.AvailableCash.Value,
                CashIn = reply.MoneyIn.Value,
                CashOut = reply.MoneyOut.Value,
                CreditsPlayed = reply.Played.Value,
                CreditsWon = reply.Won.Value,
                SessionWinOrLoss = reply.SessionWinOrLoss.Value,
                IsWinningSession = reply.IsWinningSession,
                IsCrown = reply.IsCrown,
                SessionStarted = new DateTime(reply.TimeStartedTick),
                SessionDuration = TimeSpan.FromTicks(reply.TotalPlayedTicks)
            };

            return pidSessionData;
        }

        /// <inheritdoc/>
        public PidConfigurationInfo GetPidConfiguration()
        {
            var request = CreateBasicRequest<UgpPidCategoryGetPidConfigurationSend>();
            var reply = SendMessageAndGetReply<UgpPidCategoryGetPidConfigurationReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });

            var pidConfiguration = new PidConfigurationInfo
            {
                IsMainEntryEnabled = reply.IsMainEntryEnabled,
                IsRequestServiceEnabled = reply.IsRequestServiceEnabled,
                GameInformationDisplayStyle = reply.GameInformationDisplayStyle,
                IsRequestServiceActivated = reply.IsRequestServiceActivated,
                SessionTrackingOption = reply.SessionTrackingOption,
                IsGameRulesEnabled = reply.IsGameRulesEnabled,
                InformationMenuTimeout = new TimeSpan(reply.InformationMenuTimeoutTicks),
                SessionStartMessageTimeout = new TimeSpan(reply.SessionStartMessageTimeoutTicks),
                ViewSessionScreenTimeout = new TimeSpan(reply.ViewSessionScreenTimeoutTicks),
                ViewGameInformationTimeout = new TimeSpan(reply.ViewGameInformationTimeoutTicks),
                ViewGameRulesTimeout = new TimeSpan(reply.ViewGameRulesTimeoutTicks),
                ViewPayTableTimeout = new TimeSpan(reply.ViewPayTableTimeoutTicks),
                SessionTimeoutInterval = new TimeSpan(reply.SessionTimeoutIntervalTicks),
                SessionTimeoutStartOnZeroCredits = reply.SessionTimeoutStartOnZeroCredits,
                TotalNumberLinkEnrolments = reply.TotalNumberLinkEnrolments,
                TotalLinkPercentageContributions = reply.TotalLinkPercentageContributions,
                ShowLinkJackpotCount = reply.ShowLinkJackpotCount,
                LinkRtpForGameRtp = reply.LinkRTPForGameRTP
            };

            IsServiceRequested = pidConfiguration.IsRequestServiceActivated;

            return pidConfiguration;
        }

        /// <inheritdoc/>
        public void ActivationStatusChanged(bool currentStatus)
        {
            var request = CreateBasicRequest<UgpPidCategoryActivationStatusChangedSend>();
            var messageItem = (UgpPidCategoryActivationStatusChangedSend)request.Message.Item;
            messageItem.IsActive = currentStatus;
            var reply = SendMessageAndGetReply<UgpPidCategoryActivationStatusChangedReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void GameInformationScreenEntered()
        {
            var request = CreateBasicRequest<UgpPidCategoryGameInformationScreenEnteredSend>();
            var reply = SendMessageAndGetReply<UgpPidCategoryGameInformationScreenEnteredReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void SessionInformationScreenEntered()
        {
            var request = CreateBasicRequest<UgpPidCategorySessionInformationScreenEnteredSend>();
            var reply = SendMessageAndGetReply<UgpPidCategorySessionInformationScreenEnteredReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        /// <inheritdoc/>
        public void AttendantServiceRequested()
        {
            var request = CreateBasicRequest<UgpPidCategoryAttendantServiceRequestedSend>();
            var reply = SendMessageAndGetReply<UgpPidCategoryAttendantServiceRequestedReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
            IsServiceRequested = !IsServiceRequested;

            callbackHandler.NotifyAttendantServiceRequested(IsServiceRequested);
        }

        /// <inheritdoc/>
        public void RequestForcePayout()
        {
            var request = CreateBasicRequest<UgpPidCategoryRequestForcePayoutSend>();
            var reply = SendMessageAndGetReply<UgpPidCategoryRequestForcePayoutReply>(Channel.Foundation, request);
            CheckReply(new ReplyException
            {
                ErrorCode = reply.Reply.ReplyCode,
                ErrorDescription = reply.Reply.ErrorDescription
            });
        }

        #endregion
    }
}