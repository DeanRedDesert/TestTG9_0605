//-----------------------------------------------------------------------
// <copyright file = "ILightFeatureState.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides a read-only view of the state of a light feature.
    /// </summary>
    public interface ILightFeatureState
    {
        /// <summary>
        /// The unique identifier for the feature.
        /// </summary>
        string FeatureId { get; }

        /// <summary>
        /// Current state of the groups in the feature.
        /// </summary>
        IDictionary<byte, ILightGroupState> GroupStates { get; }
    }
}
