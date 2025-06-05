//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Standalone implementation of the UgpProgressiveAward extended interface.
    /// </summary>
    internal sealed class StandaloneUgpProgressiveAward : IUgpProgressiveAward, IInterfaceExtension,
                                                          IStandaloneHelperUgpProgressiveAward
    {
        #region Private Fields

        /// <summary>
        /// The interface for posting foundation events in the main event queue.
        /// </summary>
        private readonly IStandaloneEventPosterDependency eventPoster;

        /// <summary>
        /// The flag indicating if standalone progressive award is manually controlled.
        /// </summary>
        private bool isManuallyControlled;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneUgpProgressiveAward"/>.
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
        public StandaloneUgpProgressiveAward(IStandaloneEventPosterDependency eventPosterInterface,
                                             IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            eventPoster = eventPosterInterface ?? throw new ArgumentNullException(nameof(eventPosterInterface));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, ProgressiveAwardVerified);
            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, ProgressiveAwardPaid);
        }

        #endregion

        #region IUgpProgressiveAward Implementation

        /// <inheritdoc/>
        public event EventHandler<ProgressiveAwardVerifiedEventArgs> ProgressiveAwardVerified;

        /// <inheritdoc/>
        public event EventHandler<ProgressiveAwardPaidEventArgs> ProgressiveAwardPaid;

        /// <inheritdoc/>
        public void StartingProgressiveAward(int progressiveAwardIndex, string progressiveLevelId, long defaultVerifiedAmount)
        {
            if(!isManuallyControlled)
            {
                eventPoster.PostTransactionalEvent(new ProgressiveAwardVerifiedEventArgs
                {
                    ProgressiveAwardIndex = progressiveAwardIndex,
                    ProgressiveLevelId = progressiveLevelId,
                    VerifiedAmount = defaultVerifiedAmount
                });
            }
        }

        /// <inheritdoc/>
        public void FinishedDisplay(int progressiveAwardIndex, string progressiveLevelId, long defaultPaidAmount)
        {
            if(!isManuallyControlled)
            {
                eventPoster.PostTransactionalEvent(new ProgressiveAwardPaidEventArgs
                {
                    ProgressiveAwardIndex = progressiveAwardIndex,
                    ProgressiveLevelId = progressiveLevelId,
                    PaidAmount = defaultPaidAmount
                });
            }
        }

        #endregion

        #region IStandaloneHelperUgpProgressiveAward Implementation

        /// <inheritdoc/>
        public void SendVerified(int progressiveIndex, long verifiedAmount)
        {
            eventPoster.PostTransactionalEvent(new ProgressiveAwardVerifiedEventArgs
            {
                ProgressiveAwardIndex = progressiveIndex,
                PayType = ProgressiveAwardPayType.CreditMeter,
                VerifiedAmount = verifiedAmount
            });
        }

        /// <inheritdoc/>
        public void SendPaid(int progressiveIndex, long paidAmount)
        {
            eventPoster.PostTransactionalEvent(new ProgressiveAwardPaidEventArgs
            {
                ProgressiveAwardIndex = progressiveIndex,
                PaidAmount = paidAmount
            });
        }

        /// <inheritdoc/>
        public void SetManualControl(bool isControlledManually)
        {
            isManuallyControlled = isControlledManually;
        }

        #endregion
    }
}
