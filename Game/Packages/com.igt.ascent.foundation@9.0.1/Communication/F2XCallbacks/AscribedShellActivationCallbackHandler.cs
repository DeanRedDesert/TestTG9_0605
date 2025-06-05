// -----------------------------------------------------------------------
// <copyright file = "AscribedShellActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ExtensionBinLib;
    using Ascent.Communication.Platform.ExtensionBinLib.Interfaces;
    using Ascent.Communication.Platform.ExtensionLib.Interfaces;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2XTransport;
    using F2XAscribedShellActivation = F2X.Schemas.Internal.AscribedShellActivation;

    /// <summary>
    /// Handles AscribedShellActivation category callbacks by posting events to <see cref="IEventCallbacks"/>.
    /// </summary>
    internal class AscribedShellActivationCallbackHandler : IAscribedShellActivationCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Instantiates a new <see cref="AscribedShellActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public AscribedShellActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region IAscribedShellActivationCategoryCallbacks Members

        /// <inheritdoc/>
        public string ProcessActivateAscribedShellContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new ActivateAscribedShellContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new ActivateInnerContextEventArgs<IAscribedGameContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateAscribedShellContext()
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new InactivateAscribedShellContextEventArgs());

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new InactivateInnerContextEventArgs<IAscribedGameContext>());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessNewAscribedShellContext(F2XAscribedShellActivation.GameMode gameMode)
        {
            // This event is used by ExtensionLib.  Can be removed when ExtensionLib is phased out.
            eventCallbacks.PostEvent(new NewAscribedShellContextEventArgs((GameMode)gameMode));

            // This event is used by ExtensionBinLib.
            eventCallbacks.PostEvent(new NewInnerContextEventArgs<IAscribedGameContext>(new AscribedGameContext((GameMode)gameMode)));
            return null;
        }

        #endregion
    }
}
