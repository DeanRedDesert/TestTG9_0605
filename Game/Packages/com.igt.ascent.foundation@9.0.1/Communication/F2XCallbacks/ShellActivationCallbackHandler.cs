// -----------------------------------------------------------------------
// <copyright file = "ShellActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.ShellLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.ShellActivation;
    using F2XTransport;
    using PlatformInterfaces = Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// This class implements callback methods supported by the
    /// F2X Shell Activation category.
    /// </summary>
    internal sealed class ShellActivationCallbackHandler : IShellActivationCategoryCallbacks
    {
        #region Private Fields

        private readonly IEventCallbacks eventCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="ShellActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public ShellActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #endregion

        #region IShellActivationCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessActivateShellContext()
        {
            eventCallbacks.PostEvent(new ActivateShellContextEventArgs());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateShellContext()
        {
            eventCallbacks.PostEvent(new InactivateShellContextEventArgs());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessNewShellContext(GameMode gameMode, SecurityLevelType securityLevel)
        {
            eventCallbacks.PostEvent(new NewShellContextEventArgs((PlatformInterfaces.GameMode)gameMode));
            return null;
        }

        #endregion
    }
}