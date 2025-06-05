//-----------------------------------------------------------------------
// <copyright file = "InterfaceExtensionDependencies.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using System;
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;

    /// <summary>
    /// Base implementation of the interface extension dependencies.
    /// </summary>
    public class InterfaceExtensionDependencies : IInterfaceExtensionDependencies
    {
        /// <summary>
        /// Initializes an instance of <see cref="InterfaceExtensionDependencies"/> with
        /// default values.
        /// </summary>
        public InterfaceExtensionDependencies()
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="InterfaceExtensionDependencies"/> with
        /// an existing dependency object interface.
        /// </summary>
        /// <param name="dependencies">The dependency object interface to copy from.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dependencies"/> is null.
        /// </exception>
        protected InterfaceExtensionDependencies(IInterfaceExtensionDependencies dependencies)
        {
            if(dependencies == null)
            {
                throw new ArgumentNullException("dependencies");
            }

            TransactionWeightVerification = dependencies.TransactionWeightVerification;
            CriticalDataProvider = dependencies.CriticalDataProvider;
            LayeredContextActivationEvents = dependencies.LayeredContextActivationEvents;
            CultureInfoProvider = dependencies.CultureInfoProvider;
            GameModeQuery = dependencies.GameModeQuery;
            TransactionalEventDispatcher = dependencies.TransactionalEventDispatcher;
            NonTransactionalEventDispatcher = dependencies.NonTransactionalEventDispatcher;
        }

        #region IInterfaceExtensionDependencies Members

        /// <inheritdoc/>
        public ITransactionWeightVerificationDependency TransactionWeightVerification { get; set; }

        /// <inheritdoc/>
        public ICriticalDataDependency CriticalDataProvider { get; set; }

        /// <inheritdoc/>
        public ILayeredContextActivationEventsDependency LayeredContextActivationEvents { get; set; }

        /// <inheritdoc/>
        public ICultureInfoDependency CultureInfoProvider { get; set; }

        /// <inheritdoc/>
        public IGameModeQuery GameModeQuery { get; set; }

        /// <inheritdoc/>
        public IEventDispatcher TransactionalEventDispatcher { get; set; }

        /// <inheritdoc/>
        public IEventDispatcher NonTransactionalEventDispatcher { get; set; }

        #endregion
    }
}