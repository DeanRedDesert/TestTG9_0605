// -----------------------------------------------------------------------
// <copyright file = "IExtensionBinLibRestricted.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionBinLib.Interfaces
{
    using System;
    using Game.Core.Threading;
    using Platform.Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// This interface defines restricted functionality available for an Extension Bin application
    /// to communicate with the Foundation.
    /// The restricted functionality is supposed to be used by higher level SDK components only.
    /// Extension development should not use this interface.
    /// </summary>
    public interface IExtensionBinLibRestricted
    {
        #region Events

        /// <summary>
        /// Occurs when a response is received for a heavyweight action request.
        /// </summary>
        event EventHandler<ActionResponseEventArgs> ActionResponseEvent;

        /// <summary>
        /// Occurs when the Extension Bin is activated by the Foundation.
        /// </summary>
        event EventHandler<ActivateContextEventArgs> ActivateContextEvent;

        /// <summary>
        /// Occurs when the Extension Bin is being shut down by the Foundation.
        /// </summary>
        /// <remarks>
        /// This event is not associated with a transaction.
        /// It is raised on a pool thread. Therefore, handler of this event must be thread safe.
        /// </remarks>
        event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <summary>
        /// Occurs when the Extension Bin is being parked by the Foundation.
        /// </summary>
        event EventHandler<ParkEventArgs> ParkEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the token assigned by the Foundation which is used to identify
        /// this Extension Bin application instance.
        /// Can be used to coordinate with other protocols such as the CSI.
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Gets the mount point of the Extension Bin application package.
        /// </summary>
        string MountPoint { get; }

        /// <summary>
        /// Gets the exception monitor used by the Extension Bin.
        /// </summary>
        IExceptionMonitor ExceptionMonitor { get; }

        /// <summary>
        /// Gets the transactional event queue associated with the Extension Bin.
        /// </summary>
        IEventQueue TransactionalEventQueue { get; }

        /// <summary>
        /// Gets the non transactional event queue associated with the Extension Bin.
        /// </summary>
        IEventQueue NonTransactionalEventQueue { get; }

        /// <summary>
        /// Gets the id of the last opened transaction.
        /// </summary>
        uint LastTransactionId { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Establishes the F2X connection to the Foundation.
        /// </summary>
        /// <devdoc>
        /// Must be called following the construction of ExtensionBinLib.
        /// It is part of the initialization that puts the ExtensionBinLib
        /// in a workable state.
        /// </devdoc>
        /// <returns>
        /// True if connection is established successfully; False otherwise.
        /// </returns>
        bool ConnectToFoundation();

        /// <summary>
        /// Disconnect from the Foundation.
        /// </summary>
        /// <returns>True if the connection is terminated successfully.</returns>
        bool DisconnectFromFoundation();

        /// <summary>
        /// Initiates a heavyweight action request.
        /// </summary>
        /// <param name="transactionName">A name to associate with the heavyweight transaction.</param>
        void ActionRequest(string transactionName = null);

        #endregion
    }
}