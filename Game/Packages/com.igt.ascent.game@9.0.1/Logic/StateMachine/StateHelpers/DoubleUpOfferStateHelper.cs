//-----------------------------------------------------------------------
// <copyright file = "DoubleUpOfferStateHelper.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.Interfaces;
    using Communication.Logic.CommServices;
    using Foundation.ServiceProviders;

    /// <summary>
    /// This class supports common DoubleUpOffer functionality that should be common across most games.
    /// </summary>
    public class DoubleUpOfferStateHelper
    {
        #region Protected Members

        /// <summary>
        /// Lookup table for action handlers.
        /// </summary>
        protected Dictionary<string, Action> DoubleUpOfferActions = new Dictionary<string, Action>();

        /// <summary>
        /// Framework used to communicate with the presentation.
        /// </summary>
        protected IStateMachineFramework StateMachineFramework;

        /// <summary>
        /// DoubleUpOffer state's presentation complete message that was read from safe storage.
        /// </summary>
        protected GameLogicPresentationStateCompleteMsg doubleUpOfferCompleteMessage;

        #endregion

        #region Public Members

        /// <summary>
        /// Flag indicating the player has start a new game in DoubleUp offer state.
        /// Please don't access this property in Idle state, since it accesses GameCycle scope of critical data. 
        /// </summary>
        public bool IsNewGameRequested
        {
            get
            {
                return StateMachineFramework != null && StateMachineFramework.GameLib.ReadCriticalData<bool>(CriticalDataScope.GameCycle,
                                                    StateHelperCriticalDataPaths.NewGameRequestedWhenDoubleUpOfferPath);
            }
            private set
            {
                StateMachineFramework.GameLib.WriteCriticalData(CriticalDataScope.GameCycle, 
                        StateHelperCriticalDataPaths.NewGameRequestedWhenDoubleUpOfferPath, value);
            }
        }

        /// <summary>
        /// Flag indicating going to DoubleUp or not.
        /// </summary>
        public bool IsDoubleUpStarted { get; private set; }


        #endregion

        #region Public Methods

        ///<summary>
        /// This sets the default handlers for supported actions in DoubleUp offer state.
        ///</summary>
        public void SetDefaultHandlers()
        {
            DoubleUpOfferActions[DoubleUpOfferStateHelperSupportedActions.DoubleUpPressedRequest] = StartDoubleUp;
            DoubleUpOfferActions[DoubleUpOfferStateHelperSupportedActions.DoubleUpDisabledRequest] = CancelDoubleUpOffer;
            DoubleUpOfferActions[DoubleUpOfferStateHelperSupportedActions.LineSelectedRequest] = CancelDoubleUpOffer;
            DoubleUpOfferActions[DoubleUpOfferStateHelperSupportedActions.StartGameRequest] = StartGame;
            DoubleUpOfferActions[DoubleUpOfferStateHelperSupportedActions.AutoPlayRequest] = StartGame;
        }

        ///<summary>
        /// Sets the DoubleUp offer state action handler for a specific action
        ///</summary>
        ///<param name="actionName">Action to handle.</param>
        ///<param name="actionHandler">Function to use when the action is received.</param>
        public void SetActionHandler(string actionName, Action actionHandler)
        {
            DoubleUpOfferActions[actionName] = actionHandler;
        }

        /// <summary>
        /// Request Foundation to offer DoubleUp.
        /// Attention: this function can only be called once in Processing stage within a game cycle. 
        /// </summary>
        /// <param name="framework">Framework used to communicate with the presentation.</param>
        /// <param name="riskAmout">The risk amount.</param>
        /// <returns>True if DoubleUp is offered. Otherwise, false. </returns>
        /// <exception cref = "ArgumentNullException">Thrown if <paramref name="framework"/> is null.</exception>
        public bool RequestOfferDoubleUp(IStateMachineFramework framework, long riskAmout)
        {
            if (framework == null)
            {
                throw new ArgumentNullException("framework", "Parameter cannot be null.");
            }

            const int maxDoubleUpWinMultiplier = 4;
            var moneytaryLimit = framework.GameLib.AncillaryMonetaryLimit;
            // If limit equals to 0, there is no limit.
            return framework.GameLib.AncillaryEnabled &&
                    riskAmout > 0 &&
                    (moneytaryLimit == 0 || riskAmout * maxDoubleUpWinMultiplier <= moneytaryLimit) &&
                    framework.GameLib.OfferAncillaryGame();
        }

        /// <summary>
        /// Start a typical presentation to offer DoubleUp.
        /// </summary>
        /// <param name="stateMachineBase">State machine to use to start a presentation state.</param>
        /// <param name="framework">Framework used to communicate with the presentation.</param>
        /// <param name="offerProvider">DoubleUp offer provider to check if DoubleUp is still available.</param>
        /// <exception cref = "ArgumentNullException">
        /// Thrown if <paramref name="stateMachineBase"/>, <paramref name="framework"/>, 
        /// or <paramref name="offerProvider"/> is null.
        /// </exception>
        public virtual void HandleDoubleUpOfferPresentation(StateMachineBase stateMachineBase,
                                                            IStateMachineFramework framework,
                                                            AncillaryGameOfferProvider offerProvider)
        {
            if (framework == null)
            {
                throw new ArgumentNullException("framework", "Parameter cannot be null.");
            }
            StateMachineFramework = framework;

            if (stateMachineBase == null)
            {
                throw new ArgumentNullException("stateMachineBase", "Parameter cannot be null.");
            }

            if (offerProvider == null)
            {
                throw new ArgumentNullException("stateMachineBase", "Parameter cannot be null.");
            }

            if (!offerProvider.IsAncillaryGameAvailable)
            {
                return;
            }

            // Start the presentation.
            stateMachineBase.StartPresentationState(framework);

            doubleUpOfferCompleteMessage = framework.GetPresentationEvent<GameLogicPresentationStateCompleteMsg>();

            // Disable the DoubleUp button. 
            offerProvider.IsAncillaryGameAvailable = false;

            // Clear Enter DoubleUp before processing the action request.
            IsDoubleUpStarted = false;

            // Process the presentation complete message.
            if (DoubleUpOfferActions.ContainsKey(doubleUpOfferCompleteMessage.ActionRequest))
            {
                DoubleUpOfferActions[doubleUpOfferCompleteMessage.ActionRequest]();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Process the request of starting DoubleUp.
        /// </summary>
        protected void StartDoubleUp()
        {
            IsDoubleUpStarted = StateMachineFramework.GameLib.StartAncillaryPlaying();
        }

        /// <summary>
        /// Process the request of starting a new game.
        /// </summary>
        protected void StartGame()
        {
            // For start game and start auto play case.
            // Safe store the complete message and will omit idle state later. 
            StateMachineFramework.GameLib.WriteCriticalData(CriticalDataScope.Theme,
                                                StateHelperCriticalDataPaths.IdlePresentationCompletePath,
                                                doubleUpOfferCompleteMessage);
            IsNewGameRequested = true;
        }

        /// <summary>
        /// Process the request of cancelling the DoubleUp offer. 
        /// </summary>
        protected void CancelDoubleUpOffer()
        {
            // Do nothing, just end the DoubleUpOffer state.
        }

        #endregion

        #region Internal Methods

        ///<summary>
        /// Returns the function handle for the desired action.
        ///</summary>
        ///<param name="actionName">Action to return handler for</param>
        ///<returns>Function handle for requested action.</returns>
        ///<remarks>
        ///This function will be used primarily for debug purposes to see what
        ///function will be called for an action.
        ///</remarks>
        internal Action GetActionHandler(string actionName)
        {
            Action handler = null;
            if (DoubleUpOfferActions.ContainsKey(actionName))
            {
                handler = DoubleUpOfferActions[actionName];
            }
            return handler;
        }

        #endregion
    }
}
