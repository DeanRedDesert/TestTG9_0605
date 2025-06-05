//-----------------------------------------------------------------------
// <copyright file = "UgpMachineConfigurationCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    using System;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// Implementation of the F2X UgpMachineConfiguration category.
    /// </summary>
    [DisableCodeCoverageInspection]
    internal class UgpMachineConfigurationCategory : F2XTransactionalCategoryBase<UgpMachineConfigurationCategoryInternal>,
                                                     IUgpMachineConfigurationCategory
    {
        #region Private Fields

        /// <summary>
        /// Object which implements the UgpMachineConfigurationCategory callbacks.
        /// </summary>
        private readonly IUgpMachineConfigurationCategoryCallbacks callbackHandler;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="UgpMachineConfigurationCategory"/>.
        /// </summary>
        /// <param name="transport">
        /// Transport that this category will be installed in.
        /// </param>
        /// <param name="callbackHandler">
        /// UgpMachineConfigurationCategory callback handler.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="callbackHandler"/> is null.
        /// </exception>
        public UgpMachineConfigurationCategory(IF2XTransport transport,
                                               IUgpMachineConfigurationCategoryCallbacks callbackHandler)
            : base(transport)
        {
            this.callbackHandler = callbackHandler ?? throw new ArgumentNullException(nameof(callbackHandler));

            AddMessagehandler<UgpMachineConfigurationCategorySetParametersSend>(HandleSetParametersSend);
        }

        #endregion

        #region Message Handlers

        /// <summary>
        /// Handler for the UgpMachineConfigurationCategorySetParametersSend message.
        /// </summary>
        /// <param name="message">
        /// Message from the Foundation to handle.
        /// </param>
        private void HandleSetParametersSend(UgpMachineConfigurationCategorySetParametersSend message)
        {
			var parameters = new MachineConfigurationParameters(
				message.IsClockVisible,
				message.ClockFormat,
				message.Tokenisation,
				message.GameCycleTime,
				message.ContinuousPlayAllowed,
				message.FeatureAutoStartEnabled,
				message.CurrentMaximumBet,
				message.SlamSpinAllowed,
				message.WinCapStyle.ToPublic(),
				message.QcomJurisdiction,
				message.CabinetId,
				message.BrainboxId,
				message.Gpu);

			callbackHandler.ProcessSetParameters(parameters);

            var reply = CreateReply<UgpMachineConfigurationCategorySetParametersReply>(0, "");
            SendFoundationChannelResponse(reply);
        }

        #endregion

        #region IApiCategory Members

        /// <inheritdoc/>
        public override MessageCategory Category => MessageCategory.UgpMachineConfiguration;

        /// <inheritdoc/>
        public override uint MajorVersion => 1;

        /// <inheritdoc/>
        public override uint MinorVersion => 1;

        #endregion

        #region IUgpMachineConfigurationCategory Members

        /// <inheritdoc/>
		public MachineConfigurationParameters GetMachineConfigurationParameters()
		{
			var request = CreateBasicRequest<UgpMachineConfigurationCategoryGetParametersSend>();
			var r = SendMessageAndGetReply<UgpMachineConfigurationCategoryGetParametersReply>(Channel.Foundation, request);
			return new MachineConfigurationParameters(r.IsClockVisible, r.ClockFormat, r.Tokenisation,
				r.GameCycleTime, r.ContinuousPlayAllowed, r.FeatureAutoStartEnabled, r.CurrentMaximumBet,
				r.SlamSpinAllowed, r.WinCapStyle.ToPublic(), r.QcomJurisdiction, r.CabinetId, r.BrainboxId, r.Gpu);
		}

        #endregion
    }
}