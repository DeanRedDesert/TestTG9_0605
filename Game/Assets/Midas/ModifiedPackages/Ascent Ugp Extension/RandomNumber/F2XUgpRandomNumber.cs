//-----------------------------------------------------------------------
// <copyright file = "F2XUgpRandomNumber.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RandomNumber
{
    using System;
    using System.Collections.Generic;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpRandomNumber extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpRandomNumber : IUgpRandomNumber, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpRandomNumber category handler.
        /// </summary>
        private readonly IUgpRandomNumberCategory ugpRandomNumberCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpRandomNumber"/>.
        /// </summary>
        /// <param name="ugpRandomNumberCategory">
        /// The UgpRandomNumber category handler used to communicate with the foundation.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="ugpRandomNumberCategory"/> is null.
        /// </exception>
        public F2XUgpRandomNumber(IUgpRandomNumberCategory ugpRandomNumberCategory)
        {
            this.ugpRandomNumberCategory = ugpRandomNumberCategory ?? throw new ArgumentNullException(nameof(ugpRandomNumberCategory));
        }

        #endregion

        #region IUgpRandomNumber Implementation

        /// <inheritdoc/>
        public IEnumerable<double> GetRandomNumbers(uint numberOfRandomNumbers)
        {
            return ugpRandomNumberCategory.GetRandomNumbers(numberOfRandomNumbers);
        }

        #endregion
    }
}
