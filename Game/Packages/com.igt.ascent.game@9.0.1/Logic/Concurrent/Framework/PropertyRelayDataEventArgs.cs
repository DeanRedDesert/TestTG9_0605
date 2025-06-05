// -----------------------------------------------------------------------
// <copyright file = "PropertyRelayDataEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    /// <inheritdoc/>
    /// <typeparam name="T">Data type of the property.</typeparam>
    internal sealed class PropertyRelayDataEventArgs<T> : PropertyRelayEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the data for the property.
        /// </summary>
        public T Data { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyRelayDataEventArgs{T}"/>.
        /// </summary>
        /// <param name="propertyId">The property identifier.</param>
        /// <param name="data">The data for the property.</param>
        public PropertyRelayDataEventArgs(PropertyRelay propertyId, T data) : base(propertyId)
        {
            Data = data;
        }

        #endregion
    }
}