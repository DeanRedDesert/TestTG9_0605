// -----------------------------------------------------------------------
// <copyright file = "ICabinetServiceRestricted.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    /// <summary>
    /// Restricted interface for a device service. Exposes disconnect functionality.
    /// Connect functionalities are defined in base interface <see cref="IAsyncConnect"/>.
    /// </summary>
    /// <inheritdoc/>
    public interface ICabinetServiceRestricted : IAsyncConnect
    {
        /// <summary>
        /// Disconnect the service from its current CSI client.
        /// </summary>
        void Disconnect();
    }
}