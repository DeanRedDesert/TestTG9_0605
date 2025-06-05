//-----------------------------------------------------------------------
// <copyright file = "DenominationChangeRequestProvider.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Denomination
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Services;

    /// <summary>
    /// Provider for denomination change request related services.
    /// </summary>
    public class DenominationChangeRequestProvider : IGameLibEventListener, INotifyAsynchronousProviderChanged, IDenominationChange
    {
        #region Fields

        /// <summary>
        /// The <see cref="IGameLib"/> instance used to communicate with the Foundation.
        /// </summary>
        private readonly IGameLib gameLib;

        #endregion

        #region Game Services

        /// <summary>
        /// Get the current DenominationChangeState of the game.
        /// </summary>
        [AsynchronousGameService]
        public DenominationChangeState DenominationChangeState { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for Denomination Provider.
        /// </summary>
        /// <param name="gameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="gameLib"/> is null.</exception>
        public DenominationChangeRequestProvider(IGameLib gameLib)
        {
            if (gameLib == null)
            {
                throw new ArgumentNullException("gameLib", "Parameter cannot be null.");
            }

            this.gameLib = gameLib;
            gameLib.ActivateThemeContextEvent += HandleActivateThemeContextEvent;
            gameLib.DenominationChangeCancelledEvent += HandleDenominationChangeCancelledEvent;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handler for the Denomination Change Cancelled Event.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="denominationChangeCancelledEventArgs">The payload of the event.</param>
        private void HandleDenominationChangeCancelledEvent(object sender, DenominationChangeCancelledEventArgs denominationChangeCancelledEventArgs)
        {
            DenominationChangeState = DenominationChangeState.DenominationChangeCancelled;

            var handle = AsynchronousProviderChanged;
            if(handle != null)
            {
                handle(this, new AsynchronousProviderChangedEventArgs("DenominationChangeState"));
            }
        }

        /// <summary>
        /// Handler for the Activate ThemeContext Event.
        /// </summary>
        /// <param name="sender">The event sender</param>
        /// <param name="activateThemeContextEventArgs">The payload of the event.</param>
        private void HandleActivateThemeContextEvent(object sender, ActivateThemeContextEventArgs activateThemeContextEventArgs)
        {
            DenominationChangeState = DenominationChangeState.DenominationSet;
 
            var handle = AsynchronousProviderChanged;
            if(handle != null)
            {
                handle(this, new AsynchronousProviderChangedEventArgs("DenominationChangeState"));
            }
        }

        #endregion

        #region IDenominationChange Members

        /// <inheritDoc />
        DenominationChangeState IDenominationChange.DenominationChangeState
        {
            get { return DenominationChangeState; }
        }

        /// <inheritDoc />
        public bool RequestDenominationChange(long newDenomination)
        {
            bool requestAccepted = gameLib.RequestDenominationChange(newDenomination);

            if(requestAccepted)
            {
                DenominationChangeState = DenominationChangeState.DenominationChanging;
            }

            return requestAccepted;
        }

        #endregion

        #region IGameLibEventListener Members

        /// <inheritDoc />
        public void UnregisterGameLibEvents(IGameLib iGameLib)
        {
            iGameLib.ActivateThemeContextEvent -= HandleActivateThemeContextEvent;
            iGameLib.DenominationChangeCancelledEvent -= HandleDenominationChangeCancelledEvent;
        }

        #endregion

        #region INotifyAsynchronousProviderChanged Members

        /// <inheritDoc />
        public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;

        #endregion
    }
}
