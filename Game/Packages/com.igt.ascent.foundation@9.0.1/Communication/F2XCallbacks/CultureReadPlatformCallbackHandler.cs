// -----------------------------------------------------------------------
// <copyright file = "CultureReadPlatformCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using F2X;
    using F2XTransport;
    using IGT.Ascent.Communication.Platform.Interfaces;
    using F2XCultureRead = F2X.Schemas.Internal.CultureRead;

    /// <summary>
    /// This class implements callback methods supported by the F2X
    /// Culture Read API category.
    /// This implementation posts events of Platform namespace rather than Foundation namespace.
    /// </summary>
    internal class CultureReadPlatformCallbackHandler : ICultureReadCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes a new instance of <see cref="CultureReadPlatformCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public CultureReadPlatformCallbackHandler(IEventCallbacks eventCallbacks)
        {
            this.eventCallbacks = eventCallbacks ?? throw new ArgumentNullException(nameof(eventCallbacks));
        }

        #region ICultureReadCategoryCallbacks Implementation

        /// <inheritdoc/>
        public string ProcessCultureChanged(F2XCultureRead.CultureContext affectedContext, string currentCulture)
        {
            var cultureChangedEventArgs = new CultureChangedEventArgs((CultureContext)affectedContext,
                                                                      currentCulture);
            eventCallbacks.PostEvent(cultureChangedEventArgs);
            return null;
        }

        #endregion
    }
}
