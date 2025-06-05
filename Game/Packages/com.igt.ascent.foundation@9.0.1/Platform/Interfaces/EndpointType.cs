//-----------------------------------------------------------------------
// <copyright file = "EndpointType.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;

    /// <summary>
    /// The enumeration defines the endpoints of the parcel communication.
    /// </summary>
    [Serializable]
    public enum EndpointType
    {
        /// <summary>
        /// Indicates that parcel communication endpoint is a Theme.
        /// </summary>
        Theme,

        /// <summary>
        /// Indicates that parcel communication endpoint is a Shell.
        /// </summary>
        Shell,

        /// <summary>
        /// Indicates that parcel communication endpoint is an Extension.
        /// </summary>
        Extension,

        /// <summary>
        /// Indicates that parcel communication endpoint is a Potato Cannon.
        /// </summary>
        Ptc,

        /// <summary>
        /// Parcel communication endpoint is the CTC (Common Theme Control).
        /// </summary>
        CommonThemeControl,

        /// <summary>
        /// Parcel communication endpoint is the Chooser.
        /// </summary>
        Chooser
    }
}
