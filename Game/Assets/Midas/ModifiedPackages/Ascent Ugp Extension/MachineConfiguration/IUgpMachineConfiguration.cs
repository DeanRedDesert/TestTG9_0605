//-----------------------------------------------------------------------
// <copyright file = "IUgpMachineConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    using System;

    /// <summary>
    /// Define an interface that allows the package to retrieve the machine configuration information.
    /// </summary>
    public interface IUgpMachineConfiguration
    {
        /// <summary>
        /// Event raised when the machine configuration has changed.
        /// </summary>
        event EventHandler<MachineConfigurationChangedEventArgs> MachineConfigurationChanged;

		/// <summary>
		/// Returns the machine configuration parameters.
		/// </summary>
		MachineConfigurationParameters GetMachineConfigurationParameters();
    }
}