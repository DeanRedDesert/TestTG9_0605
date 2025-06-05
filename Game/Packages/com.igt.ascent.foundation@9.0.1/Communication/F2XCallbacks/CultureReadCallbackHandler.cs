// -----------------------------------------------------------------------
// <copyright file = "CultureReadCallbackHandler.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using F2X;
    using F2XTransport;
    using F2XCultureContext = F2X.Schemas.Internal.CultureRead.CultureContext;

    /// <summary>
    /// This class implements callback methods supported by the F2X
    /// Culture Read API category.
    /// </summary>
    internal class CultureReadCallbackHandler : ICultureReadCategoryCallbacks
    {
        private readonly IEventCallbacks eventCallbacks;

        /// <summary>
        /// Initializes a new instance of <see cref="CultureReadCallbackHandler"/>.
        /// </summary>
        /// <param name="eventCallbacks">The callback interface for handling events.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="eventCallbacks"/> is null.
        /// </exception>
        public CultureReadCallbackHandler(IEventCallbacks eventCallbacks)
        {
            if(eventCallbacks == null)
            {
                throw new ArgumentNullException("eventCallbacks");
            }
            this.eventCallbacks = eventCallbacks;
        }

        #region Implementation of ICultureReadCategoryCallbacks

        /// <inheritdoc/>
        public string ProcessCultureChanged(F2XCultureContext affectedContext, string currentCulture)
        {
            var cultureChangedEventArgs =
                new CultureChangedEventArgs((CultureContext)affectedContext,
                                            currentCulture);
            eventCallbacks.PostEvent(cultureChangedEventArgs);
            return null;
        }

        #endregion
    }
}
