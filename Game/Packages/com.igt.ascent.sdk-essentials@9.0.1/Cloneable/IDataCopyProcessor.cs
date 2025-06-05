// -----------------------------------------------------------------------
// <copyright file = "IDataCopyProcessor.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Cloneable
{
    /// <summary>
    /// This interface is used to deep copy objects of types from assemblies that does not have access
    /// to the type <see cref="IDeepCloneable"/>.
    /// A typical example is the types from .net framework class libraries,
    /// such as List, Dictionary, KeyValuePair, etc.
    /// </summary>
    /// <remarks>
    /// Please don't implement <see cref="IDataCopyProcessor"/> for cloning types that are derived
    /// from <see cref="string"/>, or types that are immutable, such as the primitive value types
    /// and <see cref="IDeepCloneable"/>.
    /// </remarks>
    public interface IDataCopyProcessor
    {
        /// <summary>
        /// Deep copy the source object.
        /// </summary>
        /// <param name="source">The source object.</param>
        /// <returns>The copied object.</returns>
        object DeepCopy(object source);
    }
}