// -----------------------------------------------------------------------
// <copyright file = "KeyMatch.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization.GameEvents
{
    /// <summary>
    /// Class containing default options for key match conditions.
    /// </summary>
    public static class KeyMatch
    {
        /// <summary>
        /// Compare a sender key to <see cref="BankSynchronizationController.ThemeId"/>.
        /// </summary>
        /// <param name="senderKey">The sender key.</param>
        /// <returns>True if the keys are the same.</returns>
        public static bool SameKey(string senderKey)
        {
            return string.Equals(senderKey, BankSynchronizationController.ThemeId);
        }

        /// <summary>
        /// Return true for any specified sender key.
        /// </summary>
        /// <param name="senderKey">The sender key.</param>
        /// <returns>True.</returns>
        public static bool AnyKey(string senderKey)
        {
            return true;
        }
    }
}