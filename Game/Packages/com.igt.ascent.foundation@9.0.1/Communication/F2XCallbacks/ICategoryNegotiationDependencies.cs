// -----------------------------------------------------------------------
// <copyright file = "ICategoryNegotiationDependencies.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using F2X;
    using F2XTransport;

    /// <summary>
    /// This interface defines the dependencies required during construction of an F2X category
    /// that is responsible for negotiating categories on a negotiation level.
    /// </summary>
    /// <inheritdoc/>
    public interface ICategoryNegotiationDependencies : ICategoryCreationDependencies
    {
        /// <summary>
        /// Gets the object that implements <see cref="ILinkControlCategoryCallbacks"/>
        /// </summary>
        ILinkControlCategoryCallbacks LinkControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="ISystemApiControlCategoryCallbacks"/>
        /// </summary>
        ISystemApiControlCategoryCallbacks SystemApiControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="IAscribedGameApiControlCategoryCallbacks"/>
        /// </summary>
        IAscribedGameApiControlCategoryCallbacks AscribedGameApiControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="IThemeApiControlCategoryCallbacks"/>
        /// </summary>
        IThemeApiControlCategoryCallbacks ThemeApiControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="ITsmApiControlCategoryCallbacks"/>
        /// </summary>
        ITsmApiControlCategoryCallbacks TsmApiControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="IShellApiControlCategoryCallbacks"/>
        /// </summary>
        IShellApiControlCategoryCallbacks ShellApiControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="ICoplayerApiControlCategoryCallbacks"/>
        /// </summary>
        ICoplayerApiControlCategoryCallbacks CoplayerApiControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="IAppApiControlCategoryCallbacks"/>
        /// </summary>
        IAppApiControlCategoryCallbacks AppApiControlCategoryCallbacks { get; }

        /// <summary>
        /// Gets the object that implements <see cref="ITransactionCallbacks"/>.
        /// </summary>
        /// <remarks>
        /// This is needed by processing ActionResponse message.  Strictly speaking,
        /// it is not a dependency for category negotiation, but it is put in this
        /// interface to prevent assess to it by interface extensions.
        /// </remarks>
        ITransactionCallbacks TransactionCallbacks { get; }
    }
}