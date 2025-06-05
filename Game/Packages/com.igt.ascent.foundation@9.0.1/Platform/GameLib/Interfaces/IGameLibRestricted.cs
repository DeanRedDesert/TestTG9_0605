//-----------------------------------------------------------------------
// <copyright file = "IGameLibRestricted.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System.Collections.Generic;
    using System.Threading;
    using Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces;
    using Platform.Interfaces;

    /// <summary>
    /// GameLib interface which contains functionality that should
    /// not be used by client code in most cases.
    /// </summary>
    public interface IGameLibRestricted
    {
        /// <summary>
        /// Connect to the Foundation.  Must be called after the construction
        /// of Game Lib.  It is part of the initialization that puts the Game
        /// Lib in a workable state, for both Standard and Standalone Game Lib.
        /// </summary>
        /// <returns>True if connection is established successfully; False otherwise.</returns>
        bool ConnectToFoundation();

        /// <summary>
        /// Connect to the Foundation.  Must be called after the construction
        /// of Game Lib.  It is part of the initialization that puts the Game
        /// Lib in a workable state, for both Standard and Standalone Game Lib.
        /// </summary>
        /// <param name="additionalInterfaceConfigurations">
        /// List of additional interface configurations to install in this gamelib.
        /// </param>
        /// <remarks>
        /// Requested interface configurations may be unavailable if not supported by the underlying platform.
        /// </remarks>
        /// <returns>True if connection is established successfully; False otherwise.</returns>
        bool ConnectToFoundation(IEnumerable<IInterfaceExtensionConfiguration> additionalInterfaceConfigurations);

        /// <summary>
        /// Disconnect from the Foundation.
        /// </summary>
        /// <returns>True if the connection is terminated successfully.</returns>
        bool DisconnectFromFoundation();

        /// <summary>
        /// Create a game initiated transaction with the foundation.
        /// </summary>
        /// <returns>
        /// <see cref="ErrorCode.NoError"/>
        /// if a game initiated transaction is created successfully.
        /// <see cref="ErrorCode.OpenTransactionExisted"/>
        /// if a game initiated transaction is already open.
        /// <see cref="ErrorCode.EventWaitingForProcess"/>
        /// if there are events waiting in queue to be processed.
        /// The game should go process the events when receiving
        /// this error code.
        /// </returns>
        ErrorCode CreateTransaction();

        /// <summary>
        /// Create a game initiated transaction with the foundation with a name.
        /// </summary>
        /// <param name="name">Name of the transaction to be put into the request payload.</param>
        /// <returns>
        /// <see cref="ErrorCode.NoError"/>
        /// if a game initiated transaction is created successfully.
        /// <see cref="ErrorCode.OpenTransactionExisted"/>
        /// if a game initiated transaction is already open.
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
        /// if a game initiated transaction is closed successfully.
        /// <see cref="ErrorCode.NoTransactionOpen"/>
        /// if no open game initiated transaction is available to be closed.
        /// </returns>
        ErrorCode CloseTransaction();

        /// <summary>
        /// Gets a boolean indicating if there is currently a game initiated transaction open.
        /// </summary>
        bool TransactionOpen { get; }

        /// <summary>
        /// Process any pending events, or wait for an event and then process it.
        /// Once processing events, it will keep waiting and processing
        /// incoming events until being notified by the Foundation that no
        /// more events are coming in.
        /// </summary>
        /// <param name="timeout">
        /// If no events are available, then this specifies the amount of time
        /// to wait for an event. If the timeout is 0, then the function will
        /// return immediately after processing any pending events. If the
        /// timeout is Timeout.Infinite, then the function will not return until
        /// an event has been processed.
        /// </param>
        /// <remarks>
        /// The Foundation will not post any event if there is a game initiated
        /// transaction open.  Therefore, make sure to close any game initiated
        /// transaction first before calling this function.
        /// </remarks>
        void ProcessEvents(int timeout);

        /// <summary>
        /// Process any pending events, or wait for an event and then process it.
        /// This function does not take a timeout, so it will wait indefinitely.
        /// Once processing events, it will keep waiting and processing
        /// incoming events until being notified by the Foundation that no
        /// more events are coming in.
        /// </summary>
        /// <remarks>
        /// The Foundation will not post any event if there is a game initiated
        /// transaction open.  Therefore, make sure to close any game initiated
        /// transaction first before calling this function.
        /// </remarks>
        void ProcessEvents();

        /// <summary>
        /// Process any pending events, or wait for an event and then process it.
        /// If any of the passed waitHandles are signaled, then the function will 
        /// unblock. This function does not take a timeout, so it will wait 
        /// indefinitely. Once processing events, it will keep waiting and 
        /// processing incoming events until being notified by the Foundation 
        /// that no more events are coming in.
        /// </summary>
        /// <param name="waitHandles">
        ///     An array of additional wait handles that will block the function.
        /// </param>
        /// <returns>
        ///     The supplied wait handle that unblocked the function, or 
        ///     <see langword="null"/> if the function was unblocked for another reason.
        /// </returns>
        WaitHandle ProcessEvents(WaitHandle[] waitHandles);

        /// <summary>
        /// Process any pending events, or wait for an event and then process it.
        /// If any of the passed waitHandles are signaled, then the function will 
        /// unblock. Once processing events, it will keep waiting and processing
        /// incoming events until being notified by the Foundation that no
        /// more events are coming in.
        /// </summary>
        /// <param name="timeout">
        ///     If no events are available, then this specifies the amount of time
        ///     to wait for an event. If the timeout is 0, then the function will
        ///     return immediately after processing any pending events. If the
        ///     timeout is Timeout.Infinite, then the function will not return until
        ///     an event has been processed.</param>
        /// <param name="waitHandles">
        ///     An array of additional wait handles that will block the function.
        /// </param>
        /// <returns>
        ///     The supplied wait handle that unblocked the function, or 
        ///     <see langword="null"/> if the function was unblocked for another reason.
        /// </returns>
        WaitHandle ProcessEvents(int timeout, WaitHandle[] waitHandles);

        /// <summary>
        /// Set the optional <see cref="IPrepickedValueProvider"/> to use when filling random value requests.
        /// </summary>
        /// <param name="providerToUse">An implementation of the <see cref="IPrepickedValueProvider"/> interface.</param>
        void SetPrepickedValueProvider(IPrepickedValueProvider providerToUse);

        /// <summary>
        /// A token assigned by the Foundation which is used to identify this game instance. Can be used to coordinate
        /// with other protocols such as the CSI.
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Get a restricted service interface.
        /// This allows for extension of platform features beyond <see cref="IGameLibRestricted"/>.
        /// </summary>
        /// <typeparam name="TServiceInterface">
        /// Interface to get an implementation of.
        /// </typeparam>
        /// <returns>
        /// An implementation of the interface. If no implementation can be accessed,
        /// then <see langword="null"/> will be returned.
        /// </returns>
        TServiceInterface GetServiceInterface<TServiceInterface>() where TServiceInterface : class;
    }
}
