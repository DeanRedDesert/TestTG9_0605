//-----------------------------------------------------------------------
// <copyright file = "UgpMessageStripInterfaceConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System;
    using System.Collections.Generic;
    using F2XTransport;
    using Interfaces;

    /// <summary>
    /// Configuration for the UgpMessageStrip extended interface.
    /// </summary>
    public sealed class UgpMessageStripInterfaceConfiguration : F2XInterfaceExtensionConfigurationBase
    {
        #region Private Fields

        /// <summary>
        /// The flag indicating if the interface for this configuration is required.
        /// </summary>
        private readonly bool required;

        #endregion

        #region Private Constants

        /// <summary>
        /// The requested major version of the underlying F2X category.
        /// </summary>
        private const int RequestedMajorVersion = 1;

        /// <summary>
        /// The requested minor version of the underlying F2X category.
        /// </summary>
        private const int RequestedMinorVersion = 1;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of the configuration.
        /// </summary>
        /// <param name="required">The flag indicating if this extended interface is mandatory.</param>
        public UgpMessageStripInterfaceConfiguration(bool required)
        {
            this.required = required;
        }

        #endregion

        #region IInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies)
        {
            switch(dependencies)
            {
                case null:
                    throw new ArgumentNullException(nameof(dependencies));

                case F2XInterfaceExtensionDependencies f2XDependencies:
                    try
                    {
                        return new F2XUgpMessageStrip(f2XDependencies.Category as IUgpMessageStripCategory,
                            f2XDependencies.TransactionalEventDispatcher);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing F2X UgpMessageStrip implementation from dependencies.", innerException);
                    }

                case StandaloneInterfaceExtensionDependencies _:
                    try
                    {
                        return new StandaloneUgpMessageStrip();
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing standalone UgpMessageStrip implementation from dependencies.", innerException);
                    }

                default:
                    throw new InterfaceExtensionDependencyException("Unrecognized dependencies.");
            }
        }

        /// <inheritdoc/>
        public override Type InterfaceType => typeof(IUgpMessageStrip);

        #endregion

        #region IF2XInterfaceExtensionConfiguration Implementation

        /// <inheritdoc/>
        public override IEnumerable<CategoryRequest> GetCategoryRequests()
        {
            return new List<CategoryRequest>
                       {
                           new CategoryRequest(
                               new CategoryVersionInformation((int)MessageCategory.UgpMessageStrip,
                                                              RequestedMajorVersion,
                                                              RequestedMinorVersion),
                               required,
                               FoundationTarget.AllAscent,  //TODO: Revised to target Foundation in need
                               dependencies => new UgpMessageStripCategory(
                                   dependencies.Transport, new UgpMessageStripCategoryCallbacks(dependencies.EventCallbacks)))
                       };
        }

        #endregion
    }
}
