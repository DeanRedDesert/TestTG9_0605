//-----------------------------------------------------------------------
// <copyright file = "UgpExternalJackpotsCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.ExternalJackpots
{
    using System;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the UgpExternalJackpots category.
    /// </summary>
    class UgpExternalJackpotsCategoryCallbacks : IUgpExternalJackpotsCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        /// <summary>
        /// Initialize an instance of <see cref="UgpExternalJackpotsCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for the handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public UgpExternalJackpotsCategoryCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #region IUgpExternalJackpotsCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessUpdateJackpots(ExternalJackpots externalJackpots)
        {
            eventCallbacksInterface.PostEvent(new ExternalJackpotChangedEventArgs(externalJackpots));
            return null;
        }

        #endregion
    }
}
