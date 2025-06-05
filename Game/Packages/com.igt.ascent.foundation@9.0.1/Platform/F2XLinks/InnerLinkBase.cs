// -----------------------------------------------------------------------
// <copyright file = "InnerLinkBase.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.F2XLinks
{
    using System;
    using System.Collections.Generic;
    using Game.Core.Communication.Foundation.F2XTransport;
    using Game.Core.Communication.Foundation.Standard.F2XLinks;

    /// <summary>
    /// Base class for all Inner Link implementations.
    /// </summary>
    internal abstract class InnerLinkBase
    {
        #region Private Fields

        private volatile bool isConnected;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the API Manager working on the link.
        /// </summary>
        public IApiManager ApiManager { get; protected set; }

        /// <summary>
        /// Gets or sets the flag indicating whether the link is currently connected to Foundation.
        /// </summary>
        public bool IsConnected
        {
            get => isConnected;
            protected set => isConnected = value;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// In a list of message category handlers, finds and returns the one that
        /// implements the given category interface and for the given category number.
        /// </summary>
        /// <typeparam name="TCategory">
        /// The type of the category to find.
        /// </typeparam>
        /// <param name="categoryHandlers">
        /// The list of message category handlers to search.
        /// </param>
        /// <param name="messageCategory">
        /// The number of the message category to find.
        /// </param>
        /// <returns>
        /// The implementation of <typeparamref name="TCategory"/>.
        /// Null if not found.
        /// </returns>
        /// <exception cref="ApplicationException">
        /// Thrown when the category found for the given <paramref name="messageCategory"/> is not
        /// of the given return type of <typeparamref name="TCategory"/>.
        /// </exception>
        protected static TCategory Retrieve<TCategory>(IDictionary<MessageCategory, IApiCategory> categoryHandlers,
                                                       MessageCategory messageCategory) where TCategory : class
        {
            TCategory result;

            if(categoryHandlers.TryGetValue(messageCategory, out var installedHandler))
            {
                result = installedHandler as TCategory ??
                         throw new ApplicationException($"The installed handler for {messageCategory} category is not of the expected type {typeof(TCategory)}.");
            }
            else
            {
                result = default;
            }

            return result;
        }

        #endregion
    }
}