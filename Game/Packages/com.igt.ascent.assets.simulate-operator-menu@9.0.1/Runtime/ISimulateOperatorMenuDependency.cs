// -----------------------------------------------------------------------
// <copyright file = "ISimulateOperatorMenuDependency.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Assets.SimulateOperatorMenu
{
    using Communication.Platform.Interfaces;

    /// <summary>
    /// The dependency upon which Simulate Operator Menu relies in order to function.
    /// </summary>
    public interface ISimulateOperatorMenuDependency
    {
        /// <summary>
        /// Gets the object that is in charge of changing game mode.
        /// </summary>
        ISimulateGameModeControl GameModeControl { get; }

        /// <summary>
        /// Checks if entering the operator menu is allowed at this moment.
        /// </summary>
        /// <returns>
        /// True if entering the operator menu is allowed at this moment.
        /// </returns>
        bool CanEnterOperatorMenu();
    }
}