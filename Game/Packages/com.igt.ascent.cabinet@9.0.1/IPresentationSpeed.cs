//-----------------------------------------------------------------------
// <copyright file = "IPresentationSpeed.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Interface for presentation speed category.
    /// </summary>
    public interface IPresentationSpeed
    {
        /// <summary>
        /// Gets the <see cref="PresentationSpeedInfo"/> from the Foundation.
        /// </summary>
        /// <returns>The presentation speed info.</returns>
        PresentationSpeedInfo GetPresentationSpeedInfo();

        /// <summary>
        /// Sets the presentation speed.
        /// </summary>
        /// <param name="presentationSpeed">
        /// The current value of the presentation speed. The speed of the presentation is restricted between 0 and 100.
        /// 100 indicates the maximum possible speed, while 0 the minimum possible speed.
        /// </param>
        void SetPresentationSpeed(uint presentationSpeed);
    }
}