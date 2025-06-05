// -----------------------------------------------------------------------
// <copyright file = "FlashPlayerClockInterfaceConfiguration.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.FlashPlayerClock
{
    using System;
    using System.Collections.Generic;
    using Communication;
    using F2X;
    using F2XTransport;
    using InterfaceExtensions;
    using Interfaces;

    /// <summary>
    /// Interface Extension Configuration for the FlashPlayerClock extended interface.
    /// </summary>
    public class FlashPlayerClockInterfaceConfiguration : F2XInterfaceExtensionConfigurationBase
    {
        /// <summary>
        /// The flag which indicates if or not this interface extension is required.
        /// </summary>
        private readonly bool required;

        /// <summary>
        /// Initializes an instance of <see cref="FlashPlayerClockInterfaceConfiguration"/>.
        /// </summary>
        /// <param name="required">
        /// The flag indicating if this extended interface is required.
        /// </param>
        public FlashPlayerClockInterfaceConfiguration(bool required)
        {
            this.required = required;
        }

        #region Overrides of F2XInterfaceExtensionConfigurationBase

        /// <inheritdoc />
        public override IInterfaceExtension CreateInterfaceExtension(IInterfaceExtensionDependencies dependencies)
        {
            switch(dependencies)
            {
                case null:
                    throw new ArgumentNullException(nameof(dependencies));
                case F2XInterfaceExtensionDependencies f2XDependencies:
                    try
                    {
                        return new F2XFlashPlayerClock(
                            f2XDependencies.Category as IFlashPlayerClockCategory,
                            f2XDependencies.TransactionalEventDispatcher,
                            f2XDependencies.LayeredContextActivationEvents);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing F2X FlashPlayerClock implementation from dependencies.", innerException);
                    }

                case StandaloneInterfaceExtensionDependencies standaloneDependencies:
                    try
                    {
                        return new StandaloneFlashPlayerClock(standaloneDependencies.TransactionalEventDispatcher);
                    }
                    catch(Exception innerException)
                    {
                        throw new InterfaceExtensionDependencyException(
                            "Issue constructing standalone FlashPlayerClock implementation from dependencies.", innerException);
                    }

                default:
                    throw new InterfaceExtensionDependencyException("Unrecognized dependencies.");
            }
        }

        /// <inheritdoc />
        public override Type InterfaceType => typeof(IFlashPlayerClock);

        /// <inheritdoc />
        public override IEnumerable<CategoryRequest> GetCategoryRequests()
        {
            return new List<CategoryRequest>
            {
                new CategoryRequest(
                    new CategoryVersionInformation((int)MessageCategory.FlashPlayerClock, 1, 0),
                    required,
                    FoundationTarget.AscentR1Series.AndHigher(),
                    dependencies => new FlashPlayerClockCategory(
                        dependencies.Transport, new FlashPlayerClockCategoryCallbacks(dependencies.EventCallbacks))
                )
            };
        }

        #endregion
    }
}