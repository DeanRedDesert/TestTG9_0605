// -----------------------------------------------------------------------
// <copyright file = "CategoryNegotiationDependencies.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using F2X;
    using F2XTransport;
    using System;
    using System.Collections.Generic;

    /// <inheritdoc/>
    public sealed class CategoryNegotiationDependencies : ICategoryNegotiationDependencies
    {
        private Dictionary<Type, object> additionalDependencies;

        #region ICategoryNegotiationDependencies Implementation

        /// <inheritdoc/>
        public IF2XTransport Transport { get; set; }

        /// <inheritdoc/>
        public IEventCallbacks EventCallbacks { get; set; }

        /// <inheritdoc/>
        public INonTransactionalEventCallbacks NonTransactionalEventCallbacks { get; set; }

        /// <inheritdoc/>
        public ILinkControlCategoryCallbacks LinkControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public ISystemApiControlCategoryCallbacks SystemApiControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public IAscribedGameApiControlCategoryCallbacks AscribedGameApiControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public IThemeApiControlCategoryCallbacks ThemeApiControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public ITsmApiControlCategoryCallbacks TsmApiControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public IShellApiControlCategoryCallbacks ShellApiControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public ICoplayerApiControlCategoryCallbacks CoplayerApiControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public IAppApiControlCategoryCallbacks AppApiControlCategoryCallbacks { get; set; }

        /// <inheritdoc/>
        public ITransactionCallbacks TransactionCallbacks { get; set; }

        /// <inheritdoc/>
        public T GetDependency<T>(bool required = true) where T : class
        {
            var dependency = default(T);
            if(additionalDependencies != null)
            {
                var type = typeof(T);
                object dependencyObject;
                if(additionalDependencies.TryGetValue(type, out dependencyObject))
                {
                    dependency = (T)dependencyObject;
                }
            }
            if(required && dependency == null)
            {
                var type = typeof(T);
                var message = string.Format("No dependency of type {0} is available.", type);
                throw new InvalidOperationException(message);
            }
            return dependency;
        }

        #endregion

        /// <summary>
        /// Adds the given dependency to the set of available dependencies.
        /// </summary>
        /// <typeparam name="T">The type of the dependency.</typeparam>
        /// <param name="dependency">The dependency to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="dependency"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if a dependency of type <typeparamref name="T"/> has already been added.
        /// </exception>
        public void AddDependency<T>(T dependency) where T : class
        {
            if(dependency == null)
            {
                throw new ArgumentNullException();
            }
            if(additionalDependencies == null)
            {
                additionalDependencies = new Dictionary<Type, object>();
            }
            var type = typeof(T);
            if(additionalDependencies.ContainsKey(type))
            {
                var message = string.Format("A dependency of type {0} is already present.", type);
                throw new InvalidOperationException(message);
            }
            additionalDependencies[type] = dependency;
        }
    }
}