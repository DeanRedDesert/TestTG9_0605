// -----------------------------------------------------------------------
// <copyright file = "IDeepCloneable.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Cloneable
{
    /// <summary>
    /// This interface defines the method used to deep clone an object.
    /// </summary>
    public interface IDeepCloneable
    {
        /// <summary>
        /// Deep clone the object.
        /// </summary>
        /// <returns>The cloned object.</returns>
        object DeepClone();
    }
}