//-----------------------------------------------------------------------
// <copyright file = "CategoryRequest.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XTransport
{
    /// <summary>
    /// The delegate for creating a category implementation.
    /// </summary>
    /// <param name="dependencies">The dependencies for the category creation.</param>
    /// <returns>The category implementation created.</returns>
    public delegate IApiCategory CreateCategoryDelegate(ICategoryCreationDependencies dependencies);

    /// <summary>
    /// Class for creating category requests.
    /// </summary>
    public class CategoryRequest
    {
        #region Public Properties

        /// <summary>
        /// The category, and version of the category, being requested.
        /// </summary>
        public CategoryVersionInformation CategoryVersionInformation { get; private set; }

        /// <summary>
        /// Boolean indicating if the category is required.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// The foundation version the category targets.
        /// </summary>
        public FoundationTarget FoundationTarget { get; private set; }

        /// <summary>
        /// Gets the function which creates the category implementation.
        /// </summary>
        public CreateCategoryDelegate CreateCategory { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a category request whose Required flag is set to false.
        /// </summary>
        /// <param name="categoryVersionInformation">The category and its version.</param>
        /// <param name="foundationTarget">The foundation version the category targets.</param>
        /// <param name="createMethod">Method which creates a category implementation.</param>
        public CategoryRequest(CategoryVersionInformation categoryVersionInformation,
                               FoundationTarget foundationTarget,
                               CreateCategoryDelegate createMethod)
            : this(categoryVersionInformation, false, foundationTarget, createMethod)
        {
        }

        /// <summary>
        /// Create a category request with the given information.
        /// </summary>
        /// <param name="categoryVersionInformation">The category and its version.</param>
        /// <param name="required">Flag indicating if the category is required.</param>
        /// <param name="foundationTarget">The foundation version the category targets.</param>
        /// <param name="createMethod">Method which creates a category implementation.</param>
        public CategoryRequest(CategoryVersionInformation categoryVersionInformation,
                               bool required,
                               FoundationTarget foundationTarget,
                               CreateCategoryDelegate createMethod)
        {
            CategoryVersionInformation = categoryVersionInformation;
            Required = required;
            FoundationTarget = foundationTarget;
            CreateCategory = createMethod;
        }

        #endregion
    }
}
