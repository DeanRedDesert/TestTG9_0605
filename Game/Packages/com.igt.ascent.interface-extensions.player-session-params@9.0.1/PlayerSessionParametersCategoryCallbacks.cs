//-----------------------------------------------------------------------
// <copyright file = "PlayerSessionParametersCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.PlayerSessionParams
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using F2X;
    using F2X.Schemas.Internal.PlayerSessionParameters;
    using F2XTransport;

    /// <summary>
    /// This class implements callback methods supported by the F2X
    /// Player Session Parameters Communication API category.
    /// </summary>
    internal class PlayerSessionParametersCategoryCallbacks : IPlayerSessionParametersCategoryCallbacks
    {
        /// <summary>
        /// The callback interface for handling events.
        /// </summary>
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes an instance of <see cref="PlayerSessionParametersCategoryCallbacks"/>.
        /// </summary>
        /// <param name="eventCallbacks">
        /// The callback interface for handling events.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public PlayerSessionParametersCategoryCallbacks(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region IPlayerSessionParametersCategoryCallbacks Members

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the paramaters is null.
        /// </exception>
        public string ProcessCurrentResetParametersChanged(IEnumerable<SessionParameterType> pendingParameters,
                                                           IEnumerable<SessionParameterType> resetParameters)
        {
            if(pendingParameters == null)
            {
                throw new ArgumentNullException(nameof(pendingParameters));
            }
            if(resetParameters == null)
            {
                throw new ArgumentNullException(nameof(resetParameters));
            }

            eventCallbacks.PostEvent(new CurrentResetParametersChangedEventArgs(
                pendingParameters.Select(item => (PlayerSessionParameterType)item),
                resetParameters.Select(item => (PlayerSessionParameterType)item)));

            return null;
        }

        #endregion
    }
}
