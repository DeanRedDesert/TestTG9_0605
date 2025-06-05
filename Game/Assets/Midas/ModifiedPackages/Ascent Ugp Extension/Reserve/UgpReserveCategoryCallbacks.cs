//-----------------------------------------------------------------------
// <copyright file = "UgpReserveCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    using System;
    using F2XTransport;

    /// <summary>
    /// This class is responsible for handling callbacks from the UgpReserve category.
    /// </summary>
    class UgpReserveCategoryCallbacks : IUgpReserveCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling transactional events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacksInterface;

        /// <summary>
        /// Initialize an instance of <see cref="UgpReserveCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacksInterface">
        /// The callback interface for the handling transactional events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacksInterface"/> is null.
        /// </exception>
        public UgpReserveCategoryCallbacks(IEventCallbacks eventCallbacksInterface)
        {
            this.eventCallbacksInterface = eventCallbacksInterface ?? throw new ArgumentNullException(nameof(eventCallbacksInterface));
        }

        #region IUgpReserveCategoryCallbacks Implementation

        /// <inheritdoc />
        public string ProcessSetReserveParameters(ReserveParameters parameters)
        {
            eventCallbacksInterface.PostEvent(new ReserveParametersChangedEventArgs(parameters));
            return null;
        }

        #endregion
    }
}
