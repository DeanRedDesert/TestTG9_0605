// -----------------------------------------------------------------------
// <copyright file = "F2XFlashPlayerClock.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using F2X;
    using Interfaces;

    /// <summary>
    /// Implementation of the extended interface of client side Flash Player Clock that is backed by F2X.
    /// </summary>
    internal class F2XFlashPlayerClock : IFlashPlayerClock, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The cached FlashPlayerClock category instance used to communicate with the Foundation.
        /// </summary>
        private readonly IFlashPlayerClockCategory flashPlayerClockCategory;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes an instance of <see cref="F2XFlashPlayerClock"/>.
        /// </summary>
        /// <param name="flashPlayerClockCategory">
        /// Flash Player Clock category instance used to communicate with the Foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// The interface for processing a transactional event.
        /// </param>
        /// <param name="layeredContextActivationEvents">
        /// The interface for providing context events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XFlashPlayerClock(IFlashPlayerClockCategory flashPlayerClockCategory,
            IEventDispatcher transactionalEventDispatcher,
            ILayeredContextActivationEventsDependency layeredContextActivationEvents)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            if(layeredContextActivationEvents == null)
            {
                throw new ArgumentNullException(nameof(layeredContextActivationEvents));
            }

            this.flashPlayerClockCategory = flashPlayerClockCategory ?? throw new ArgumentNullException(nameof(flashPlayerClockCategory));

            transactionalEventDispatcher.EventDispatchedEvent += HandleFlashPlayerClockDataUpdatedEvent;
            layeredContextActivationEvents.ActivateLayeredContextEvent += HandleActivateThemeContextEvent;
        }

        #endregion

        #region Private methods

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
                // When receiving properties updates outside of activation its not guaranteed that all properties are defined. 
                FlashPlayerClockProperties = UpdateFlashPlayerClockProperties(FlashPlayerClockProperties, propertiesUpdatedEventArgs);

                var handler = FlashPlayerClockPropertiesChangedEvent;
                if(handler != null)
                {
                    handler(this, propertiesUpdatedEventArgs);
                    dispatchedEventArgs.IsHandled = true;
                }
            }
        }

        /// <summary>
        /// Handles the activate theme context event to cache the flash player clock data.
        /// </summary>
        /// <param name="sender">
        /// The sender of this event.
        /// </param>
        /// <param name="eventArgs">
        /// The activate (theme) context event arguments.
        /// </param>
        private void HandleActivateThemeContextEvent(object sender, LayeredContextActivationEventArgs eventArgs)
        {
            // Only handle F2L theme (which is on link level) context activation events.
            if(eventArgs.ContextLayer == ContextLayer.LegacyTheme)
            {
                var data = flashPlayerClockCategory.GetConfigData();
                if(data != null)
                {
                    FlashPlayerClockConfig = new FlashPlayerClockConfig(data.FlashPlayerClockEnabled,
                        data.NumberOfFlashesPerSequence, data.FlashSequenceLengthMilliseconds, data.MinutesBetweenSequences);

                }

                var flashPlayerClockProperties = flashPlayerClockCategory.GetFlashPlayerClockProperties();
                if(flashPlayerClockProperties != null)
                {
                    // It is safe to assume that PlayerClockSessionActive is defined here,
                    // since the schema is designed to supply a value for all properties on initial activation.
                    FlashPlayerClockProperties = new FlashPlayerClockProperties(flashPlayerClockProperties.PlayerClockSessionActive);
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
        public FlashPlayerClockConfig FlashPlayerClockConfig { get; private set; }

        #endregion
    }
}