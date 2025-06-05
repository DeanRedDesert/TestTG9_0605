//-----------------------------------------------------------------------
// <copyright file = "IUgpMachineConfigurationCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    /// <summary>
    /// Interface of UgpMachineConfiguration category messages.
    /// </summary>
    public interface IUgpMachineConfigurationCategory
    {
		/// <summary>
		/// Returns the <see cref="MachineConfigurationParameters"/>.
		/// </summary>
		MachineConfigurationParameters GetMachineConfigurationParameters();
    }
}