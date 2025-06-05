//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpReserve.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpReserve extended interface.
    /// </summary>
    internal sealed class StandaloneUgpReserve : IUgpReserve, IStandaloneHelperUgpReserve, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The interface for posting foundation events in the main event queue.
        /// </summary>
        private readonly IStandaloneEventPosterDependency eventPoster;

		/// <summary>
		/// Cache of the reserve parameters.
		/// </summary>
		private ReserveParameters reserveParameters;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneUgpReserve"/>.
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
        public StandaloneUgpReserve(IStandaloneEventPosterDependency eventPosterInterface,
                                    IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            eventPoster = eventPosterInterface ?? throw new ArgumentNullException(nameof(eventPosterInterface));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, ReserveParametersChanged);

			reserveParameters = new ReserveParameters(true, true, 20000, 30000);
        }

        #endregion

        #region IUgpReserve Implementation

        /// <inheritdoc/>
        public event EventHandler<ReserveParametersChangedEventArgs> ReserveParametersChanged;

        /// <inheritdoc/>
		public ReserveParameters GetReserveParameters()
		{
			return reserveParameters;
		}

        /// <inheritdoc/>
        public void SendActivationChanged(bool isActive)
        {
        }

        #endregion

        #region IStandaloneHelperUgpReserve Implementation

        /// <inheritdoc/>
        public void SetReserveConfiguration(
                           bool isReserveAllowedWithCredits, bool isReserveAllowedWithoutCredits,
                           long reserveTimeWithCreditsMilliseconds, long reserveTimeWithoutCreditsMilliseconds)
        {
			reserveParameters = new ReserveParameters(
				isReserveAllowedWithCredits,
				isReserveAllowedWithoutCredits,
				reserveTimeWithCreditsMilliseconds,
				reserveTimeWithoutCreditsMilliseconds);

            eventPoster.PostTransactionalEvent(new ReserveParametersChangedEventArgs(reserveParameters));
        }

        #endregion
    }
}
