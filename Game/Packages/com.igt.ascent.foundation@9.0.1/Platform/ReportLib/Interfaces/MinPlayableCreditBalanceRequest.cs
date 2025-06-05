//-----------------------------------------------------------------------
// <copyright file = "MinPlayableCreditBalanceRequest.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ReportLib.Interfaces
{
    /// <summary>
    /// The arguments and result for the minimum playable credit balance request.
    /// </summary>
    public class MinPlayableCreditBalanceRequest
    {
        /// <summary>
        /// Gets the identifier of the theme.
        /// This is the identifier maintained by the Foundation, used in communication
        /// with the Foundation to identify a theme.
        /// </summary>
        public string ThemeIdentifier { get; }

        /// <summary>
        /// Gets the identifier of the paytable.
        /// This is the identifier maintained by the Foundation, used in communication
        /// with the Foundation to identify an individual "pay variation".
        /// </summary>
        public string PaytableIdentifier { get; }

        /// <summary>
        /// Gets the denomination.
        /// </summary>
        public uint Denomination { get; }

        /// <summary>
        /// The minimum player-wagerable credit balance amount required to commit a new game-cycle
        /// for the theme, payvar and denomination combination given.
        /// This is to consider all current configuration settings including theme-specific custom configurations items,
        /// minimum selectable lines, etc.
        /// However it should NOT consider configuration setting related to Credit Playoff, including whether or not
        /// Credit Playoff is enabled.
        /// </summary>
        public long MinPlayableCreditBalance { get; set; }

        /// <summary>
        /// Initializes an instance of <see cref="MinPlayableCreditBalanceRequest"/>.
        /// </summary>
        /// <param name="themeIdentifier">The identifier of the theme to use for the minimum playable credit balance amount.</param>
        /// <param name="paytableIdentifier">The identifier of the paytable to use for the minimum playable credit balance amount.</param>
        /// <param name="denomination">The denomination to use for the minimum playable credit balance amount.</param>
        public MinPlayableCreditBalanceRequest(string themeIdentifier, string paytableIdentifier, uint denomination)
        {
            ThemeIdentifier = themeIdentifier;
            PaytableIdentifier = paytableIdentifier;
            Denomination = denomination;
        }
    }
}
