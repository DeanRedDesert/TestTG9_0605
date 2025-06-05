//-----------------------------------------------------------------------
// <copyright file = "IdleStateHelper.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.StateHelpers
{
    using System;
    using System.Collections.Generic;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using BetFramework.Interfaces;
    using Communication.Logic.CommServices;
    using Denomination;
    using Foundation.ServiceProviders;
    using BetFrameworkProvider = Ascent.Logic.BettingPermit.FoundationBettingPermit.BetProvider;

    /// <summary>
    ///   This class supports common Idle State functionality that should be common across most games.
    /// </summary>
    public class IdleStateHelper
    {
        /// <summary>
        /// Lookup table for action handlers.
        /// </summary>
        protected readonly Dictionary<string, Func<bool>> IdleActions = new Dictionary<string, Func<bool>>();

        /// <summary>
        /// Framework used to communicate with the presentation.
        /// </summary>
        protected IStateMachineFramework Framework;

        /// <summary>
        /// Bet provider(based on BetFramework) containing bet.
        /// </summary>
        protected BetFrameworkProvider BetFrameworkProvider;

        /// <summary>
        /// Provider containing current auto-play settings and to update auto-play bet on.
        /// </summary>
        protected AutoPlayProvider AutoPlayProvider;

        /// <summary>
        /// The denomination change instance used to request the denomination change.
        /// </summary>
        protected IDenominationChange DenominationChangeHandler;

        /// <summary>
        /// Idle state's presentation complete message that was read from safe storage.
        /// </summary>
        protected GameLogicPresentationStateCompleteMsg IdleCompleteMessage;

        ///<summary>
        /// This sets the default handlers for supported actions in Idle state.
        ///</summary>
        public void SetDefaultHandlers()
        {
            IdleActions[IdleStateHelperSupportedActions.StartGameRequest] = StartGame;
            IdleActions[IdleStateHelperSupportedActions.CashOutRequest] = CashOut;
            IdleActions[IdleStateHelperSupportedActions.AutoPlayChangeStateRequest] = AutoPlayStateChange;
            IdleActions[IdleStateHelperSupportedActions.ChangeLanguageRequest] = ChangeLanguage;
            IdleActions[IdleStateHelperSupportedActions.ShowThemeSelectionMenuRequest] = ShowThemeSelectionMenu;
            IdleActions[IdleStateHelperSupportedActions.ChangeDenominationRequest] = ChangeDenomination;
            IdleActions[IdleStateHelperSupportedActions.TransferBankToWagerableRequest] = TransferBankToWagerable;
        }

        ///<summary>
        /// Sets the idle state action handler for a specific action
        ///</summary>
        ///<param name="actionName">Action to handle.</param>
        ///<param name="actionHandler">Function to use when the action is received.</param>
        public void SetActionHandler(string actionName, Func<bool> actionHandler)
        {
            IdleActions[actionName] = actionHandler;
        }

        ///<summary>
        /// Returns the function handle for the desired action.
        ///</summary>
        ///<param name="actionName">Action to return handler for</param>
        ///<returns>Function handle for requested action.</returns>
        ///<remarks>
        ///This function will be used primarily for debug purposes to see what
        ///function will be called for an action.
        ///</remarks>
        internal Func<bool> GetActionHandler(string actionName)
        {
            Func<bool> handler = null;
            if(IdleActions.ContainsKey(actionName))
            {
                handler = IdleActions[actionName];
            }
            return handler;
        }

        ///<summary>
        /// Starts the idle state by enabling the bet, updating the auto-play bet, and starting the presentation.
        /// Safe stores the presentation response.
        ///</summary>
        ///<param name = "stateMachineBase">State machine to use to start a presentation state.</param>
        ///<param name = "framework">Framework used to communicate with the presentation.</param>
        ///<param name = "betProvider">Bet provider containing bet.</param>
        ///<param name = "autoPlayProvider">Auto-play provider to update bet on.</param>
        ///<returns>Presentation response.</returns>
        ///<exception cref = "ArgumentNullException">Thrown if stateMachineBase, framework, or betProvider is null.</exception>
        public virtual GameLogicPresentationStateCompleteMsg
            HandleIdlePresentation(StateMachineBase stateMachineBase, IStateMachineFramework framework,
                                   BetFrameworkProvider betProvider, AutoPlayProvider autoPlayProvider)
        {
            if(betProvider == null)
            {
                throw new ArgumentNullException(nameof(betProvider));
            }

            betProvider.Data.Enabled = true;

            // Set the total Bet if auto play provider is created.
            if(autoPlayProvider != null)
            {
                autoPlayProvider.CurrentBet = betProvider.GetTotalInGameCredits();
            }

            return HandlePresentation(stateMachineBase, framework);
        }

        ///<summary>
        /// Starts the presentation and safe stores the presentation response.
        ///</summary>
        ///<param name="stateMachineBase">State machine to use to start a presentation state.</param>
        ///<param name = "framework">Framework used to communicate with the presentation.</param>
        ///<returns>Presentation response.</returns>
        ///<exception cref = "ArgumentNullException">Thrown if stateMachineBase, framework is null.</exception>
        protected virtual GameLogicPresentationStateCompleteMsg HandlePresentation(StateMachineBase stateMachineBase,
                                                                                   IStateMachineFramework framework)
        {
            if(stateMachineBase == null)
            {
                throw new ArgumentNullException(nameof(stateMachineBase));
            }

            if(framework == null)
            {
                throw new ArgumentNullException(nameof(framework));
            }

            stateMachineBase.StartPresentationState(framework);

            var completeMessage = framework.GetPresentationEvent<GameLogicPresentationStateCompleteMsg>();
            framework.GameLib.WriteCriticalData(CriticalDataScope.Theme,
                                                StateHelperCriticalDataPaths.IdlePresentationCompletePath,
                                                completeMessage);
            return completeMessage;
        }

        ///<summary>
        /// Processes the Idle state complete message.
        /// This functions the same as its other overload except doesn't output the idle complete message.
        ///</summary>
        ///<param name = "framework">Framework containing GameLib.</param>
        ///<param name = "betProvider">BetProvider containing current bet.</param>
        ///<param name = "autoPlayProvider">AutoPlay provider containing current auto-play settings.</param>
        ///<param name="denominationChangeHandler">The handler used to request denomination change.</param>
        ///<returns>True if the game is committed.</returns>
        public virtual bool ProcessIdleActions(IStateMachineFramework framework, 
                                               BetFrameworkProvider betProvider,
                                               AutoPlayProvider autoPlayProvider,
                                               IDenominationChange denominationChangeHandler)
        {
            return ProcessIdleActions(framework,
                                      betProvider,
                                      autoPlayProvider,
                                      denominationChangeHandler,
                                      out _);
        }

        ///<summary>
        /// Processes the Idle state complete message. Will change the bet if the idle complete message requests a bet level.
        /// Also will handle the following actions:
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="IdleStateHelperSupportedActions.StartGameRequest"/></term>
        ///         <description>
        ///         Commits a game if there is enough credits.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IdleStateHelperSupportedActions.CashOutRequest"/></term>
        ///         <description>
        ///         Requests a Cashout from the system.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IdleStateHelperSupportedActions.AutoPlayChangeStateRequest"/></term>
        ///         <description>
        ///         Changes the current Auto-play state.  Requires Generic Data with key
        ///         <see cref="IdleStateHelperDataKeys.AutoPlayStarted"/> containing a flag
        ///         of new auto-play state.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IdleStateHelperSupportedActions.ChangeLanguageRequest"/></term>
        ///         <description>
        ///         Attempts to change language.  Requires Generic Data with key
        ///         <see cref="IdleStateHelperDataKeys.Language"/> containing name of new language.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IdleStateHelperSupportedActions.ChangeDenominationRequest"/></term>
        ///         <description>
        ///         Requests to change denomination.  Requires Generic Data with key
        ///         <see cref="IdleStateHelperDataKeys.Denomination"/> containing value of new denomination.
        ///         </description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="IdleStateHelperSupportedActions.TransferBankToWagerableRequest"/></term>
        ///         <description>
        ///          Requests a transfer from the credit (Bank) to the Wagerable meter.
        ///         </description>
        ///     </item>
        /// </list>
        ///</summary>
        ///<param name="framework">Framework containing GameLib.</param>
        ///<param name="betProvider">BetProvider containing current bet.</param>
        ///<param name="autoPlayProvider">AutoPlay provider containing current auto-play settings.</param>
        ///<param name="denominationChangeHandler">The handler used to request denomination change.</param>
        ///<param name="idleCompleteMessage">Outputs the complete message that was read from safe storage.</param>
        ///<returns>True if game is committed.</returns>
        ///<exception cref="ArgumentNullException">
        /// Thrown if any of the <paramref name="framework"/>, <paramref name="betProvider"/>, 
        /// or <paramref name="denominationChangeHandler"/> is null.
        /// </exception>
        ///<exception cref="NullReferenceException">Thrown if <paramref name="idleCompleteMessage"/> is null when retrieved from safe storage.</exception>
        public virtual bool ProcessIdleActions(IStateMachineFramework framework,
                                               BetFrameworkProvider betProvider,
                                               AutoPlayProvider autoPlayProvider,
                                               IDenominationChange denominationChangeHandler,
                                               out GameLogicPresentationStateCompleteMsg idleCompleteMessage)
        {
            if(framework == null)
            {
                throw new ArgumentNullException(nameof(framework), "Parameters may not be null.");
            }

            BetFrameworkProvider = betProvider ?? throw new ArgumentNullException(nameof(betProvider));
            AutoPlayProvider = autoPlayProvider;
            DenominationChangeHandler = denominationChangeHandler ?? throw new ArgumentNullException(nameof(denominationChangeHandler));

            // Reading critical data.
            GetIdleCompleteMessage(framework);

            // Process betting.
            ProcessBetFrameworkProvider();

            // Process the action.
            var committed = ProcessIdleCompleteActions();

            idleCompleteMessage = IdleCompleteMessage;

            return committed;
        }

        ///<summary>
        /// Enables Betting and updates the auto-play bet.
        ///</summary>
        protected virtual void ProcessBetFrameworkProvider()
        {
            BetFrameworkProvider.Data.Enabled = true;

            // Set the total Bet if auto play provider is created.
            if(AutoPlayProvider != null)
            {
                AutoPlayProvider.CurrentBet = BetFrameworkProvider.GetTotalInGameCredits();
            }
        }

        /// <summary>
        /// Handles the different actions.
        /// </summary>
        /// <returns>True if game is committed.</returns>
        protected virtual bool ProcessIdleCompleteActions()
        {
            var committed = false;
            if(IdleActions.ContainsKey(IdleCompleteMessage.ActionRequest))
            {
                committed = IdleActions[IdleCompleteMessage.ActionRequest]();
            }

            return committed;
        }

        ///<summary>
        /// Gets the value for the IdleCompleteMessage by reading the data from
        /// <see cref="StateHelperCriticalDataPaths.IdlePresentationCompletePath"/>.
        ///</summary>
        ///<param name="framework"></param>
        protected virtual void GetIdleCompleteMessage(IStateMachineFramework framework)
        {
            Framework = framework ?? throw new ArgumentNullException(nameof(framework));

            // Read the presentation complete message.
            IdleCompleteMessage =
                framework.GameLib.ReadCriticalData<GameLogicPresentationStateCompleteMsg>(CriticalDataScope.Theme,
                                                                                          StateHelperCriticalDataPaths.
                                                                                              IdlePresentationCompletePath);
            if(IdleCompleteMessage == null)
            {
                throw new NullReferenceException(
                    "IdleComplete Message was not stored in safe storage. Scope: Theme Path: " +
                    StateHelperCriticalDataPaths.IdlePresentationCompletePath);
            }
        }

        /// <summary>
        /// Requests a Cashout from the system.
        /// </summary>
        /// <returns>True the game should attempt to start a game. This is always false.</returns>
        private bool CashOut()
        {
            Framework.GameLib.RequestCashOut();
            return false;
        }

        /// <summary>
        /// Requests changes to the auto-play state.
        /// </summary>
        /// <exception cref="InvalidActionException">Thrown if generic data is missing for auto play.</exception>
        /// <returns>True the game should attempt to start a game. This is always false.</returns>
        private bool AutoPlayStateChange()
        {
            if(IdleCompleteMessage.GenericData.ContainsKey(IdleStateHelperDataKeys.AutoPlayStarted))
            {
                if((bool)IdleCompleteMessage.GenericData[IdleStateHelperDataKeys.AutoPlayStarted])
                {
                    Framework.GameLib.SetAutoPlayOn();
                }
                else
                {
                    Framework.GameLib.SetAutoPlayOff();
                }
            }
            else
            {
                throw new InvalidActionException(IdleStateHelperSupportedActions.AutoPlayChangeStateRequest +
                                                 " requires generic data containing a value for " +
                                                 IdleStateHelperDataKeys.AutoPlayStarted);
            }
            return false;
        }

        /// <summary>
        /// Request a change on language.
        /// </summary>
        /// <exception cref="InvalidActionException">Thrown if the generic data required to change language is missing</exception>
        /// <returns>True if the game should attempt to start a game. This function always returns false.</returns>
        protected virtual bool ChangeLanguage()
        {
            if(IdleCompleteMessage.GenericData.ContainsKey(IdleStateHelperDataKeys.Language))
            {
                var language = IdleCompleteMessage.GenericData[IdleStateHelperDataKeys.Language] as string;
                Framework.GameLib.SetLanguage(language);
            }
            else
            {
                throw new InvalidActionException(IdleStateHelperSupportedActions.ChangeLanguageRequest +
                                                 " requires generic data containing a value for " +
                                                 IdleStateHelperDataKeys.Language);
            }
            return false;
        }

        /// <summary>
 		/// Requests the foundation to show the theme selection menu.
        /// </summary>
        /// <returns>True if the game should attempt to start a game. This method always returns false.</returns>
        protected virtual bool ShowThemeSelectionMenu()
        {
            Framework.GameLib.RequestThemeSelectionMenu();
            return false;
        }

        /// <summary>
        /// Requests a change in denomination.
        /// </summary>
        /// <exception cref="InvalidActionException">Thrown if the generic data required to change denomination is missing</exception>
        /// <returns>True the game should attempt to start a game. This is always false.</returns>
        protected virtual bool ChangeDenomination()
        {
            if(IdleCompleteMessage.GenericData.ContainsKey(IdleStateHelperDataKeys.Denomination))
            {
                var denomination = (long)IdleCompleteMessage.GenericData[IdleStateHelperDataKeys.Denomination];
                DenominationChangeHandler.RequestDenominationChange(denomination);
            }
            else
            {
                throw new InvalidActionException(IdleStateHelperSupportedActions.ChangeDenominationRequest +
                                                 " requires generic data containing a value for " +
                                                 IdleStateHelperDataKeys.Denomination);
            }
            return false;
        }

        /// <summary>
        /// Requests a transfer from the credit (Bank) to the Wagerable meter.
        /// </summary>
        /// <returns>True the game should attempt to start a game. This is always false.</returns>
        private bool TransferBankToWagerable()
        {
            Framework.GameLib.RequestMoneyMove(MoneyLocation.PlayerBankMeter, MoneyLocation.PlayerWagerableMeter);
            return false;
        }

        /// <summary>
        /// Starts the commit game process.
        /// </summary>
        /// <returns>Return true if commit bet is successful.</returns>
        /// <devdoc>
        /// In the future, we are going to get rid of the old BetProvider.
        /// The UnCommitGameCycle() is repeated right now but one will be removed later.
        /// </devdoc>
        /// <exception cref="NullReferenceException">
        /// Thrown if <see cref="BetFrameworkProvider"/> is in use and its <see cref="IBetData"/> member is null.
        /// </exception>
        protected virtual bool StartGame()
        {
            var committed = false;

            if(BetFrameworkProvider != null)
            {
                if(IdleCompleteMessage.GenericData.ContainsKey(IdleStateHelperDataKeys.AutoPlayStarted) &&
                   (bool)IdleCompleteMessage.GenericData[IdleStateHelperDataKeys.AutoPlayStarted])
                {
                    if(BetFrameworkProvider.Data == null)
                    {
                        throw new NullReferenceException("BetFrameworkProvider's bet data found to be null when " +
                                                         "attempting to commit game.");
                    }
                    BetFrameworkProvider.Data.SetVariable("Commit", true);
                }

                if(BetFrameworkProvider.Data.Commit && Framework.GameLib.CommitGameCycle())
                {
                    committed = Framework.GameLib.CommitBet(BetFrameworkProvider.GetTotalInGameCredits(),
                                                            Framework.GameLib.GameDenomination);
                    BetFrameworkProvider.Data.Enabled = !committed;
                    if(!committed)
                    {
                        Framework.GameLib.UncommitGameCycle();
                    }
                }
            }
           
            return committed;
        }
    }
}
