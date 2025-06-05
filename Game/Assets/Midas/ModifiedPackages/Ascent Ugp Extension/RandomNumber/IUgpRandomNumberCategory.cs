//-----------------------------------------------------------------------
// <copyright file = "IUgpRandomNumberCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.RandomNumber
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines an interface to retrieve random numbers from foundation.
    /// </summary>
    public interface IUgpRandomNumberCategory
    {
        /// <summary>
        /// Gets a collection of random numbers greater than or equal to 0 and less than 1.0.
        /// </summary>
        /// <param name="numberOfRandomNumbers">Specify the number of random numbers to retrieve.</param>
        IList<double> GetRandomNumbers(uint numberOfRandomNumbers);
    }
}
