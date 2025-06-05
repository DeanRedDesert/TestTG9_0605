//-----------------------------------------------------------------------
// <copyright file = "ThemeContextSessionProvider.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;
    using Session;

    /// <summary>
    /// This class is to provide the service to indicate if the theme context has been changed.
    /// </summary>
    public class ThemeContextSessionProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        /// <summary>
        /// Provides a unique identifier each time theme context changes.
        /// </summary>
        /// <remarks>
        /// <see cref="ThemeContextSession" /> is updated whenever a new theme context event comes from the Foundation, 
        /// including theme switching, game mode changing and denomination changing,
        /// in contrast to <see cref="NewThemeSelectionSession" /> which is updated only when a new theme is selected.
        ///</remarks>
        [AsynchronousGameService]
        public UniqueIdentifier ThemeContextSession{get; private set; }

        /// <summary>
        /// Provides a unique identifier each time theme changes.
        /// </summary>
        /// <remarks>
        /// <see cref="NewThemeSelectionSession" /> is updated only when a new theme is selected, 
        /// in contrast to <see cref="ThemeContextSession" /> which is updated 
        /// whenever a new theme context event comes from the Foundation, 
        /// including theme switching, game mode changing and denomination changing.
        ///</remarks>
        [AsynchronousGameService]
        public UniqueIdentifier NewThemeSelectionSession { get; private set; }

        /// <summary>
        /// Provide information included theme context session identifier.
        /// </summary>
        /// <param name="gameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="gameLib"/> is null.</exception>
        public ThemeContextSessionProvider(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException("gameLib", "Parameter cannot be null.");
            }

            gameLib.ActivateThemeContextEvent += HandleActivateThemeContextEvent;
            gameLib.NewThemeSelectionEvent += HandleNewThemeSelectionEvent;
            gameLib.SwitchThemeContextEvent += HandleSwitchThemeContextEvent;
            ThemeContextSession = UniqueIdentifier.New();
            NewThemeSelectionSession = UniqueIdentifier.New();
        }

        /// <summary>
        /// Handles changes when theme context is activated.
        /// </summary>
        /// <param name="sender">GameLib posting the event.</param>
        /// <param name="eventArgs">Event arguments for activating theme context.</param>
        private void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs eventArgs)
        {
            ThemeContextSession = UniqueIdentifier.New();

            var handler = AsynchronousProviderChanged;
            if(handler != null)
            {
                handler(this, new AsynchronousProviderChangedEventArgs("ThemeContextSession"));
            }
        }

        /// <summary>
        /// Handles changes when new theme selected.
        /// </summary>
        /// <param name="sender">GameLib posting the event.</param>
        /// <param name="eventArgs">Event arguments for selected theme.</param>
        private void HandleNewThemeSelectionEvent(object sender, NewThemeSelectionEventArgs eventArgs)
        {
            UpdateNewThemeSelectionSession();
        }

        /// <summary>
        /// Handles changes when Switch Theme Context.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="eventArgs">The event data.</param>
        private void HandleSwitchThemeContextEvent(object sender, SwitchThemeContextEventArgs eventArgs)
        {
            UpdateNewThemeSelectionSession();
        }

        /// <summary>
        /// Update the NewThemeSelectionSession for a new theme session.
        /// </summary>
        private void UpdateNewThemeSelectionSession()
        {
            NewThemeSelectionSession = UniqueIdentifier.New();

            var handler = AsynchronousProviderChanged;
            if(handler != null)
            {
                handler(this, new AsynchronousProviderChangedEventArgs("NewThemeSelectionSession"));
            }
        }

        /// <summary>
        /// Raised when ThemeContextSession changes.
        /// </summary>
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib gameLib)
        {
            gameLib.ActivateThemeContextEvent -= HandleActivateThemeContextEvent;
            gameLib.NewThemeSelectionEvent -= HandleNewThemeSelectionEvent;
            gameLib.SwitchThemeContextEvent -= HandleSwitchThemeContextEvent;
        }
    }
}