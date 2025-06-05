// -----------------------------------------------------------------------
// <copyright file = "IGameGroupControlInquiry.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Interfaces
{
    /// <summary>
    /// Interface to inquiry the game group control extension.
    /// </summary>
    public interface IGameGroupControlInquiry : IInterfaceExtensionInquiry
    {
        /// <summary>
        /// Gets whether a paytable group is present for the active context.
        /// </summary>
        /// <returns>Whether a paytable group is present.</returns>
        bool IsPaytableGroupPresent();

        /// <summary>
        /// Gets the redefined maximum bet in credits for the currently selected paytable in the game group.
        /// </summary>
        /// <returns>
        /// The redefined maximum bet in credits for the current paytable in the game group.
        /// If not in a game group, returns 0.
        /// </returns>
        long GetCurrentRedefinedMaxBetCredits();
    }
}
