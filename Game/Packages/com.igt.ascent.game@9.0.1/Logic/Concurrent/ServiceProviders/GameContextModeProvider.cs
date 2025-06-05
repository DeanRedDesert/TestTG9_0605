// -----------------------------------------------------------------------
// <copyright file = "GameContextModeProvider.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    using Communication.Platform.Interfaces;
    using Game.Core.Logic.Services;

    /// <summary>
    /// A service provider that provides the game mode value.
    /// </summary>
    /// <devdoc>
    /// The provider and service names are picked to be the same as the non-concurrent provider version.
    /// </devdoc>
    public class GameContextModeProvider : NonObserverProviderBase
    {
        #region Constants

        private const string DefaultName = nameof(GameContextModeProvider);

        #endregion

        #region Game Services

        /// <summary>
        /// Gets the game mode.
        /// </summary>
        [GameService]
        public GameMode GameContextMode { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellConfigurationProvider"/>.
        /// </summary>
        /// <param name="gameMode">
        /// Current game mode.
        /// </param>
        /// <param name="name">
        /// The name of the service provider.
        /// This parameter is optional.  If not specified, the provider name will be set to <see cref="DefaultName"/>.
        /// </param>
        public GameContextModeProvider(GameMode gameMode, string name = DefaultName) : base(name)
        {
            GameContextMode = gameMode;
        }

        #endregion
    }
}