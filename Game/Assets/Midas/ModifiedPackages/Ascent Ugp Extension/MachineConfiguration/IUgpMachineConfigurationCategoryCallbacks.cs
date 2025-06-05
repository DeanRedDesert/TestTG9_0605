//-----------------------------------------------------------------------
// <copyright file = "IUgpMachineConfigurationCategoryCallbacks.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MachineConfiguration
{
    /// <summary>
    /// Interface to accept callbacks from the UGP MachineConfiguration category.
    /// </summary>
    public interface IUgpMachineConfigurationCategoryCallbacks
    {
        /// <summary>
        /// Method called when UgpMachineConfigurationCategory SetParameters message is received from the foundation.
        /// </summary>
        /// <returns>
        /// An error message if an error occurs; otherwise, null.
        /// </returns>
        string ProcessSetParameters(MachineConfigurationParameters parameters);
    }
}
