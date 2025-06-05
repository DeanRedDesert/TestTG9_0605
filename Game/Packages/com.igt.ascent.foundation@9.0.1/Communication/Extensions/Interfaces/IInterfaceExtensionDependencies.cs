//-----------------------------------------------------------------------
// <copyright file = "IInterfaceExtensionDependencies.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    using Ascent.Communication.Platform.Interfaces;
    using Ascent.Restricted.EventManagement.Interfaces;

    /// <summary>
    /// This interface defines the dependencies required during construction of interface extensions.
    /// </summary>
    public interface IInterfaceExtensionDependencies
    {
        /// <summary>
        /// Gets or sets the interface for validating transaction weights.
        /// </summary>
        ITransactionWeightVerificationDependency TransactionWeightVerification { get; set; }

        /// <summary>
        /// Gets or sets the interface to use for persisting data.
        /// </summary>
        ICriticalDataDependency CriticalDataProvider { get; set; }

        /// <summary>
        /// Gets or sets the interface to use for receiving context events.
        /// </summary>
        ILayeredContextActivationEventsDependency LayeredContextActivationEvents { get; set; }

        /// <summary>
        /// Gets or sets the interface to use for persisting data.
        /// </summary>
        ICultureInfoDependency CultureInfoProvider { get; set; }

        /// <summary>
        /// Gets or sets the interface for querying game mode.
        /// </summary>
        IGameModeQuery GameModeQuery { get; set; }

        /// <summary>
        /// Gets or sets the interface for processing a transactional event.
        /// </summary>
        IEventDispatcher TransactionalEventDispatcher { get; set; }

        /// <summary>
        /// Gets or sets the interface for processing a non-transactional event.
        /// </summary>
        IEventDispatcher NonTransactionalEventDispatcher { get; set; }
    }
}