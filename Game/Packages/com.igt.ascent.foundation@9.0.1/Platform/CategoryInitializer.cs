// -----------------------------------------------------------------------
// <copyright file = "CategoryInitializer.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform
{
    using System;

    /// <summary>
    /// A wrapper class that handles the initialization of an F2X category instance, and
    /// always checks whether the instance has been initialized before accessing it.
    /// </summary>
    /// <remarks>
    /// This class is specifically designed to be used by classes that work as the "translator"
    /// between a public interface and an F2X category.  A "translator" class typically holds
    /// a reference to an F2X category interface which cannot be set by constructor, but has to
    /// be initialized at later times.  This class helps with the initialization and checking
    /// the nullness of the category instance.  Do not use it for other purposes.
    /// </remarks>
    /// <typeparam name="TCategory">
    /// The type of the category interface that needs initialization.
    /// </typeparam>
    internal class CategoryInitializer<TCategory> where TCategory : class
    {
        #region Private Fields

        private TCategory instance;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the initialized category instance.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the instance has not been initialized to non-null value.
        /// </exception>
        public TCategory Instance
        {
            get
            {
                if(instance == null)
                {
                    throw new InvalidOperationException(typeof(TCategory) + " cannot be used without being initialized first.");
                }

                return instance;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the category instance.
        /// </summary>
        /// <param name="inst">
        /// The value to initialize with.
        /// </param>
        /// <devdoc>
        /// The caller should have checked the nullness already.
        /// For efficiency, we don't re-check it here.
        /// </devdoc>
        public void Initialize(TCategory inst)
        {
            instance = inst;
        }

        #endregion
    }
}