//-----------------------------------------------------------------------
// <copyright file = "IPick.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    /// <summary>
    /// This interface represents a weighted pick object that could be picked up from a collection.
    /// </summary>
    public interface IPick
    {
        /// <summary>
        /// Gets the name of this pick.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Gets/sets the weight of this pick.
        /// </summary>
        uint Weight { get; set; }
    }
}