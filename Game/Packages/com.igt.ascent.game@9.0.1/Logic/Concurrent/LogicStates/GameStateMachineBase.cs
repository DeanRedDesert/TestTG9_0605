// -----------------------------------------------------------------------
// <copyright file = "GameStateMachineBase.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System.Diagnostics.CodeAnalysis;
    using Communication.Platform.CoplayerLib.Interfaces;
    using Interfaces;
    using ServiceProviders;

    /// <inheritdoc cref="IGameStateMachine"/>UnusedParameter.Local
    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    public abstract class GameStateMachineBase : StateMachineBase<IGameState, IGameFrameworkInitialization, IGameFrameworkExecution>,
                                                 IGameStateMachine
    {
        #region Constructors

        /// <summary>
        /// Default constructor.
        /// This allows the derived classes not have to call the other constructor.
        /// </summary>
        protected GameStateMachineBase()
        {
        }

        /// <summary>
        /// Demonstration of how states are created and chained.
        /// Derived classes should NOT need to call this base class constructor
        /// since they usually have their own states setup.
        /// </summary>
        /// <param name="coplayerContext">
        /// The coplayer context for which the state machine is created.
        /// </param>
        protected GameStateMachineBase(ICoplayerContext coplayerContext)
        {
            // Instantiate and add states.
            var idleState = (BaseIdleState)AddState(new BaseIdleState());
            var enrollState = (BaseEnrollState)AddState(new BaseEnrollState());
            var evaluationState = (BaseEvaluationState)AddState(new BaseEvaluationState());
            var playState = (BasePlayState)AddState(new BasePlayState());
            var finalizeState = (BaseFinalizeState)AddState(new BaseFinalizeState());
            var endGameCycleState = (BaseEndGameCycleState)AddState(new BaseEndGameCycleState());
            var abortState = (BaseAbortState)AddState(new BaseAbortState());

            InitialStateName = idleState.StateName;

            // State Transitions.
            idleState.NextOnCommitted = enrollState;
            enrollState.NextOnEnrollFailure = idleState;
            enrollState.NextOnEnrollSuccess = evaluationState;
            evaluationState.NextOnComplete = playState;
            playState.NextOnMainPlayComplete = finalizeState;
            playState.NextOnAbort = abortState;
            finalizeState.NextOnComplete = endGameCycleState;
            endGameCycleState.NextOnComplete = idleState;
            abortState.NextOnAbortComplete = endGameCycleState;
            abortState.NextOnAbortRejected = playState;
        }

        #endregion

        #region IGameStateMachine Implementation

        /// <inheritdoc/>
        public virtual void Initialize(IGameFrameworkInitialization framework)
        {
            var configurationProvider = new CoplayerConfigurationProvider(framework.CoplayerLib, framework.ShellExposition);
            framework.ServiceController.AddProvider(configurationProvider, configurationProvider.Name);

            var displayControlStateProvider = new DisplayControlStateProvider(framework.ObservableCollection.ObservableDisplayControlState);
            framework.ServiceController.AddProvider(displayControlStateProvider, displayControlStateProvider.Name);

            var cultureProvider = new CultureProvider(framework.ObservableCollection.ObservableCulture);
            framework.ServiceController.AddProvider(cultureProvider, cultureProvider.Name);
        }

        /// <inheritdoc/>
        public virtual void ReadConfiguration(ICoplayerLib coplayerLib)
        {
        }

        /// <inheritdoc/>
        public abstract void CleanUp(IGameFrameworkInitialization framework);

        #endregion
    }
}