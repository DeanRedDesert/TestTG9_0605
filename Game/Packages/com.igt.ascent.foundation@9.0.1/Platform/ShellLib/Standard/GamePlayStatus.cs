// -----------------------------------------------------------------------
// <copyright file = "GamePlayStatus.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Implementation of the <see cref="IGamePlayStatus"/> that uses
    /// F2X to communicate with the Foundation to support game play status.
    /// </summary>
    internal sealed class GamePlayStatus : GamePlayStatusBase
    {
        #region Private Fields

        /// <summary>
        /// The interface for the game play status category.
        /// </summary>
        private readonly CategoryInitializer<IGamePlayStatusCategory> gamePlayStatusCategory;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public GamePlayStatus(object eventSender,
                              IEventDispatcher transactionalEventDispatcher)
            : base(eventSender, transactionalEventDispatcher)
        {
            gamePlayStatusCategory = new CategoryInitializer<IGamePlayStatusCategory>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="GamePlayStatus"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(IGamePlayStatusCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }
            gamePlayStatusCategory.Initialize(category);
        }

        #endregion

        #region GamePlayStatusBase Overrides

        /// <inheritdoc/>
        public override void NewContext(IShellContext shellContext)
        {
            GameInProgress = gamePlayStatusCategory.Instance.GetGameInProgress();

            var f2XGameFocus = gamePlayStatusCategory.Instance.GetGameFocus();
            GameFocus = f2XGameFocus == null
                            ? null
                            : new GameFocus(f2XGameFocus.Coplayer,
                                            f2XGameFocus.ThemeSelector.ThemeIdentifier.ToToken(),
                                            f2XGameFocus.ThemeSelector.Denom);
        }

        #endregion
    }
}