//-----------------------------------------------------------------------
// <copyright file = "Wheel.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.MechanicalReels
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet.MechanicalReels;

    /// <summary>
    /// Mechanical reel implementation for wheel devices.
    /// </summary>
    public class Wheel : MechanicalReelDevice
    {
        /// <summary>
        /// Constructor taking a <see cref="ReelFeatureDescription"/> argument.
        /// </summary>
        /// <param name="description">The <see cref="ReelFeatureDescription"/> of this reel device.</param>
        /// <exception cref="ArgumentNullException">Thrown if description is null.</exception>
        public Wheel(ReelFeatureDescription description) : base(description)
        {
        }

        /// <summary>
        /// Spins the wheel using the specified profile.
        /// </summary>
        /// <param name="profile">The spin profile to use.</param>
        /// <returns>The <see cref="ReelCommandResult"/> of this command.</returns>
        public ReelCommandResult Spin(SpinProfile profile)
        {
            if(Description != null)
            {
                return MechanicalReels.Spin(Description.FeatureId, new List<SpinProfile> { profile });
            }

            return ReelCommandResult.Failed;
        }
    }
}
