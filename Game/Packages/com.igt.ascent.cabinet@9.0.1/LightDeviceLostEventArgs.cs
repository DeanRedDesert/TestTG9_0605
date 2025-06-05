//-----------------------------------------------------------------------
// <copyright file = "LightDeviceLostEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;

    /// <summary>
    /// Event which indicates that a light device has been lost.
    /// </summary>
    public class LightDeviceLostEventArgs : EventArgs
    {
        /// <summary>
        /// FeatureId of the lost device.
        /// </summary>
        public string FeatureId { get; }

        /// <summary>
        /// Construct an instance of the event arguments.
        /// </summary>
        /// <param name="featureId">The id of the feature which was lost.</param>
        public LightDeviceLostEventArgs(string featureId)
        {
            FeatureId = featureId;
        }
    }
}
