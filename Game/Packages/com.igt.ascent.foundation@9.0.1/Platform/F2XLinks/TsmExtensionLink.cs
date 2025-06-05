// -----------------------------------------------------------------------
// <copyright file = "TsmExtensionLink.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ExtensionBinLib.Interfaces;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;

    /// <summary>
    /// Implementation of <see cref="ITsmExtensionLink"/>.
    /// </summary>
    internal sealed class TsmExtensionLink : InnerLinkBase, ITsmExtensionLink, ITsmApiCallbacks
    {
        #region Private Fields

        /// <summary>
        /// The list of Tsm message categories this link will subscribe.
        /// </summary>
        private readonly List<CategorySubscription> tsmCategorySubscriptions =
            new List<CategorySubscription>
                {
                    new CategorySubscription(MessageCategory.TsmActivation, true),
                };

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="TsmExtensionLink"/>.
        /// </summary>
        internal TsmExtensionLink()
        {
            ApiManager = new TsmApiManager(tsmCategorySubscriptions, this);
        }

        #endregion

        #region ITsmExtensionLink Implementation

        /// <inheritdoc />
        public string ChooserIdentifier { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<IExtensionIdentity> ChooserImportedExtensions { get; private set; }

        #endregion

        #region ITsmApiCallbacks Implementation

        /// <inheritdoc />
        public void ProcessTsmApiStart(string tsmIdentifier, IEnumerable<Extension> extensions)
        {
            if(extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            IsConnected = false;

            ChooserIdentifier = tsmIdentifier ?? throw new ArgumentNullException(nameof(tsmIdentifier));
            ChooserImportedExtensions = extensions.Select(extension => extension.ToPublic()).ToList();
        }

        /// <inheritdoc />
        public void ProcessTsmApiNegotiation(IDictionary<MessageCategory, IApiCategory> installedHandlers)
        {
            IsConnected = true;
        }

        #endregion
    }
}