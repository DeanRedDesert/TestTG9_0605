//-----------------------------------------------------------------------
// <copyright file = "GameContextModeProvider.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;

    /// <summary>
    /// This class is to provide the service to get current game mode.
    /// </summary>
    public class GameContextModeProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        /// <summary>
        /// Reference to the current GameLib.
        /// </summary>
        private readonly IGameLib gameLib;

        /// <summary>
        /// Get current game mode.
        /// </summary>
        [AsynchronousGameService]
        public GameMode GameContextMode
        {
            get { return gameLib.GameContextMode; }
        }

        /// <summary>
        /// Provide information included game mode.
        /// </summary>
        /// <param name="gameLib">GameLib used to determine what the game mode is.</param>
        public GameContextModeProvider(IGameLib gameLib)
        {
            this.gameLib = gameLib;
            gameLib.ActivateThemeContextEvent += HandleActivateThemeContextEvent;
            gameLib.InactivateThemeContextEvent += HandleInactivateThemeContextEvent;
        }

        /// <summary>
        /// Handles changes when theme context is activated.
        /// Game Mode can only change on activate and inactivate context events.
        /// </summary>
        /// <param name="sender">Game lib posting the event.</param>
        /// <param name="e">Event args for activating theme context.</param>
        private void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs e)
        {
            var handler = AsynchronousProviderChanged;
            if(handler != null)
            {
                handler(this, new AsynchronousProviderChangedEventArgs("GameContextMode"));
            }
        }

        /// <summary>
        /// Handles changes when theme context is inactivated.
        /// Game Mode can only change on activate and inactivate context events.
        /// </summary>
        /// <param name="sender">Game lib posting the event.</param>
        /// <param name="e">Event args for inactivating theme context.</param>
        private void HandleInactivateThemeContextEvent(object sender, InactivateThemeContextEventArgs e)
        {
            var handler = AsynchronousProviderChanged;
            if (handler != null)
            {
                handler(this, new AsynchronousProviderChangedEventArgs("GameContextMode"));
            }
        }

        /// <summary>
        /// Raised when GameContextMode changes.
        /// </summary>
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib iGameLib)
        {
            gameLib.ActivateThemeContextEvent -= HandleActivateThemeContextEvent;
            gameLib.InactivateThemeContextEvent -= HandleInactivateThemeContextEvent;
        }
    }
}