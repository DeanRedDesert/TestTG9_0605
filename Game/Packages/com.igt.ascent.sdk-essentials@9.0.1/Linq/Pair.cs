//-----------------------------------------------------------------------
// <copyright file = "Pair.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Linq
{
    /// <summary>Generic pair of two types.</summary>
    /// <typeparam name="TFirst">Type of the First object.</typeparam>
    /// <typeparam name="TSecond">Type of the Second object.</typeparam>
    public class Pair<TFirst, TSecond>
    {
        /// <summary>First object.</summary>
        public TFirst First;

        /// <summary>Second object.</summary>
        public TSecond Second;
    }
}
