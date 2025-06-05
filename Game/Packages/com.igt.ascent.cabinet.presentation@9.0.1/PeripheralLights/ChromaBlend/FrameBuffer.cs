//-----------------------------------------------------------------------
// <copyright file = "FrameBuffer.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Streaming;

    /// <summary>
    /// Used to read lists of frames out of a layer.
    /// </summary>
    internal class FrameBuffer : IFrameBuffer
    {
        /// <summary>
        /// The maximum number of frames that can be in a chunk.
        /// </summary>
        public const int MaxChunkSize = 30;

        private readonly LightSequencePosition currentPosition = new LightSequencePosition();
        private Layer sequenceLayer;

        /// <summary>
        /// This array keeps track of the current state of all the LEDs at the end of the previous chunk.
        /// This allows the first frame of the next chunk to not have any linked colors in it.
        /// </summary>
        /// <remarks>
        /// An array was specifically used here so the colors would all be in contiguous memory space.
        /// </remarks>
        private Color[] lastChunkEndFrameColors;

        private readonly object lastChunkEndFrameColorsSynchronizer = new object();

        /// <summary>
        /// Constructs a frame buffer.
        /// </summary>
        /// <param name="layer">The light layer being buffered.</param>
        /// <exception cref="InvalidLayerException">Thrown if the passed in layer is null.</exception>
        public FrameBuffer(Layer layer)
        {
            SequenceLayer = layer ?? throw new InvalidLayerException("The layer passed in was null.");
        }

        /// <summary>
        /// Gets or sets the framebuffer's layer.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the property is set to null.
        /// </exception>
        internal Layer SequenceLayer
        {
            get => sequenceLayer;
            set
            {
                sequenceLayer = value ?? throw new ArgumentNullException(nameof(value));
                lock(currentPosition)
                {
                    currentPosition.Reset();
                }
                lastChunkEndFrameColors = null;
            }
        }

        /// <summary>
        /// Gets if the frame buffer has reached the end of the light sequence or not.
        /// </summary>
        public bool AtEnd
        {
            get
            {
                lock(currentPosition)
                {
                    return currentPosition.Segment >= SequenceLayer.LightSequence.Segments.Count;
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            lock(currentPosition)
            {
                return
                    $"Current Frame: {currentPosition.Frame} Current Segment: {currentPosition.Segment} Current Segment Loop: {currentPosition.SegmentLoop} AtEnd: {AtEnd}";
            }
        }

        /// <inheritdoc />
        public void SeekToFrame(int seekSegment, int seekSegmentLoop, int seekFrame)
        {
            lock(currentPosition)
            {
                currentPosition.SegmentLoop = seekSegmentLoop;
                currentPosition.Segment = seekSegment;
                currentPosition.Frame = seekFrame;
            }
        }

        /// <summary>
        /// Gets a copy of the current position of the buffer in the light sequence.
        /// </summary>
        /// <returns>A copy of the current light sequence position.</returns>
        public LightSequencePosition GetCurrentPosition()
        {
            LightSequencePosition positionCopy;
            lock(currentPosition)
            {
                positionCopy = currentPosition.Clone() as LightSequencePosition;
            }
            return positionCopy;
        }

        /// <inheritdoc />
        public List<Frame> ReadChunk()
        {
            return ReadChunk(MaxChunkSize);
        }

        /// <inheritdoc />
        public List<Frame> ReadChunk(int frameCount)
        {
            List<Frame> frames;
            lock(lastChunkEndFrameColorsSynchronizer)
            {
                frames = ReadFromSequence(frameCount);

                if(frames.Count > 0)
                {
                    var framesToSkip = 0;
                    if(lastChunkEndFrameColors == null)
                    {
                        lastChunkEndFrameColors = new Color[frames[0].ColorFrameData.Count];
                    }
                    else
                    {
                        // Apply the last chunk frame end colors to the first frame of this chunk.
                        var firstFrameColors = frames[0].ColorFrameData;
                        var colorsWereChanged = false;
                        for(var index = 0; index < firstFrameColors.Count; index++)
                        {
                            if(firstFrameColors[index].IsLinkedColor)
                            {
                                firstFrameColors[index] = lastChunkEndFrameColors[index];
                                colorsWereChanged = true;
                            }
                            else
                            {
                                // If the first frame color isn't linked, save its color/state into the last frame image.
                                lastChunkEndFrameColors[index] = firstFrameColors[index];
                            }
                        }
                        if(colorsWereChanged)
                        {
                            frames[0].UpdateColors(firstFrameColors);
                        }
                        framesToSkip++;
                    }

                    foreach(var frameColors in frames.Skip(framesToSkip).Select(frame => frame.ColorFrameData))
                    {
                        for(var index = 0; index < frameColors.Count; index++)
                        {
                            if(!frameColors[index].IsLinkedColor)
                            {
                                lastChunkEndFrameColors[index] = frameColors[index];
                            }
                        }
                    }
                }
            }

            return frames;
        }

        /// <summary>
        /// Read a set of frames from the light sequence.
        /// </summary>
        /// <param name="frameCount">The amount of frames to read, can't be larger than MaxChunkSize.</param>
        /// <returns>A list of frames from the sequence.</returns>
        private List<Frame> ReadFromSequence(int frameCount)
        {
            // MaxChunkSize is the max size we can read.
            if(frameCount > MaxChunkSize)
            {
                frameCount = MaxChunkSize;
            }

            var frames = new List<Frame>();

            lock(currentPosition)
            {
                // Step through each segment to grab needed frames.
                while(currentPosition.Segment < SequenceLayer.LightSequence.Segments.Count)
                {
                    var segment = SequenceLayer.LightSequence.Segments[currentPosition.Segment];
                    // Even if the segment doesn't loop we need to iterate over it once.
                    while(currentPosition.SegmentLoop < segment.Loop + 1)
                    {
                        while(currentPosition.Frame < segment.Frames.Count)
                        {
                            if(frames.Count >= frameCount)
                            {
                                return frames;
                            }
                            frames.Add(segment.Frames[currentPosition.Frame]);
                            ++currentPosition.Frame;
                        }
                        ++currentPosition.SegmentLoop;
                        currentPosition.Frame = 0;
                    }
                    ++currentPosition.Segment;
                    currentPosition.SegmentLoop = 0;
                }

                // If we loop and need more frames, grab more frames.
                if(SequenceLayer.LightSequence.Loop)
                {
                    // Reset the segment back to the beginning of the light sequence.
                    if(AtEnd)
                    {
                        currentPosition.Segment = 0;
                    }
                    if(frames.Count < frameCount)
                    {
                        frames.AddRange(ReadFromSequence(frameCount - frames.Count));
                    }
                }
            }

            return frames;
        }
    }
}
