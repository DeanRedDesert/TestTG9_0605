//-----------------------------------------------------------------------
// <copyright file = "IExtensionLibRestricted.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ExtensionLib.Interfaces
{
    using System.Threading;
    using Platform.Interfaces;

    /// <summary>
    /// ExtensionLib interface which contains functionality that should
    /// not be used by client code in most cases.
    /// </summary>
    public interface IExtensionLibRestricted
    {
        #region Properties

        /// <summary>
        /// Get the token assigned by the Foundation which is used to identify this extension instance.
        /// Can be used to coordinate with other protocols such as the CSI.
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Get the Boolean flag indicating if there is currently an open transaction.
        /// </summary>
        bool TransactionOpen { get; }

        /// <summary>
        /// Gets the id of the last opened transaction.
        /// </summary>
        uint LastTransactionId { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Connect to the Foundation.  Must be called after the construction
        /// of Extension Lib.  It is part of the initialization that puts the Extension
        /// Lib in a workable state, for both Standard and Standalone Extension Lib.
        /// </summary>
        /// <returns>True if connection is established successfully; False otherwise.</returns>
        bool ConnectToFoundation();

        /// <summary>
        /// Disconnect from the Foundation.
        /// </summary>
        /// <returns>True if the connection is terminated successfully.</returns>
        bool DisconnectFromFoundation();

        /// <summary>
        /// Create an extension initiated transaction with the foundation.
        /// </summary>
        /// <returns>
        /// <see cref="ErrorCode.NoError"/>
        /// if an extension initiated transaction is created successfully.
        /// <see cref="ErrorCode.OpenTransactionExisted"/>
        /// if an extension initiated transaction is already open.
        /// <see cref="ErrorCode.EventWaitingForProcess"/>
        /// if there are events waiting in queue to be processed.
        /// The game should go process the events when receiving
        /// this error code.
        /// </returns>
        ErrorCode CreateTransaction();

        /// <summary>
        /// Create an extension initiated transaction with the foundation with a name.
        /// </summary>
        /// <param name="name">Name of the transaction to be put into the request payload.</param>
        /// <returns>
        /// <see cref="ErrorCode.NoError"/>
        /// if an extension initiated transaction is created successfully.
        /// <see cref="ErrorCode.OpenTransactionExisted"/>
        /// if an extension initiated transaction is already open.
        /// <see cref="ErrorCode.EventWaitingForProcess"/>
        /// if there are events waiting in queue to be processed.
        /// The game should go process the events when receiving
        /// this error code.
        /// </returns>
        ErrorCode CreateTransaction(string name);

        /// <summary>
        /// Close an open game initiated transaction with the foundation.
        /// </summary>
        /// <returns>
        /// <see cref="ErrorCode.NoError"/>
        /// if an extension initiated transaction is closed successfully.
        /// <see cref="ErrorCode.NoTransactionOpen"/>
        /// if no open game initiated transaction is available to be closed.
        /// </returns>
        ErrorCode CloseTransaction();

        /// <summary>
        /// Process any pending events, or wait for an event and then process it.
        /// If any of the passed waitHandles are signaled, then the function will 
        /// unblock.
        /// </summary>
        /// <param name="timeout">
        /// If no events are available, then this specifies the amount of time
        /// to wait for an event. If the timeout is 0, then the function will
        /// return immediately after processing any pending events. If the
        /// timeout is Timeout.Infinite, then the function will not return until
        /// an event has been processed.
        /// </param>
        /// <param name="waitHandles">
        /// An array of additional wait handles that will unblock the function.
        /// </param>
        /// <returns>
        /// The supplied wait handle that unblocked the function, or 
        /// <see langword="null"/> if the function was unblocked for another reason.
        /// </returns>
        WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles);

        /// <summary>
        /// Get a restricted service interface.
        /// This allows for extension of platform features beyond <see cref="IExtensionLibRestricted"/>.
        /// </summary>
        /// <typeparam name="TServiceInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. If no implementation can be accessed,
        /// then <see langword="null"/> will be returned.
        /// </returns>
        TServiceInterface GetServiceInterface<TServiceInterface>() where TServiceInterface : class;

        #endregion
    }
}
