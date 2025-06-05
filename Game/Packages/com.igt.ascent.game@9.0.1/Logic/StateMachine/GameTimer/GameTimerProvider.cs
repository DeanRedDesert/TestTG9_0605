//-----------------------------------------------------------------------
// <copyright file = "GameTimerProvider.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    using Services;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A provider for the events raised by an <see cref="IGameTimer"/>.
    /// </summary>
    public class GameTimerProvider : INotifyAsynchronousProviderChanged
    {
        private GameTimerEventArgs gameTimerEventArguments;
        private const string GameTimerServiceName = "GameTimerEvent";

        /// <summary>
        /// GameTimerEvent game service.
        /// </summary>
        /// <returns>An instance of <see cref="GameTimerEventArgs"/> that hold the most recent timer event arguments for
        /// the game timer providing this event.</returns>
        [AsynchronousGameService]
        public GameTimerEventArgs GameTimerEvent
        {
            get
            {
                return gameTimerEventArguments;
            }
        }

        /// <summary>
        /// Constructs an instance of <see cref="GameTimerProvider"/>.
        /// </summary>
        /// <param name="gameTimer">An non null <see cref="IGameTimer"/> reference.</param>
        /// <exception cref="ArgumentNullException">Raised if the provided <see cref="IGameTimer"/> is null.</exception>
        public GameTimerProvider(IGameTimer gameTimer)
        {
            if(gameTimer == null)
            {
                throw new ArgumentNullException("gameTimer");
            }

            gameTimer.GameTimerConsumerEvent += HandleGameTimerTimerProviderSourceEvent;
            Name = gameTimer.Name + "Provider";
        }

        /// <summary>
        /// Gets/private sets the current timer provider name.
        /// </summary>
        public string Name 
        { 
            get; 
            private set; 
        }

        /// <summary>
        /// Handles a game timer event, modifies the game service property, and raises the AsynchronousProviderChanged event.
        /// </summary>
        /// <param name="sender">The object that raised this event.</param>
        /// <param name="eventArgs">Event args of type <see cref="GameTimerEventArgs"/>.</param>
        private void HandleGameTimerTimerProviderSourceEvent(object sender, GameTimerEventArgs eventArgs)
        {
            gameTimerEventArguments = new GameTimerEventArgs(eventArgs);

            OnServiceUpdated(GameTimerServiceName, null);
        }

        /// <summary>
        /// Post the AsynchronousProviderChanged event when an asynchronous member is changed.
        /// </summary>
        /// <param name="serviceName">Name of the service that has changed.</param>
        /// <param name="serviceArguments">Arguments to the changed service.</param>
        protected void OnServiceUpdated(string serviceName, IDictionary<string, object> serviceArguments)
        {
            // Create a temporary copy of the event handler for thread safety.
            var handler = AsynchronousProviderChanged;

            // If there are any handlers registered with this event.
            if(handler != null)
            {
                // Post the event with this service provider as the sender and the parameter values passed in.
                handler(this, new AsynchronousProviderChangedEventArgs(serviceName, serviceArguments, false));
            }
        }

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion
    }
}
