// -----------------------------------------------------------------------
// <copyright file = "F2XInterfaceExtensionConfigurationBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions
{
    using System;
    using System.Collections.Generic;
    using F2XTransport;
    using Interfaces;

    /// <summary>
    /// Abstract base class for an F2X Interface Extension Configuration.
    /// </summary>
    public abstract class F2XInterfaceExtensionConfigurationBase : IF2XInterfaceExtensionConfiguration
    {
        #region Abstract Members

        /// <inheritdoc/>
        public abstract IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies);

        /// <inheritdoc/>
        public abstract Type InterfaceType { get; }

        /// <inheritdoc/>
        public abstract IEnumerable<CategoryRequest> GetCategoryRequests();

        #endregion

        #region Virtual Members

        /// <inheritdoc/>
        public virtual CategoryNegotiationLevel NegotiationLevel
        {
            get { return CategoryNegotiationLevel.Link; }
        }

        #endregion

    }
}