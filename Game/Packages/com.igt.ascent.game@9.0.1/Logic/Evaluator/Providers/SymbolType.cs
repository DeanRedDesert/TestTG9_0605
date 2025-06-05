//-----------------------------------------------------------------------
// <copyright file = "SymbolType.cs" company = "IGT">
//     Copyright (c) IGT 2017.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Providers
{
    /// <summary>
    /// Types of a symbol. This affects the rules that are applied to the symbol during presentation.
    /// </summary>
    public enum SymbolType
    {
        /// <summary>
        /// Low value symbol. 
        /// </summary>
        /// <remarks>
        /// Rule: The same SymbolIDs are not allowed to be next to each other.
        /// </remarks>
        LowValue = 0,

        /// <summary>
        /// High value symbol.
        /// </summary>
        /// <remarks>
        /// Rule: Same SymbolIDs are not allowed to be visible on the same reel picture. 
        /// </remarks>
        HighValue = 1,

        /// <summary>
        /// Special symbol such as Trigger symbols. 
        /// </summary>
        /// <summary>
        /// Rule: Max. one Special symbol is allowed to be visible on the same reel picture. 
        /// </summary>
        Special = 2
    }
}