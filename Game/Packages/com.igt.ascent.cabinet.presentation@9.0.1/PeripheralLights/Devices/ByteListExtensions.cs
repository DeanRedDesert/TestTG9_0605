//-----------------------------------------------------------------------
// <copyright file = "ByteListExtensions.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extends the List class with some helpful functions when packing hardware parameters
    /// </summary>
    internal static class ByteListExtensions
    {
        /// <summary>
        /// Splits a unsigned 16-bit integer into two bytes and adds each byte to the list.
        /// </summary>
        /// <param name="list">The byte list.</param>
        /// <param name="word">The UInt16 number to add.</param>
        public static void Add(this IList<byte> list, ushort word)
        {
            var lower = Convert.ToByte(word & 0xFF);
            var upper = Convert.ToByte((word & 0xFF00) >> 8);

            list.Add(lower);
            list.Add(upper);
        }
    }
}
