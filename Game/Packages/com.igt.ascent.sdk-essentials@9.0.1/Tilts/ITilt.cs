//-----------------------------------------------------------------------
// <copyright file = "ITilt.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Tilts
{
    /// <summary>
    /// An interface that encapsulates a tilt object which defines a tilt discard behavior. If the tilt discard behavior is
    /// set to <see cref="TiltDiscardBehavior.Never"/>, then the tilt object will persist on a power hit or when the theme
    /// context gets activated/re-activated. If the tilt discard behavior is set to
    /// <see cref="TiltDiscardBehavior.OnGameTermination"/>, then the tilt object will not persist on a power hit or when the
    /// theme context gets activated/re-activated. The tilt object will be discarded when the theme gets inactivated.
    /// </summary>
    public interface ITilt : IActiveTilt
    {
        /// <summary>
        /// Defines whether the tilt will be discarded upon game unload.
        /// </summary>
        TiltDiscardBehavior DiscardBehavior { get; }
    }
}
