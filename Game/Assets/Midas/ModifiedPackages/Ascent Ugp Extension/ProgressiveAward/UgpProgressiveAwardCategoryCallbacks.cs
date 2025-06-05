//-----------------------------------------------------------------------
// <copyright file = "UgpProgressiveAwardCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ProgressiveAward
{
    using System;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the UgpProgressiveAward category.
    /// </summary>
    class UgpProgressiveAwardCategoryCallbacks : IUgpProgressiveAwardCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        /// <summary>
        /// Initialize an instance of <see cref="UgpProgressiveAwardCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for the handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public UgpProgressiveAwardCategoryCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #region IUgpProgressiveAwardCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessProgressiveVerified(int progressiveAwardIndex, string progressiveLevelId,
                                                 long verifiedAmount, ProgressiveAwardPayTypeEnum payType)
        {
            eventCallbacksInterface.PostEvent(new ProgressiveAwardVerifiedEventArgs
            {
                ProgressiveAwardIndex = progressiveAwardIndex,
                ProgressiveLevelId = progressiveLevelId,
                PayType = payType.ToPublic(),
                VerifiedAmount = verifiedAmount,
            });

            return null;
        }

        /// <inheritdoc />
        public string ProcessProgressivePaid(int progressiveAwardIndex, string progressiveLevelId,
                                             long paidAmount)
        {
            eventCallbacksInterface.PostEvent(new ProgressiveAwardPaidEventArgs
            {
                ProgressiveAwardIndex = progressiveAwardIndex,
                ProgressiveLevelId = progressiveLevelId,
                PaidAmount = paidAmount,
            });

            return null;
        }

        #endregion
    }
}
