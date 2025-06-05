//-----------------------------------------------------------------------
// <copyright file = "ILightSequence.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.Generic;
    using PeripheralLights;

    /// <summary>
    /// The interface to a light sequence.
    /// </summary>
    public interface ILightSequence : IEquatable<ILightSequence>
    {
        /// <summary>
        /// The name of the light sequence.
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// The light device this sequence is intended for.
        /// </summary>
        StreamingLightHardware LightDevice
        {
            get;
        }

        /// <summary>
        /// Gets if the whole light sequence should loop or not.
        /// </summary>
        bool Loop
        {
            get;
        }

        /// <summary>
        /// The version of the light sequence.
        /// </summary>
        ushort Version
        {
            get;
        }

        /// <summary>
        /// The segments in this sequence.
        /// </summary>
        IList<Segment> Segments
        {
            get;
        }

        /// <summary>
        /// The display time of the light sequence in milliseconds.
        /// </summary>
        ulong DisplayTime
        {
            get;
        }

        /// <summary>
        /// The pre generated key for the light sequence.
        /// </summary>
        string UniqueId
        {
            get;
        }

        /// <summary>
        /// Encodes a light sequence into a base 64 string.
        /// </summary>
        /// <returns>The base 64 encoded string.</returns>
        /// <remarks>
        /// This function is used to encode the light sequence into bytes.
        /// </remarks>
        byte[] GetSequenceBytes();
    }
}
