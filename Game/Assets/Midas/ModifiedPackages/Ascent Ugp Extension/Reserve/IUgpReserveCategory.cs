//-----------------------------------------------------------------------
// <copyright file = "IUgpReserveCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Reserve
{
    /// <summary>
    /// Defines an interface that allows a package to retrieve Reserve parameters from the foundation
    /// and notify the foundation whenever Reserve's activation changes.
    /// </summary>
    public interface IUgpReserveCategory
    {
		/// <summary>
		/// Gets the reserve parameters from the foundation.
		/// </summary>
		ReserveParameters GetReserveParameters();

        /// <summary>
        /// Sends the Reserve activation status to the foundation.
        /// </summary>
        /// <param name="isActive">The flag indicating whether Reserve is active or not.</param>
        void SendActivationChanged(bool isActive);
    }
}
