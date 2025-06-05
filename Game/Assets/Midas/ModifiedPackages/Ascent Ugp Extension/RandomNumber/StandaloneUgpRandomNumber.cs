//-----------------------------------------------------------------------
// <copyright file = "StandaloneUgpRandomNumber.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RandomNumber
{
    using System;
    using Interfaces;
    using System.Collections.Generic;

    /// <summary>
    /// Standalone implementation of the UgpRandomNumber extended interface.
    /// </summary>
    internal sealed class StandaloneUgpRandomNumber : IUgpRandomNumber, IInterfaceExtension
    {
        #region Private Fields

        private readonly Random random;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="StandaloneUgpRandomNumber"/>.
        /// </summary>
        public StandaloneUgpRandomNumber()
        {
            random = new Random();
        }

        #endregion

        #region IUgpRandomNumbers Implementation

        /// <inheritdoc/>
        public IEnumerable<double> GetRandomNumbers(uint numberOfRandomNumbers)
        {
            var numbers = new List<double>();
            for(var i = 0; i < numberOfRandomNumbers; i++)
            {
                numbers.Add(random.NextDouble());
            }

            return numbers;
        }

        #endregion
    }
}
