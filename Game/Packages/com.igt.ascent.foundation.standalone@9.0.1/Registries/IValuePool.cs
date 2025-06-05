//-----------------------------------------------------------------------
// <copyright file = "IValuePool.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    /// <summary>
    /// The interface represents a value pool object and is used to retrieve the value pool from registry files.
    /// </summary>
    /// <typeparam name="T">The type of value in the value pool.</typeparam>
    public interface IValuePool<T>
    {
        /// <summary>
        /// Check if value pool contains the specified value.
        /// </summary>
        /// <param name="value">Specify the value to check within this value pool.</param>
        /// <returns>true if value pool contains the specified value, false otherwise.</returns>
        bool Contains(T value);
    }
}
