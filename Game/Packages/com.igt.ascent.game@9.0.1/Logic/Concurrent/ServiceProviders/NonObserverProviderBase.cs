// -----------------------------------------------------------------------
// <copyright file = "NonObserverProviderBase.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.ServiceProviders
{
    /// <summary>
    /// A base class for non-observer providers.
    /// This type of provider provides only synchronous game services,
    /// NEVER asynchronous games services.
    /// And they should NOT subscribe to any Lib events!
    /// </summary>
    public abstract class NonObserverProviderBase
    {
        #region Properties

        /// <summary>
        /// Gets the name of the provider.
        /// </summary>
        public string Name { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new provider instance.
        /// </summary>
        /// <param name="name">
        /// The name of the provider.
        /// </param>
        protected NonObserverProviderBase(string name)
        {
            Name = name ?? string.Empty;
        }

        #endregion
    }
}