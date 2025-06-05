// -----------------------------------------------------------------------
// <copyright file = "IShellRunner.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using System;
    using Communication.Platform.Interfaces;
    using Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// This interface defines functionalities of the object running the shell logic main thread.
    /// It is to be used by SDK entry scripts only.  Other SDK or game specific code should NOT use it.
    /// </summary>
    public interface IShellRunner
    {
        #region Properties

        /// <summary>
        /// Gets the logic token for the shell application.
        /// </summary>
        string Token {get; }

        /// <summary>
        /// Gets the mount point of the shell package.
        /// </summary>
        string MountPoint { get; }

        /// <summary>
        /// Gets the GL2P comm manager from which the presentation
        /// can retrieve a comm channel for GL2P communication.
        /// </summary>
        IGl2PCommManager Gl2PCommManager { get; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the shell is being shut down by the Foundation.
        /// </summary>
        /// <remarks>
        /// This event is not associated with a transaction.
        /// It is raised on a communication thread. Therefore, handler of this event must be thread safe.
        /// </remarks>
        event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <summary>
        /// Occurs during context activation to notify games that their presentation should be unparked 
        /// if applicable.
        /// </summary>
        event EventHandler<EventArgs> Unpark;

        /// <summary>
        /// Occurs when the foundation tells the shell to park its presentation. 
        /// </summary>
        event EventHandler<EventArgs> Park;

        #endregion

        #region Methods

        /// <summary>
        /// Establishes the F2X connection to the Foundation.
        /// </summary>
        /// <returns>
        /// True if connection is established successfully; False otherwise.
        /// </returns>
        bool ConnectToFoundation();

        /// <summary>
        /// Initializes the shell runner instance.
        /// </summary>
        /// <devdoc>
        /// Separate this initialization from constructor in case
        /// ShellEntryScript needs to call them at different times
        /// for better game loading performance.
        /// </devdoc>
        void Initialize(IServiceRequestDataManager requestDataManager);

        #endregion
    }
}