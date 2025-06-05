// -----------------------------------------------------------------------
// <copyright file = "StandaloneFlashPlayerClock.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the extended interface of client side flash player clock
    /// that is to be used in standalone mode.
    /// </summary>
    internal class StandaloneFlashPlayerClock : IFlashPlayerClock, IInterfaceExtension
    {
        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneFlashPlayerClock"/>.
        /// </summary>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public StandaloneFlashPlayerClock(IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            transactionalEventDispatcher.EventDispatchedEvent += HandleFlashPlayerClockDataUpdatedEvent;

            // Initially set the flash player clock session to true for standalone.
            // This can be modified when implementing a more robust Standalone implementation.
            FlashPlayerClockProperties = new FlashPlayerClockProperties(true);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Returns a new set of property values based on the current values and the update values.
        /// </summary>
        /// <param name="current">The current property values.</param>
        /// <param name="update">The property values to update.</param>
        /// <returns>The property values after update.</returns>
        private static FlashPlayerClockProperties UpdateFlashPlayerClockProperties(FlashPlayerClockProperties current, FlashPlayerClockPropertiesChangedEventArgs update)
        {
            return new FlashPlayerClockProperties(update.PlayerClockSessionActive ?? current.PlayerClockSessionActive);
        }

        /// <summary>
        /// Handles the dispatched event if the dispatched event is PlayerClockSessionActiveChangedEventArgs.
        /// </summary>
        /// <param name="sender">
        /// The sender of the dispatched event.
        /// </param>
        /// <param name="dispatchedEventArgs">
        /// The arguments used for processing the dispatched event.
        /// </param>
        private void HandleFlashPlayerClockDataUpdatedEvent(object sender, EventDispatchedEventArgs dispatchedEventArgs)
        {
            if(dispatchedEventArgs.DispatchedEvent is FlashPlayerClockPropertiesChangedEventArgs propertiesUpdatedEventArgs)
            {
                FlashPlayerClockProperties =
                    UpdateFlashPlayerClockProperties(FlashPlayerClockProperties, propertiesUpdatedEventArgs);

                var handler = FlashPlayerClockPropertiesChangedEvent;
                if(handler != null)
                {
                    handler(this, propertiesUpdatedEventArgs);
                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        #endregion

        #region IFlashPlayerClock

        /// <inheritdoc />
        public event EventHandler<FlashPlayerClockPropertiesChangedEventArgs> FlashPlayerClockPropertiesChangedEvent;

        /// <inheritdoc />
        public FlashPlayerClockProperties FlashPlayerClockProperties { get; private set; }

        /// <inheritdoc />
        /// <remarks>
        /// This config is temporarily hardcoded until a more robust Standalone implementation is completed.
        /// </remarks>
        public FlashPlayerClockConfig FlashPlayerClockConfig => new FlashPlayerClockConfig(true, 2, 500, 1);

        #endregion
    }
}