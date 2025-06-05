//-----------------------------------------------------------------------
// <copyright file = "FrameBuffer.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System.Collections.Generic;
    using Streaming;

    /// <summary>
    /// The interface for a frame buffer.
    /// </summary>
    public interface IFrameBuffer
    {
        /// <summary>
        /// Seeks to a specific frame in the buffer.
        /// </summary>
        /// <param name="seekSegment">The segment to seek.</param>
        /// <param name="seekSegmentLoop">How many loops into the segment to seek.</param>
        /// <param name="seekFrame">The frame to seek.</param>
        void SeekToFrame(int seekSegment, int seekSegmentLoop, int seekFrame);

        /// <summary>
        /// Read in the maximum chunk size of data.
        /// </summary>
        /// <returns>Returns the frame chunk within the time slice.</returns>
        List<Frame> ReadChunk();

        /// <summary>
        /// Read a specific number of frames from the buffer.
        /// </summary>
        /// <param name="frameCount">THe amount of frames to be read, can't be larger than MaxChunkSize.</param>
        /// <returns>Returns the frame chunk within the time slice.</returns>
        List<Frame> ReadChunk(int frameCount);
    }
}