// -----------------------------------------------------------------------
// <copyright file = "PropertyRelayEventArgs.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using Communication.Platform.Interfaces;

    /// <summary>
    /// Event indicating new data for a property is being relayed to the coplayers.  Sent by Shell.
    /// </summary>
    internal class PropertyRelayEventArgs : NonTransactionalEventArgs
    {
        #region Properties

        /// <summary>
        /// Gets the property identifier.
        /// </summary>
        public PropertyRelay PropertyId { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyRelayEventArgs"/>.
        /// </summary>
        /// <param name="propertyId">The property identifier.</param>
        protected PropertyRelayEventArgs(PropertyRelay propertyId)
        {
            PropertyId = propertyId;
        }

        #endregion
    }
}