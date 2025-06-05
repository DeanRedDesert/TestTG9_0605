// -----------------------------------------------------------------------
// <copyright file = "IFrameworkRunner.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Game.Core.Communication.Logic.CommServices;

    /// <summary>
    /// This interface defines functionalities provided by a runner class
    /// to support a state machine framework.
    /// </summary>
    internal interface IFrameworkRunner
    {
        /// <summary>
        /// Executes the given action in a heavyweight transaction.
        /// </summary>
        /// <param name="transactionName">
        /// The name to use for the transaction.
        /// </param>
        /// <param name="action">
        /// The action to execute in the transaction.
        /// </param>
        /// <returns>
        /// True if the transaction is opened and the action executed;
        /// False otherwise.  A transaction can fail to open if there are higher priority interrupts.
        /// </returns>
        bool DoTransaction(string transactionName, Action action);

        /// <summary>
        /// Executes the given action in a lightweight transaction.
        /// </summary>
        /// <param name="transactionName">
        /// The name to use for the transaction.
        /// </param>
        /// <param name="action">
        /// The action to execute in the transaction.
        /// </param>
        /// <returns>
        /// True if the transaction is opened and the action executed;
        /// False otherwise.  A transaction can fail to open if there are higher priority interrupts.
        /// </returns>
        bool DoTransactionLite(string transactionName, Action action);

        /// <summary>
        /// Executes the given action in the next available heavyweight transaction,
        /// by either appending to an existing transaction or creating a new one.
        /// </summary>
        /// <param name="transactionName">
        /// The name to use if a new transaction is created for this action.
        /// </param>
        /// <param name="action">
        /// The action to execute in the transaction.
        /// </param>
        /// <returns>
        /// True if a transaction is available and the action executed;
        /// False otherwise.  A transaction can fail to open if there are higher priority interrupts.
        /// </returns>
        bool OnNextTransaction(string transactionName, Action action);

        /// <summary>
        /// Waits for a condition to be fulfilled.  The condition checking involves
        /// both lightweight transactional and non transactional operations.
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
        /// Registers an event queue to participate in the event processing loop.
        /// </summary>
        /// <remarks>
        /// The event queue must be unregistered by calling <see cref="UnregisterEventQueue"/>
        /// when the event queue ceases to exist or is disposed.
        /// 
        /// If two event queues receive events at the same time,
        /// the event queue registered earlier will be processed first.
        /// </remarks>
        /// <param name="eventQueue">
        /// The event queue to register.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventQueue"/> is null.
        /// </exception>
        void RegisterEventQueue(IEventQueue eventQueue);

        /// <summary>
        /// Unregisters an event queue.
        /// </summary>
        /// <param name="eventQueue">The event queue to unregister.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventQueue"/> is null.
        /// </exception>
        void UnregisterEventQueue(IEventQueue eventQueue);

        /// <summary>
        /// Occurs when a presentation event has been received.
        /// </summary>
        event EventHandler<GameLogicGenericMsg> PresentationEventReceived;

        /// <summary>
        /// Occurs after each event queue's ProcessEvents is called.
        /// </summary>
        event EventHandler EventQueuePostProcessing;
    }
}