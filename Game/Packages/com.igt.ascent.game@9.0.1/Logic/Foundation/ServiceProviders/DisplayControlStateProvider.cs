//-----------------------------------------------------------------------
// <copyright file = "DisplayControlStateProvider.cs" company = "IGT">
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
    ///    Core Logic Service Provider for Display Control State. Retrieves display control state
    ///    from the foundation.
    /// </summary>
    public class DisplayControlStateProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        #region Constructors

        /// <summary>
        ///    Constructor for Display Control State provider.
        /// </summary>
        /// <param name="iGameLib">
        ///    Interface to GameLib, GameLib is responsible for communication with
        ///    the foundation.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="iGameLib"/> is null.</exception>
        public DisplayControlStateProvider(IGameLib iGameLib)
        {
            if (iGameLib == null)
            {
                throw new ArgumentNullException("iGameLib", "Parameter cannot be null.");
            }

            displayControlState = DisplayControlState.DisplayAsHidden;
            iGameLib.DisplayControlEvent += OnDisplayControlStateChanged;
        }

        #endregion

        /// <summary>
        /// Provides generic access to the Display Control State.
        /// </summary>
        /// <returns>The display state as a string.</returns>
        [AsynchronousGameService]
        public DisplayControlState DisplayControlState
        {
            get { return displayControlState; }
        }

        #region Private Members

        private DisplayControlState displayControlState;

        /// <summary>
        /// Handler for display control state events from the foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="displayStatusEvent">The display state event arguments</param>
        private void OnDisplayControlStateChanged(object sender, DisplayControlEventArgs displayStatusEvent)
        {
            var tempHandler = AsynchronousProviderChanged;
            displayControlState = displayStatusEvent.DisplayControlState;

            if (tempHandler != null)
            {
                tempHandler(this, new AsynchronousProviderChangedEventArgs("DisplayControlState"));
            }
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritdoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion

        #region IGameLibEventListener Members

        /// <inheritdoc />
        public void UnregisterGameLibEvents(IGameLib gameLib)
        {
            gameLib.DisplayControlEvent -= OnDisplayControlStateChanged;
        }

        #endregion
    }
}
