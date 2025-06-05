//-----------------------------------------------------------------------
// <copyright file = "ShowDemoEnvironment.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// This enumeration defines the current machine's environment.
    /// </summary>
    [Serializable]
    public enum ShowDemoEnvironment
    {
        /// <summary>
        /// The machine is in an unknown state.
        /// </summary>
        Unknown,
        
        /// <summary>
        /// The machine is in a development environment.
        /// </summary>
        Development,
        
        /// <summary>
        /// The machine is currently set up for a show demo environment.
        /// </summary>
        ShowDemo
    }
}
