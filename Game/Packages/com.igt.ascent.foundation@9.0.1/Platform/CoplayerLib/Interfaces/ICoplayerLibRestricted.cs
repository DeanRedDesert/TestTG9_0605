// -----------------------------------------------------------------------
// <copyright file = "ICoplayerLibRestricted.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using Game.Core.Threading;
    using IGT.Ascent.Restricted.EventManagement.Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines restricted functionality of a coplayer communication lib.
    /// The restricted functionality is supposed to be used by higher level SDK components only.
    /// Game development should not use this interface.
    /// </summary>
    public interface ICoplayerLibRestricted
    {
        #region Events

        /// <summary>
        /// Occurs when a response is received for a heavyweight action request.
        /// </summary>
        event EventHandler<ActionResponseEventArgs> ActionResponseEvent;

        /// <summary>
        /// Occurs when a response is received for a lightweight action request.
        /// </summary>
        event EventHandler<ActionResponseLiteEventArgs> ActionResponseLiteEvent;

        /// <summary>
        /// Occurs when a new coplayer context is about to start.
        /// </summary>
        event EventHandler<NewCoplayerContextEventArgs> NewCoplayerContextEvent;

        /// <summary>
        /// Occurs when a new coplayer context is activated.
        /// </summary>
        event EventHandler<ActivateCoplayerContextEventArgs> ActivateCoplayerContextEvent;

        /// <summary>
        /// Occurs when the current coplayer context is inactivated.
        /// </summary>
        event EventHandler<InactivateCoplayerContextEventArgs> InactivateCoplayerContextEvent;

        /// <summary>
        /// Occurs when the coplayer is being shut down by the Foundation.
        /// </summary>
        /// <remarks>
        /// This event is not associated with a transaction.
        /// It is raised on a communication thread. Therefore, handler of this event must be thread safe.
        /// </remarks>
        event EventHandler<ShutDownEventArgs> ShutDownEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the unique id of the coplayer.
        /// </summary>
        /// <remarks>
        /// When handling coplayer lib events, Shell Runner will cast the event sender to
        /// a coplayer lib and then retrieve the coplayer id from this property.
        /// We cannot rely on the coplayer context because if a shut down event is received
        /// after a failed category negotiation, the coplayer context is not initialized yet.
        /// </remarks>
        int CoplayerId { get; }

        /// <summary>
        /// Gets the exception monitor used by the coplayer session.
        /// </summary>
        IExceptionMonitor ExceptionMonitor { get; }

        /// <summary>
        /// Gets the transactional event queue associated with the coplayer session.
        /// </summary>
        IEventQueue TransactionalEventQueue { get; }

        /// <summary>
        /// Gets the non transactional event queue associated with the coplayer session.
        /// </summary>
        IEventQueue NonTransactionalEventQueue { get; }

        /// <summary>
        /// Gets the restricted interface for game cycle play.
        /// </summary>
        IGameCyclePlayRestricted GameCyclePlayRestricted { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Establishes the F2X connection to the Foundation.
        /// </summary>
        /// <devdoc>
        /// Must be called following the construction of Coplayer Lib.
        /// It is part of the initialization that puts the Coplayer Lib
        /// in a workable state.
        /// </devdoc>
        /// <returns>
        /// True if connection is established successfully; False otherwise.
        /// </returns>
        bool ConnectToFoundation();

        /// <summary>
        /// Initiates a heavyweight action request.
        /// </summary>
        /// <param name="transactionName">
        /// A name to associate with the transaction.
        /// This parameter is optional. If not specified, it defaults to null.
        /// </param>
        void ActionRequest(string transactionName = null);

        /// <summary>
        /// Initiates a lightweight action request.
        /// </summary>
        /// <param name="transactionName">
        /// A name to associate with the transaction.
        /// This parameter is optional. If not specified, it defaults to null.
        /// </param>
        void ActionRequestLite(string transactionName = null);

        #endregion
    }
}