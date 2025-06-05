//-----------------------------------------------------------------------
// <copyright file = "UgpCategoryBase.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp
{
    using F2X.Schemas.Internal.Types;
    using F2XTransport;

    /// <summary>
    /// Base class for all UGP message categories.
    /// </summary>
    /// <typeparam name="T">The message type that this category sends.</typeparam>
    [DisableCodeCoverageInspection]
    public abstract class UgpCategoryBase<T> : ICategory where T : ICategoryMessage, new()
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        protected UgpCategoryBase()
        {
            Message = new T();
            Version = new Version(1, 0);
        }

        /// <summary>
        /// Get the message for the category.
        /// </summary>
        public T Message { get; set; }

        /// <summary>
        /// Get or set the version for the category.
        /// </summary>
        public Version Version { get; set; }

        #region Implementation of ICategory

        /// <summary>
        /// Get the message for the category.
        /// </summary>
        ICategoryMessage ICategory.Message => Message;

        /// <summary>
        /// Gets/Sets the version associated with the category.
        /// </summary>
        IVersion ICategory.Version
        {
            get => Version;
            set => Version = (Version)value;
        }

        #endregion
    }
}