//-----------------------------------------------------------------------
// <copyright file = "AncillaryGameOfferProvider.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using Services;

    /// <summary>
    /// Core Logic Service Provider for the ancillary game offer. 
    /// Provide the ancillary game availability and monitor if the Foundation has disabled it.
    /// </summary>
    public class AncillaryGameOfferProvider : INotifyAsynchronousProviderChanged, IGameLibEventListener
    {
        #region Constructors

        /// <summary>
        /// Constructor for OfferAncillaryProvider.
        /// </summary>
        /// <param name="iGameLib">
        /// Interface to GameLib, GameLib is responsible for communication with
        /// the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="iGameLib"/> is null.</exception>
        public AncillaryGameOfferProvider(IGameLib iGameLib)
        {
            if(iGameLib == null)
            {
                throw new ArgumentNullException("iGameLib", "Parameter cannot be null.");
            }
            gameLib = iGameLib;
            gameLib.DisableAncillaryGameOfferEvent += OnDisableAncillaryGameOffer;
        }

        #endregion

        #region Game Services

        /// <summary>
        /// Get or set the flag indicating if the ancillary game is currently available.
        /// </summary>
        /// <returns>True if the ancillary game is available.</returns>
        [AsynchronousGameService]
        public bool IsAncillaryGameAvailable
        {
            get
            {
                // The CriticalDataScope could not be GameCycle,
                // since this provider need to be access in idle state.
                return gameLib.ReadCriticalData<bool>(CriticalDataScope.Payvar, AncillaryGameAvailablePath);
            }
            set
            {
                gameLib.WriteCriticalData(CriticalDataScope.Payvar, AncillaryGameAvailablePath, value);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Request the Foundation to offer the ancillary game.
        /// </summary>
        /// <param name="mainPlayWinAmount">The main game play win amount in base units.</param>
        /// <returns>True if the ancillary game is offered; otherwise, false.</returns>
        public bool RequestOfferingAncillaryGame(long mainPlayWinAmount)
        {
            return gameLib.AncillaryEnabled &&
                   mainPlayWinAmount > 0 &&
                   checked(mainPlayWinAmount * 2) <= gameLib.AncillaryMonetaryLimit &&
                   gameLib.OfferAncillaryGame();
        }

        #endregion

        #region Private Members

        /// <summary>
        /// Critial data path for DoubleUp availability flag.
        /// </summary>
        private const string AncillaryGameAvailablePath = "AncillaryGameProvider/AncillaryGameAvailable";

        /// <summary>
        /// Interface to GameLib, which is responsible for communication with the foundation.
        /// </summary>
        private readonly IGameLib gameLib;

        /// <summary>
        /// Handler for disable Ancillary Game Offer events from the foundation.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="args">The event arguments.</param>
        private void OnDisableAncillaryGameOffer(object sender, DisableAncillaryGameOfferEventArgs args)
        {
            IsAncillaryGameAvailable = false;

            // Send out async service update only in case.
            var tempHandler = AsynchronousProviderChanged;
            if(tempHandler != null)
            {
                tempHandler(this, new AsynchronousProviderChangedEventArgs("IsAncillaryGameAvailable"));
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
            gameLib.DisableAncillaryGameOfferEvent -= OnDisableAncillaryGameOffer;
        }

        #endregion
    }
}