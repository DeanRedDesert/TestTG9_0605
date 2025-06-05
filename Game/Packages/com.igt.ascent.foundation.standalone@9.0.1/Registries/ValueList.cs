//-----------------------------------------------------------------------
// <copyright file = "ValueList.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone.Registries
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Define the value list type where all values are enumerated in a predefined list.
    /// </summary>
    /// <typeparam name="T">The type of value in the value list.</typeparam>
    public class ValueList<T> : IValuePool<T>
    {
        /// <summary>
        /// The list of values of this value list.
        /// </summary>
        public ICollection<T> Values { get; }

        /// <summary>
        /// Constructor with a specified list of values.
        /// </summary>
        /// <param name="values">Specify the list of values.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the values is not defined at object construction.
        /// </exception>
        public ValueList(ICollection<T> values)
        {
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        /// <inheritDoc />
        public bool Contains(T value)
        {
            return Values.Contains(value);
        }
    }
}
