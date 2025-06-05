// -----------------------------------------------------------------------
// <copyright file = "IFrameworkExecution.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;

    /// <summary>
    /// This interface defines functionalities available for a state machine and its states
    /// during the state machine execution.
    /// </summary>
    public interface IFrameworkExecution
    {
        /// <summary>
        /// Specifies the next state to execute.
        /// If called multiple times during a state's execution,
        /// the value set in the last call will be the next state.
        /// </summary>
        /// <param name="nextStateName">
        /// The name of the state to execute after the current state completes.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="nextStateName"/> is null.
        /// </exception>
        /// <exception cref="ConcurrentLogicException">
        /// Thrown when no state by the name of <paramref name="nextStateName"/> is found.
        /// </exception>
        void SetNextState(string nextStateName);

        /// <summary>
        /// Starts the presentation state associated with the current logic state.
        /// </summary>
        void StartPresentationState();

        /// <summary>
        /// Waits for a condition to be fulfilled.  The condition checking involves
        /// both lightweight transactional and non transactional operations.
        /// No heavyweight transactional operation is allowed in the condition checking.
        /// </summary>
        /// <param name="conditionChecker">
        /// The delegate used to check whether the condition is fulfilled.
        /// It takes a Boolean flag as the argument which indicates whether the delegate
        /// is invoked inside a lightweight transaction or not, so that it knows when
        /// it is okay to do the transactional operation;
        /// The delegate returns a Boolean flag indicating whether the condition is fulfilled.
        /// </param>
        /// <returns>
        /// True if the <paramref name="conditionChecker"/> returns True;
        /// False if the waiting has been interrupted by other higher priority tasks.
        /// Caller should repeat the Wait step if this method returns false.
        /// </returns>
        bool WaitForCondition(Func<bool, bool> conditionChecker);

        /// <summary>
        /// Waits for a condition to be fulfilled.  The condition checking involves
        /// only lightweight transactional operations.
        /// </summary>
        /// <param name="conditionChecker">
        /// The delegate used to check whether the condition is fulfilled.
        /// It is always invoked inside a lightweight transaction.
        /// The delegate takes no argument, and returns a Boolean flag
        /// indicating whether the condition is fulfilled.
        /// </param>
        /// <returns>
        /// True if the <paramref name="conditionChecker"/> returns True;
        /// False if the waiting has been interrupted by other higher priority tasks.
        /// Caller should repeat the Wait step if this method returns false.
        /// </returns>
        bool WaitForLightweightTransactionalCondition(Func<bool> conditionChecker);

        /// <summary>
        /// Waits for a condition to be fulfilled.  The condition checking involves
        /// only non transactional operations.
        /// </summary>
        /// <param name="conditionChecker">
        /// The delegate used to check whether the condition is fulfilled.
        /// It should not do any transactional operations.
        /// The delegate takes no argument, and returns a Boolean flag
        /// indicating whether the condition is fulfilled.
        /// </param>
        /// <returns>
        /// True if the <paramref name="conditionChecker"/> returns True;
        /// False if the waiting has been interrupted by other higher priority tasks.
        /// Caller should repeat the Wait step if this method returns false.
        /// </returns>
        bool WaitForNonTransactionalCondition(Func<bool> conditionChecker);

        /// <summary>
        /// Waits for the current presentation state to be complete as signaled
        /// by a Presentation Complete Message sent by the presentation.
        /// </summary>
        /// <returns>
        /// True if current presentation state is complete;
        /// False if the waiting has been interrupted by other higher priority tasks.
        /// Caller should repeat the Wait step if this method returns false.
        /// </returns>
        /// <remarks>
        /// The state machine framework only supports the very basic presentation event handling
        /// for the state machine.  For more complex tasks, the state machine should subscribe to
        /// the PresentationEventReceived event provided by the framework.
        /// </remarks>
        bool WaitForPresentationStateComplete();
    }
}