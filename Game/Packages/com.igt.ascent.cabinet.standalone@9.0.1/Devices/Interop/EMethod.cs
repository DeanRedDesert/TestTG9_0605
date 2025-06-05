//-----------------------------------------------------------------------
// <copyright file = "EMethod.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.Interop
{
    using System;

#pragma warning disable 1591

    /// <summary>
    /// Constant mappings for methods.
    /// Based on code pulled from http://www.pinvoke.net
    /// </summary>
    [Serializable]
    internal enum EMethod : uint
    {
        Buffered = 0,
        InDirect = 1,
        OutDirect = 2,
        Neither = 3
    }    

    #pragma warning restore 1591
}
