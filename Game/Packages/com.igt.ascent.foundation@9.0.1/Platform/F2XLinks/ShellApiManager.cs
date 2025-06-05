// -----------------------------------------------------------------------
// <copyright file = "ShellApiManager.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Game.Core.Communication.Foundation.F2X;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.ShellApiControl;
    using Game.Core.Communication.Foundation.F2X.Schemas.Internal.Types;
    using Game.Core.Communication.Foundation.F2XCallbacks;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;

    /// <summary>
    /// This class manages the negotiation and installation of categories on <see cref="CategoryNegotiationLevel.Shell"/>.
    /// </summary>
    internal sealed class ShellApiManager : ApiManager, IShellApiControlCategoryCallbacks
    {
        #region Private Fields

        /// <summary>
        /// Interface for handling API negotiation results.
        /// </summary>
        private readonly IShellApiCallbacks shellApiCallbacks;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ShellApiManager"/>.
        /// </summary>
        /// <param name="subscriptions">
        /// The categories to subscribe.
        /// </param>
        /// <param name="shellApiCallbacks">
        /// The interface for handling API negotiation results.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="subscriptions"/> is null.
        /// </exception>
        public ShellApiManager(IList<CategorySubscription> subscriptions, IShellApiCallbacks shellApiCallbacks = null)
            : base(CategoryNegotiationLevel.Shell, subscriptions)
        {
            this.shellApiCallbacks = shellApiCallbacks;
        }

        #endregion

        #region IShellApiControlCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessGetShellApiVersions(ShellIdentifier shell,
                                                 GetShellApiVersionsSendTagDataFile tagDataFile,
                                                 GetShellApiVersionsSendLinkedExtensions linkedExtensions,
                                                 out GetShellApiVersionsReplyContentCategoryVersions callbackResult)
        {
            // Currently shell identifier is not used.
            shellApiCallbacks?.ProcessShellApiStart(tagDataFile.Tag,
                                                    tagDataFile.Value,
                                                    linkedExtensions == null
                                                        ? new List<LinkedExtension>()
                                                        : linkedExtensions.LinkedExtension);

            callbackResult = new GetShellApiVersionsReplyContentCategoryVersions
                                 {
                                     CategoryVersion = GetRequestedCategories()
                                                       .Select(categoryRequest => categoryRequest.CategoryVersionInformation.ToInternal())
                                                       .ToList()
                                 };

            return null;
        }

        /// <inheritdoc/>
        public string ProcessSetShellApiVersions(SetShellApiVersionsSendCategoryVersions categoryVersions,
                                                 out bool callbackResult)
        {
            var selectedCategories = categoryVersions.CategoryVersion
                                                     .Select(version => version.ToPublic())
                                                     .ToList();

            callbackResult = TryInstallApiCategories(selectedCategories, out var installedHandlers);

            if(callbackResult)
            {
                shellApiCallbacks?.ProcessShellApiNegotiation(installedHandlers);
            }

            // Callback result will report the success/failure.
            // There is no need to populate the error code and message.
            return null;
        }

        #endregion
    }
}