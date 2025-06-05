// -----------------------------------------------------------------------
// <copyright file = "AppActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ExtensionBinLib.Interfaces;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using F2X;
    using F2XTransport;
    using IAppExtensionContext = Ascent.Communication.Platform.ExtensionBinLib.Interfaces.IAppExtensionContext;

    /// <summary>
    /// Handles AppActivation category callbacks by posting events to <see cref="IEventCallbacks"/>.
    /// </summary>
    internal class AppActivationCallbackHandler : IAppActivationCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Instantiates a new <see cref="AppActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public AppActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region IAppActivationCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessNewAppContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new NewAppExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new NewInnerContextEventArgs<IAppExtensionContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessActivateAppContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new ActivateAppExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new ActivateInnerContextEventArgs<IAppExtensionContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateAppContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new InactivateAppExtensionContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new InactivateInnerContextEventArgs<IAppExtensionContext>());
            return null;
        }

        #endregion
    }
}
