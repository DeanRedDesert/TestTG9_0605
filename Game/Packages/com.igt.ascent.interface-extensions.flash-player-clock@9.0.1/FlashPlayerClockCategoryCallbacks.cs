// -----------------------------------------------------------------------
// <copyright file = "FlashPlayerClockCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock
{
    using System;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods supported by the F2X Flash Player Clock category.
    /// </summary>
    public class FlashPlayerClockCategoryCallbacks : IFlashPlayerClockCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="FlashPlayerClockCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacks">
        /// The callback interface for handling events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public FlashPlayerClockCategoryCallbacks(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        /// <inheritdoc/>
        public string ProcessUpdateFlashPlayerClockProperties(F2X.Schemas.Internal.FlashPlayerClock.FlashPlayerClockProperties flashPlayerClockProperties)
        {
            eventCallbacks.PostEvent(new FlashPlayerClockPropertiesChangedEventArgs(
                flashPlayerClockProperties.PlayerClockSessionActiveSpecified 
                    ? (bool?)flashPlayerClockProperties.PlayerClockSessionActive 
                    : null));

            return null;
        }
    }
}