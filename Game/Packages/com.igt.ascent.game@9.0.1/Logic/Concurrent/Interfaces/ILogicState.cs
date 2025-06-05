// -----------------------------------------------------------------------
// <copyright file = "ILogicState.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using Communication.Platform.Interfaces;

    /// <summary>
    /// This interface defines the information and step handlers needed by
    /// the state machine execution, including what steps to run in this state,
    /// how to run the steps, and which state to go next.
    /// </summary>
    /// <typeparamref name="TFrameworkInitialization">
    /// The type of interface to access the functionalities available for
    /// a state machine and its states during the state machine initialization.
    /// </typeparamref>
    /// <typeparamref name="TFrameworkExecution">
    /// The type of interface to access the functionalities available for
    /// a logic state during the state machine execution.
    /// </typeparamref>
    public interface ILogicState<TFrameworkInitialization, TFrameworkExecution>
        where TFrameworkInitialization : IFrameworkInitialization
        where TFrameworkExecution : IFrameworkExecution
    {
        /// <summary>
        /// Gets the name of the state.
        /// </summary>
        string StateName { get; }

        /// <summary>
        /// Gets the first step to run for this state.
        /// </summary>
        StateStep InitialStep { get; }

        /// <summary>
        /// Initializes this state.
        /// After the state machine has been initialized, this method will be called
        /// to initialize each individual state.
        /// </summary>
        /// <remarks>
        /// This method is run without a transaction.
        /// 
        /// It is unlikely that the individual state needs to implement its own providers,
        /// since providers registered by a state machine can be used to configure all
        /// available states in the state machine anyways.
        /// If needed, individual state can access the state machine wide providers via <paramref name="stateMachine"/>.
        /// 
        /// The individual state can use <paramref name="framework"/> to subscribe events etc.
        /// But it must un-subscribe them in its <see cref="CleanUp"/> method.
        /// </remarks>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine initialization.
        /// </param>
        /// <param name="stateMachine">
        /// That state machine to which this state belongs.
        /// </param>
        void Initialize(TFrameworkInitialization framework, object stateMachine);

        /// <summary>
        /// Cleans up the resources held by the logic state.  Called by the framework
        /// when the state machine is done executing.
        /// The clean up work should include, but not limited to:
        /// <list type="bullet">
        ///     <item>Unregister all event handlers from the framework and the LibInterface.</item>
        /// </list>
        /// </summary>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine initialization.
        /// </param>
        void CleanUp(TFrameworkInitialization framework);

        /// <summary>
        /// Gets the transaction weight for the given state step.
        /// </summary>
        /// <remarks>
        /// The transaction weight for the <see cref="StateStep.CommittedWait"/> step
        /// will always be <see cref="TransactionWeight.None"/>.
        /// The state machine framework probably won't call this method for
        /// the <see cref="StateStep.CommittedWait"/> step, but it is recommended that
        /// the implementation always returns the correct value for it.
        /// </remarks>
        /// <param name="step">
        /// The step whose transaction weight is to get.
        /// </param>
        /// <returns>
        /// The transaction weight of the given state step.
        /// </returns>
        TransactionWeight GetTransactionWeight(StateStep step);

        /// <summary>
        /// Execute the <see cref="StateStep.Processing"/> for this state.
        /// </summary>
        /// <remarks>
        /// This step is run within a transaction.
        /// </remarks>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine execution.
        /// </param>
        /// <returns>
        /// The <see cref="ProcessingStepControl"/> indicating which step to run next.
        /// </returns>
        ProcessingStepControl Processing(TFrameworkExecution framework);

        /// <summary>
        /// Execute the <see cref="StateStep.CommittedPreWait"/> for this state.
        /// </summary>
        /// <remarks>
        /// This step is run within a transaction whose weight is determined by <see cref="GetTransactionWeight"/>.
        /// </remarks>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine execution.
        /// </param>
        /// <returns>
        /// The <see cref="PreWaitStepControl"/> indicating which step to run next.
        /// </returns>
        PreWaitStepControl CommittedPreWait(TFrameworkExecution framework);

        /// <summary>
        /// Execute the <see cref="StateStep.CommittedWait"/> for this state.
        /// </summary>
        /// <remarks>
        /// This step is run without a transaction.
        /// </remarks>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine execution.
        /// </param>
        /// <returns>
        /// The <see cref="WaitStepControl"/> indicating which step to run next.
        /// </returns>
        WaitStepControl CommittedWait(TFrameworkExecution framework);

        /// <summary>
        /// Execute the <see cref="StateStep.CommittedPostWait"/> for this state.
        /// </summary>
        /// <remarks>
        /// This step is run within a transaction whose weight is determined by <see cref="GetTransactionWeight"/>.
        /// </remarks>
        /// <param name="framework">
        /// The reference of the interface to access the supporting functions provided by
        /// the state machine framework during the state machine execution.
        /// </param>
        /// <returns>
        /// The <see cref="PostWaitStepControl"/> indicating which step to run next.
        /// </returns>
        PostWaitStepControl CommittedPostWait(TFrameworkExecution framework);
    }
}