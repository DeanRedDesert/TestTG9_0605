// -----------------------------------------------------------------------
// <copyright file = "RuntimeGameEventsProvider.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents
{
    using System;
    using Ascent.Communication.Platform.GameLib.Interfaces;

    /// <summary>
    /// Provider for the runtime game events interface extension.
    /// </summary>
    public class RuntimeGameEventsProvider
    {
        #region Private Fields

        /// <summary>
        /// The cached reference of <see cref="IRuntimeGameEvents"/> instance.
        /// </summary>
        private readonly IRuntimeGameEvents runtimeGameEvents;

        #endregion

        #region Game Services

        /// <summary>
        /// Get the runtime game events configuration. Will be null if <see cref="IRuntimeGameEvents"/> is not obtained.
        /// </summary>
        public RuntimeGameEventsConfiguration RuntimeGameEventsConfiguration =>
            runtimeGameEvents?.RuntimeGameEventsConfiguration;

        #endregion

        #region Constructor

        /// <summary>
        /// Create an instance of <see cref="RuntimeGameEventsProvider"/>.
        /// </summary>
        /// <param name="gameLib">
        /// The <see cref="IGameLib"/> instance.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="gameLib"/> is null.
        /// </exception>
        public RuntimeGameEventsProvider(IGameLib gameLib)
        {
            if(gameLib == null)
            {
                throw new ArgumentNullException(nameof(gameLib));
            }

            runtimeGameEvents = gameLib.GetInterface<IRuntimeGameEvents>();
        }

        #endregion
    }
}
