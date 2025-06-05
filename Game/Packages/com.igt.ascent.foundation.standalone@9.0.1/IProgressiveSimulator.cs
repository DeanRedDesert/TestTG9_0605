// -----------------------------------------------------------------------
//  <copyright file = "IProgressiveSimulator.cs" company = "IGT">
//      Copyright (c) 2022 IGT.  All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.Standalone
{
    /// <summary>
    /// Defines an interface to a network progressive contribution simulation.
    /// </summary>
    public interface IProgressiveSimulator
    {
        /// <summary>
        /// Starts an initialized simulator, which will begin to contribute to progressives in the background.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops an initialized simulator, which will stop contributing to progressives in the background.
        /// </summary>
        void Stop();
    }
}
