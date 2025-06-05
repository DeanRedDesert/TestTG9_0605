// -----------------------------------------------------------------------
// <copyright file = "F2XPlayerSessionParameters.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.PlayerSessionParameters;
    using Interfaces;

    /// <summary>
    /// Implementation of the extended interface of Player Session Parameters
    /// that is backed by the F2X.
    /// </summary>
    internal class F2XPlayerSessionParameters : IPlayerSessionParameters, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// Interface for validating transactions.
        /// </summary>
        private readonly ITransactionWeightVerificationDependency transactionVerification;

        /// <summary>
        /// The cached Player Session Parameters category instance
        /// used to communicate with the Foundation.
        /// </summary>
        private readonly IPlayerSessionParametersCategory playerSessionParametersCategory;

        /// <summary>
        /// The interface for querying game mode.
        /// </summary>
        private readonly IGameModeQuery gameModeQuery;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="F2XPlayerSessionParameters"/>.
        /// </summary>
        /// <param name="playerSessionParametersCategory">
        /// Player Session Parameters category instance used to communicate with the Foundation.
        /// </param>
        /// <param name="transactionVerification">
        /// The interface to use for validating transactions.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// The interface for processing a transactional event.
        /// </param>
        /// <param name="layeredContextActivationEvents">
        /// The interface for providing context events.
        /// </param>
        /// <param name="gameModeQuery">
        /// The interface for querying game mode.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XPlayerSessionParameters(IPlayerSessionParametersCategory playerSessionParametersCategory,
                                          ITransactionWeightVerificationDependency transactionVerification,
                                          IEventDispatcher transactionalEventDispatcher,
                                          ILayeredContextActivationEventsDependency layeredContextActivationEvents,
                                          IGameModeQuery gameModeQuery)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }
            if(layeredContextActivationEvents == null)
            {
                throw new ArgumentNullException(nameof(layeredContextActivationEvents));
            }

            this.playerSessionParametersCategory = playerSessionParametersCategory ?? throw new ArgumentNullException(nameof(playerSessionParametersCategory));
            this.transactionVerification = transactionVerification ?? throw new ArgumentNullException(nameof(transactionVerification));
            this.gameModeQuery = gameModeQuery ?? throw new ArgumentNullException(nameof(gameModeQuery));

            transactionalEventDispatcher.EventDispatchedEvent += HandleCurrentResetParametersChangedEvent;
            layeredContextActivationEvents.ActivateLayeredContextEvent += HandleActivateThemeContextEvent;

            PendingParametersToReset = new List<PlayerSessionParameterType>();
        }

        #endregion

        #region IPlayerSessionParameters Members

        /// <inheritdoc />
        public event EventHandler<CurrentResetParametersChangedEventArgs> CurrentResetParametersChangedEvent;

        /// <inheritdoc/>
        public bool IsPlayerSessionParameterResetEnabled { get; private set; }

        /// <inheritdoc/>
        public IList<PlayerSessionParameterType> PendingParametersToReset { get; private set; }

        /// <inheritdoc/>
        public void ReportParametersBeingReset(IEnumerable<PlayerSessionParameterType> parametersBeingReset)
        {
            transactionVerification.MustHaveHeavyweightTransaction();

            playerSessionParametersCategory.ParametersReset(
                parametersBeingReset.Select(item => (SessionParameterType)item));
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Handles the dispatched event if the dispatched event is CurrentResetParametersChanged event.
        /// </summary>
        /// <param name="sender">
        /// The sender of the dispatched event.
        /// </param>
        /// <param name="dispatchedEventArgs">
        /// The arguments used for processing the dispatched event.
        /// </param>
        private void HandleCurrentResetParametersChangedEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEventType == typeof(CurrentResetParametersChangedEventArgs))
            {
                if(dispatchedEventArgs.DispatchedEvent is CurrentResetParametersChangedEventArgs eventArgs)
                {
                    PendingParametersToReset = eventArgs.PendingParameters.ToList();

                    var handler = CurrentResetParametersChangedEvent;
                    if(handler != null)
                    {
                        handler(this, eventArgs);

                        dispatchedEventArgs.IsHandled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the activate theme context event to cache the pending player session parameters to reset.
        /// </summary>
        /// <param name="sender">
        /// The sender of this event.
        /// </param>
        /// <param name="eventArgs">
        /// The activate (theme) context event arguments.
        /// </param>
        private void HandleActivateThemeContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle F2L theme (which is on link level) context activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme ||
               eventArgs.ContextLayer == ContextLayer.Shell)
            {
                if(gameModeQuery.GameMode == GameMode.Play)
                {
                    IsPlayerSessionParameterResetEnabled =
                        playerSessionParametersCategory.GetConfigDataPlayerSessionParameterResetEnabled();
                    PendingParametersToReset = playerSessionParametersCategory.QueryCurrentResetParameters()
                                                                              .Select(item => (PlayerSessionParameterType)item).ToList();
                }
                else
                {
                    IsPlayerSessionParameterResetEnabled = false;
                    PendingParametersToReset = new List<PlayerSessionParameterType>();
                }
            }
        }

        #endregion
    }
}
