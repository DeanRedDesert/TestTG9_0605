//-----------------------------------------------------------------------
// <copyright file = "IShowDemo.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.ShellLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines APIs for a game to talk to the Foundation in terms of the show demo,
    /// such as the Foundation's show or development status and calls only available in such environments.
    /// </summary>
    public interface IShowDemo
    {
        /// <summary>
        /// Gets the flag determining if the ShowDemo is available.  This is false when in a release environment.
        /// If the ShowDemo is not available, calls to other functionality in this class will
        /// be ignored or return undefined values.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets the flag indicating the machine's environment (demo, development, unknown).
        /// </summary>
        ShowDemoEnvironment ShowDemoEnvironment { get; }

        /// <summary>
        /// Make a request to add money to the player's balance.
        /// </summary>
        /// <param name="amount">The amount in base units to add to the player's balance.</param>
        void AddMoney(long amount);
    }
}
