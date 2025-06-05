// -----------------------------------------------------------------------
// <copyright file = "AscribedGameExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExtensionBinLib.Interfaces;
    using ExtensionLib.Interfaces;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.AscribedGameApiControl;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;

    /// <summary>
    /// Implementation of <see cref="IAscribedGameExtensionLink"/>.
    /// </summary>
    internal sealed class AscribedGameExtensionLink : InnerLinkBase, IAscribedGameExtensionLink, IAscribedGameApiCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The list of Ascribed Game message categories this link will subscribe to.
        /// </summary>
        private readonly List<CategorySubscription> ascribedGameCategorySubscriptions =
            new List<CategorySubscription>
                {
                    // Either ThemeActivation or AscribedShellActivation could be negotiated on AscribedGameApiControl
                    // that is depending on the extension being linked to a theme or shell.
                    new CategorySubscription(MessageCategory.ThemeActivation, false),
                    new CategorySubscription(MessageCategory.AscribedShellActivation, false),
                };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="AscribedGameExtensionLink"/>.
        /// </summary>
        internal AscribedGameExtensionLink()
        {
            ApiManager = new AscribedGameApiManager(ascribedGameCategorySubscriptions, this);
        }

        #endregion

        #region IAscribedGameExtensionLink Implementation

        /// <inheritdoc />
        public AscribedGameEntity AscribedGameEntity { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<IExtensionIdentity> AscribedGameImportedExtensions { get; private set; }

        #endregion

        #region IAscribedGameApiCallbacks Implementation

        /// <inheritdoc />
        public void ProcessAscribedGameApiStart(AscribedGame ascribedGame, IEnumerable<Extension> extensions)
        {
            if(ascribedGame == null)
            {
                throw new ArgumentNullException(nameof(ascribedGame));
            }

            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            IsConnected = false;

            switch(ascribedGame.Item)
            {
                case ThemeIdentifier themeIdentifier:
                {
                    AscribedGameEntity = new AscribedGameEntity(AscribedGameType.Theme, themeIdentifier.Value);
                    break;
                }
                case ShellIdentifier shellIdentifier:
                {
                    AscribedGameEntity = new AscribedGameEntity(AscribedGameType.Shell, shellIdentifier.Value);
                    break;
                }
                default:
                {
                    throw new ArgumentException("Not supported identifier type in the F2X AscribedGame element!");
                }
            }

            AscribedGameImportedExtensions = extensions.Select(extension => extension.ToPublic()).ToList();
        }

        /// <inheritdoc />
        public void ProcessAscribedGameApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            IsConnected = true;
        }

        #endregion
    }
}