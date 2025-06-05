//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpExternalJackpots.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using System.Collections.Generic;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpExternalJackpots extended interface.
    /// </summary>
    internal sealed class StandaloneUgpExternalJackpots : IUgpExternalJackpots,
                                                          IStandaloneHelperUgpExternalJackpots, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The interface for posting foundation events in the main event queue.
        /// </summary>
        private readonly IStandaloneEventPosterDependency eventPoster;

		/// <summary>
		/// Standalone external jackpots data.
		/// </summary>
		private ExternalJackpots jackpots;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneUgpExternalJackpots"/>.
        /// </summary>
        /// <param name="eventPosterInterface">
        /// The interface for processing and posting foundation events in the main event queue.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public StandaloneUgpExternalJackpots(IStandaloneEventPosterDependency eventPosterInterface,
                                             IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            eventPoster = eventPosterInterface ?? throw new ArgumentNullException(nameof(eventPosterInterface));

            transactionalEventDispatcher.EventDispatchedEvent += HandleExternalJackpotChangedEvent;

            jackpots = new ExternalJackpots();
        }

        #endregion

        #region IUgpExternalJackpots Implementation

		/// <inheritdoc/>
		public ExternalJackpots GetExternalJackpots()
		{
			return jackpots;
		}

		/// <inheritdoc/>
        public event EventHandler<ExternalJackpotChangedEventArgs> ExternalJackpotChanged;

        #endregion

        #region IStandaloneHelperUgpExternalJackpots Implementation

        /// <inheritdoc/>
        public void SetExternalJackpots(bool isVisible, int iconId, List<ExternalJackpot> externalJackpots)
        {
            jackpots.IsVisible = isVisible;
            jackpots.IconId = iconId;
            jackpots.Jackpots = externalJackpots;

            eventPoster.PostTransactionalEvent(new ExternalJackpotChangedEventArgs(jackpots));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the dispatched event if the dispatched event is an ExternalJackpotChanged event.
        /// </summary>
        /// <param name="sender">
        /// The sender of the dispatched event.
        /// </param>
        /// <param name="dispatchedEventArgs">
        /// The arguments used for processing the dispatched event.
        /// </param>
        private void HandleExternalJackpotChangedEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEvent is ExternalJackpotChangedEventArgs eventArgs)
            {
                jackpots = eventArgs.ExternalJackpots;

                var handler = ExternalJackpotChanged;
                if(handler != null)
                {
                    handler(this, eventArgs);

                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        #endregion
    }
}
