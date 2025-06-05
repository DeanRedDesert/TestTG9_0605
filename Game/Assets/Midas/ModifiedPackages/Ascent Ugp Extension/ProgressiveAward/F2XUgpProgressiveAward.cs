//-----------------------------------------------------------------------
// <copyright file = "F2XUgpProgressiveAward.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpProgressiveAward extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpProgressiveAward : IUgpProgressiveAward, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpProgressiveAward category handler.
        /// </summary>
        private readonly IUgpProgressiveAwardCategory ugpProgressiveAwardCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpProgressiveAward"/>.
        /// </summary>
        /// <param name="ugpProgressiveAwardCategory">
        /// The UgpProgressiveAward category used to communicate with the foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XUgpProgressiveAward(IUgpProgressiveAwardCategory ugpProgressiveAwardCategory,
                                      IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.ugpProgressiveAwardCategory = ugpProgressiveAwardCategory ?? throw new ArgumentNullException(nameof(ugpProgressiveAwardCategory));

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
            ugpProgressiveAwardCategory.StartingProgressiveAward(progressiveAwardIndex, progressiveLevelId, defaultVerifiedAmount);
        }

        /// <inheritdoc/>
        public void FinishedDisplay(int progressiveAwardIndex, string progressiveLevelId, long defaultPaidAmount)
        {
            ugpProgressiveAwardCategory.FinishedDisplay(progressiveAwardIndex, progressiveLevelId, defaultPaidAmount);
        }

        #endregion
    }
}
