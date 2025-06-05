//-----------------------------------------------------------------------
// <copyright file = "GL2PInterceptorImageFormat.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.GL2PInterceptorLib
{
    using System;

    /// <summary>
    /// This enumeration is used to indicate an image format.
    /// </summary>
    [Serializable]
    public enum GL2PInterceptorImageFormat
    {
        /// <summary>
        /// Png File Format.
        /// </summary>
        Png,

        /// <summary>
        /// A 16 bits/pixel image format.
        /// </summary>
        ARGB4444,
    };
}
