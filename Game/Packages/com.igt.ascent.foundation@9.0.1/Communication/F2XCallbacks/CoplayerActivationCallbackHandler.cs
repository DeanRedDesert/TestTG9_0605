// -----------------------------------------------------------------------
// <copyright file = "CoplayerActivationCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.CoplayerLib.Interfaces;
    using F2X;
    using F2X.Schemas.Internal.CoplayerActivation;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods supported by the
    /// F2X Coplayer Activation category.
    /// </summary>
    internal sealed class CoplayerActivationCallbackHandler : ICoplayerActivationCategoryCallbacks
    {
        #region Private Fields

        private readonly IEventCallbacks eventCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates a new <see cref="CoplayerActivationCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public CoplayerActivationCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #endregion

        #region ICoplayerActivationCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessActivateCoplayerContext()
        {
            eventCallbacks.PostEvent(new ActivateCoplayerContextEventArgs());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessInactivateCoplayerContext()
        {
            eventCallbacks.PostEvent(new InactivateCoplayerContextEventArgs());
            return null;
        }

        /// <inheritdoc/>
        public string ProcessNewCoplayerContext(uint denomination, string payvarTag, string payvarTagDataFile, GameMode gameMode)
        {
            eventCallbacks.PostEvent(new NewCoplayerContextEventArgs((Ascent.Communication.Platform.Interfaces.GameMode)gameMode,
                                                                     denomination, payvarTag, payvarTagDataFile));
            return null;
        }

        #endregion
    }
}