//-----------------------------------------------------------------------
// <copyright file = "F2XUgpProgressive.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Progressive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpProgressive extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpProgressive : IUgpProgressive, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpProgressive category handler.
        /// </summary>
        private readonly IUgpProgressiveCategory ugpProgressiveCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpProgressive"/>.
        /// </summary>
        /// <param name="ugpProgressiveCategory">
        /// The UgpProgressive category used to communicate with the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="ugpProgressiveCategory"/> is null.
        /// </exception>
        public F2XUgpProgressive(IUgpProgressiveCategory ugpProgressiveCategory)
        {
            this.ugpProgressiveCategory = ugpProgressiveCategory ?? throw new ArgumentNullException(nameof(ugpProgressiveCategory));
        }

        #endregion

        #region IUgpProgressive Implementation

        /// <inheritdoc/>
        public ProgressiveLevel GetProgressiveLevel(string progressiveId)
        {
            return ugpProgressiveCategory.GetProgressiveLevel(progressiveId).ToPublic();
        }

        /// <inheritdoc/>
        public IEnumerable<ProgressiveLevel> GetAllProgressives()
        {
            return ugpProgressiveCategory.GetAllProgressives().Select(level => level.ToPublic());
        }

        #endregion
    }
}
