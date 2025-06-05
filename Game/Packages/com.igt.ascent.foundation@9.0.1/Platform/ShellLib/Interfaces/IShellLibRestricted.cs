// -----------------------------------------------------------------------
// <copyright file = "IShellLib.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Threading;
    using Platform.Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// This interface defines restricted functionality of a shell communication lib.
    /// The restricted functionality is supposed to be used by higher level SDK components only.
    /// Game development should not use this interface.
    /// </summary>
    public interface IShellLibRestricted
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
        /// Occurs when a new shell context is about to start.
        /// </summary>
        event EventHandler<NewShellContextEventArgs> NewShellContextEvent;

        /// <summary>
        /// Occurs when a new shell context is activated.
        /// </summary>
        event EventHandler<ActivateShellContextEventArgs> ActivateShellContextEvent;

        /// <summary>
        /// Occurs when the current shell context is inactivated.
        /// </summary>
        event EventHandler<InactivateShellContextEventArgs> InactivateShellContextEvent;

        /// <summary>
        /// Occurs when the display control state is changed.
        /// </summary>
        event EventHandler<DisplayControlEventArgs> DisplayControlEvent;

        /// <summary>
        /// Occurs when the shell is being shut down by the Foundation.
        /// </summary>
        /// <remarks>
        /// This event is not associated with a transaction.
        /// It is raised on a communication thread. Therefore, handler of this event must be thread safe.
        /// </remarks>
        event EventHandler<ShutDownEventArgs> ShutDownEvent;

        /// <summary>
        /// Occurs when the shell is being parked by the Foundation.
        /// </summary>
        event EventHandler<ParkEventArgs> ParkEvent;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the token assigned by the Foundation which is used to identify
        /// this shell application instance.
        /// Can be used to coordinate with other protocols such as the CSI.
        /// </summary>
        string Token { get; }

        /// <summary>
        /// Gets the mount point of the shell application package.
        /// </summary>
        string MountPoint { get; }

        /// <summary>
        /// Gets the exception monitor used by the shell session.
        /// </summary>
        IExceptionMonitor ExceptionMonitor { get; }

        /// <summary>
        /// Gets the transactional event queue associated with the shell session.
        /// </summary>
        IEventQueue TransactionalEventQueue { get; }

        /// <summary>
        /// Gets the non transactional event queue associated with the shell session.
        /// </summary>
        IEventQueue NonTransactionalEventQueue { get; }

        /// <summary>
        /// Gets the display control state of the shell application.
        /// </summary>
        DisplayControlState DisplayControlState { get; }
        
        /// <summary>
        /// Gets the interface for accessing shell's history critical data during play mode.
        /// </summary>
        IShellHistoryStore ShellHistoryStore { get; }

        /// <summary>
        /// Gets the interface for accessing shell's history context and critical data during history mode.
        /// </summary>
        IShellHistoryControl ShellHistoryControl { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Establishes the F2X connection to the Foundation.
        /// </summary>
        /// <devdoc>
        /// Must be called following the construction of Shell Lib.
        /// It is part of the initialization that puts the Shell Lib
        /// in a workable state.
        /// </devdoc>
        /// <returns>
        /// True if connection is established successfully; False otherwise.
        /// </returns>
        bool ConnectToFoundation();

        /// <summary>
        /// Requests all the selectable theme information for this shell.
        /// </summary>
        /// <returns>A list of selectable theme information.</returns>
        IList<ShellThemeInfo> GetSelectableThemes();

        /// <summary>
        /// Requests all the information of the existing coplayers for this shell.
        /// </summary>
        /// <returns>A list of coplayer information.</returns>
        IList<CoplayerInfo> GetCoplayers();

        /// <summary>
        /// Requests the theme information for a given theme.
        /// </summary>
        /// <param name="themeIdentifier">The given theme identifier.</param>
        /// <returns>The requested theme information for the theme.</returns>
        ShellThemeInfo GetThemeInformation(IdentifierToken themeIdentifier);

        /// <summary>
        /// Requests for creating coplayers as many as specified.
        /// </summary>
        /// <param name="coplayerCount">The count of coplayers to create.</param>
        /// <returns>A list of coplayer numbers for the coplayers that are newly created.</returns>
        IList<int> CreateCoplayers(int coplayerCount);

        /// <summary>
        /// Switches a theme on the specified coplayer.
        /// </summary>
        /// <param name="coplayerId">The specified coplayer on which the theme switch happens.</param>
        /// <param name="themeIdentifier">Identifies the theme to be selected for the coplayer.</param>
        /// <param name="denomination">Identifies the denomination to be selected for the coplayer.</param>
        /// <returns>True if the theme switch was successful, false otherwise.</returns>
        bool SwitchCoplayerTheme(int coplayerId, IdentifierToken themeIdentifier, long denomination);

        /// <summary>
        /// Remove the existing theme from a coplayer.  The coplayer then becomes vacant.
        /// </summary>
        /// <param name="coplayerId">The specified coplayer whose theme is to be removed.</param>
        /// <returns>True if the theme removal was successful, false otherwise.</returns>
        bool RemoveCoplayerTheme(int coplayerId);

        /// <summary>
        /// Requests to create a new session.
        /// </summary>
        /// <returns>The session number for the newly created session.</returns>
        int CreateSession();

        /// <summary>
        /// Requests to bind a session to a coplayer.
        /// </summary>
        /// <param name="coplayerId">Identifies the coplayer to bind with the session.</param>
        /// <param name="sessionId">Identifies the session to bind.</param>
        void BindCoplayerSession(int coplayerId, int sessionId);

        /// <summary>
        /// Requests to launch the specific coplayer session.
        /// </summary>
        /// <param name="sessionId">Identifies the session to be launched.</param>
        void LaunchSession(int sessionId);

        /// <summary>
        /// Requests to destroy the specific coplayer session.
        /// </summary>
        /// <param name="sessionId">Identifies the session to be destroyed.</param>
        void DestroySession(int sessionId);

        /// <summary>
        /// Initiates a heavyweight action request.
        /// </summary>
        /// <param name="transactionName">A name to associate with the heavyweight transaction.</param>
        void ActionRequest(string transactionName = null);

        /// <summary>
        /// Initiates a lightweight action request.
        /// </summary>
        /// <param name="transactionName">A name to associate with the lightweight transaction.</param>
        void ActionRequestLite(string transactionName = null);

        #endregion
    }
}