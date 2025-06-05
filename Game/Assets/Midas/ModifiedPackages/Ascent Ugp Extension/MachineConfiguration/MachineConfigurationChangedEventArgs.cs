//-----------------------------------------------------------------------
// <copyright file = "MachineConfigurationChangedEventArgs.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    using System;
    using Ascent.Communication.Platform.Interfaces;

    /// <summary>
    /// Event arguments for UGP machine configuration being changed.
    /// </summary>
    [Serializable]
    public class MachineConfigurationChangedEventArgs : TransactionalEventArgs
    {
		/// <summary>
		/// The current <see cref="MachineConfigurationParameters"/>.
		/// </summary>
		public MachineConfigurationParameters MachineConfigurationParameters { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public MachineConfigurationChangedEventArgs(MachineConfigurationParameters machineConfigurationParameters)
		{
			MachineConfigurationParameters = machineConfigurationParameters;
		}
    }
}
