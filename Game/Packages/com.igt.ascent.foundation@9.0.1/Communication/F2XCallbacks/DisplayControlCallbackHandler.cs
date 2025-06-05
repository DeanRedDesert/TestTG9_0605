// -----------------------------------------------------------------------
// <copyright file = "DisplayControlCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2XTransport;
    using F2XDisplayControl = F2X.Schemas.Internal.DisplayControl;

    /// <summary>
    /// <summary>
    /// This class implements callback methods supported by the
    /// F2X Display Control category.
    /// </summary>
    /// </summary>
    /// <inheritdoc/>
    internal sealed class DisplayControlCallbackHandler : IDisplayControlCategoryCallbacks
    {
        #region Private Fields

        private readonly IEventCallbacks eventCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="DisplayControlCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public DisplayControlCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #endregion

        #region Interface Implementation

        /// <inheritdoc/>
        public string ProcessSetDisplayControlState(F2XDisplayControl.DisplayControlState displayControlState)
        {
            eventCallbacks.PostEvent(new DisplayControlEventArgs((DisplayControlState)displayControlState));
            return null;
        }

        #endregion
    }
}