// -----------------------------------------------------------------------
// <copyright file = "TsmActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ExtensionBinLib.Interfaces;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using F2X;
    using F2XTransport;

    /// <summary>
    /// Handles TsmActivation category callbacks by posting events to <see cref="IEventCallbacks"/>.
    /// </summary>
    internal class TsmActivationCallbackHandler : ITsmActivationCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Instantiates a new <see cref="TsmActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public TsmActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region Implementation of ITsmActivationCategoryCallbacks

        /// <inheritdoc/>
        public string ProcessActivateTsmContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new ActivateTsmExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new ActivateInnerContextEventArgs<IAscribedChooserContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateTsmContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new InactivateTsmExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new InactivateInnerContextEventArgs<IAscribedChooserContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessNewTsmContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new NewTsmExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new NewInnerContextEventArgs<IAscribedChooserContext>());
            return null;
        }

        #endregion
    }
}
