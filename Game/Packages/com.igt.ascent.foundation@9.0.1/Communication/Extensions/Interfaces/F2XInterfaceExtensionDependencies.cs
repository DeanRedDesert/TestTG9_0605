//-----------------------------------------------------------------------
// <copyright file = "F2XInterfaceExtensionDependencies.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using F2XTransport;

    /// <summary>
    /// Class for handling interface extension dependencies for F2L/F2X related interfaces.
    /// </summary>
    public class F2XInterfaceExtensionDependencies : InterfaceExtensionDependencies
    {
        /// <summary>
        /// Construct a dependency object with the specified category.
        /// </summary>
        /// <param name="category">Category on which the extension is dependent.</param>
        /// <param name="baseDependencies">The dependencies on which this interface builds.</param>
        public F2XInterfaceExtensionDependencies(IApiCategory category, IInterfaceExtensionDependencies baseDependencies)
            : base(baseDependencies)
        {
            Category = category;
        }

        /// <summary>
        /// API category for the extension.
        /// </summary>
        public IApiCategory Category { get; private set; }
    }
}