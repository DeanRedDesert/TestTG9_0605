// -----------------------------------------------------------------------
// <copyright file = "GameCulture.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Standard
{
    using System;
    using Game.Core.Communication.Foundation.F2X;
    using Interfaces;
    using Restricted.EventManagement.Interfaces;
    using F2XCultureRead = Game.Core.Communication.Foundation.F2X.Schemas.Internal.CultureRead;

    /// <summary>
    /// Implementation of the <see cref="IGameCulture"/> that uses
    /// F2X to communicate with the Foundation to support bank play.
    /// </summary>
    internal sealed class GameCulture : GameCultureBase
    {
        #region Private Fields

        private readonly CategoryInitializer<ICultureReadCategory> cultureReadCategory;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public GameCulture(object eventSender, IEventDispatcher transactionalEventDispatcher)
            : base(eventSender, transactionalEventDispatcher)
        {
            cultureReadCategory = new CategoryInitializer<ICultureReadCategory>();
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Initializes the instance of <see cref="GameCulture"/> whose values become available after construction,
        /// e.g. when a connection is established with the Foundation.
        /// </summary>
        /// <param name="category">
        /// The category interface for communicating with the Foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="category"/> is null.
        /// </exception>
        public void Initialize(ICultureReadCategory category)
        {
            if(category == null)
            {
                throw new ArgumentNullException(nameof(category));
            }

            cultureReadCategory.Initialize(category);
        }

        #endregion

        #region GameCultureBase Overrides

        /// <inheritdoc/>
        public override void NewContext(IShellContext shellContext)
        {
            Culture = cultureReadCategory.Instance.GetCulture(F2XCultureRead.CultureContext.Theme);

            AvailableCultures = cultureReadCategory.Instance.GetAvailableCultures(F2XCultureRead.CultureContext.Theme).Culture;
        }

        #endregion
    }
}