//-----------------------------------------------------------------------
// <copyright file = "ChromaBlender.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Streaming;

    /// <summary>
    /// Blends multiple light sequences together into a single light sequence.
    /// </summary>
    /// <remarks>
    /// The light blending must be thread safe.
    /// </remarks>
    public class ChromaBlender
    {
        private const int StandardMaxLayerCount = 8;
        private const int ReelBackLightMaxLayerCount = 30;
        private readonly SortedDictionary<byte, FrameBuffer> frameBuffers = new SortedDictionary<byte, FrameBuffer>();
        private readonly SortedDictionary<byte, LightSequencePosition> previousChunkPosition = new SortedDictionary<byte, LightSequencePosition>();
        private byte chunkIdentifier;

        /// <summary>
        /// A list of devices that do not support light blending.
        /// </summary>
        private static readonly List<StreamingLightHardware> InvalidBlendingDevices =
            new List<StreamingLightHardware>
            {
                StreamingLightHardware.AlcoveFlatpack,
                StreamingLightHardware.AlcoveEndCap,
                StreamingLightHardware.AlcoveCarousel,
                StreamingLightHardware.Spectrum
            };

        /// <summary>
        /// Constructs a new blender.
        /// </summary>
        public ChromaBlender()
        {
            LightSequenceVersion = 1;
        }

        #region Properties

        /// <summary>
        /// Gets or sets the light squence version to use when making sequences.
        /// </summary>
        public ushort LightSequenceVersion { get; set; }

        /// <summary>
        /// Gets the number of layers in the current blend.
        /// </summary>
        public int LayerCount
        {
            get
            {
                // Add up all of the framebuffers with non null layers.
                lock(frameBuffers)
                {
                    return frameBuffers.Count(frameBuffer => frameBuffer.Value.SequenceLayer != null);
                }
            }
        }

        #endregion

        #region Get Chunks as Raw Frames

        /// <summary>
        /// Gets the next chunk of frames.
        /// </summary>
        /// <param name="identifier">The identifier for this particular chunk.</param>
        /// <returns>The list of frames to display.</returns>
        public IList<Frame> GetNextChunk(out byte identifier)
        {
            lock(frameBuffers)
            {
                var chunk = BlendChunk();
                if(chunk != null)
                {
                    chunkIdentifier++;
                    identifier = chunkIdentifier;
                }
                else
                {
                    identifier = 0;
                }
                return chunk;
            }
        }

        /// <summary>
        /// Reblends the previous chunk that was requested with the current frame buffers.
        /// </summary>
        /// <param name="frameCount">The number of frames to blend.</param>
        /// <param name="identifier">The identifier for the chunk that was reblended.</param>
        /// <returns>The re-blended chunk of frames.</returns>
        public IList<Frame> ReBlendPreviousChunk(int frameCount, out byte identifier)
        {
            lock(frameBuffers)
            {
                // Roll the frame buffer back to its previous position.
                foreach(var pair in previousChunkPosition.Where(pair => frameBuffers.ContainsKey(pair.Key)))
                {
                    frameBuffers[pair.Key].SeekToFrame(pair.Value.Segment, pair.Value.SegmentLoop, pair.Value.Frame);
                }
                identifier = chunkIdentifier;
            }

            return BlendChunk(frameCount);
        }

        #endregion

        #region Older Pre-G Series Methods

        /// <summary>
        /// Takes the next chunk of lights and converts it into a light sequence.
        /// </summary>
        /// <param name="hardwareType">The hardware type the lights will be sent to.</param>
        /// <returns>A LightSequence to display.</returns>
        public LightSequence CreateLightSequenceFromChunk(Hardware hardwareType)
        {
            return CreateLightSequenceFromChunk((StreamingLightHardware)hardwareType);
        }

        /// <summary>
        /// Takes the next chunk of lights and converts it into a light sequence.
        /// This function also stores the last frame for use with the next sequence.
        /// </summary>
        /// <param name="hardwareType">The streaming light hardware type the lights will be sent to.</param>
        /// <returns>A LightSequence to display.</returns>
        public LightSequence CreateLightSequenceFromChunk(StreamingLightHardware hardwareType)
        {
            return CreateLightSequenceFromFrames(hardwareType, BlendChunk());
        }

        /// <summary>
        /// Creates a light sequence from the chunk that was previous returned. This will cause the previously
        /// chunk to be recalculated with the current layer information. This is used to make updates to the
        /// device faster.
        /// </summary>
        /// <param name="hardwareType">The hardware type the lights will be sent to.</param>
        /// <param name="frameCount">The number of frames to put into the chunk.</param>
        /// <returns>A light sequence to display.</returns>
        public LightSequence CreateLightSequenceFromPreviousChunk(Hardware hardwareType, int frameCount)
        {
            lock(frameBuffers)
            {
                // Roll the frame buffer back to its previous position.
                foreach(var pair in previousChunkPosition.Where(pair => frameBuffers.ContainsKey(pair.Key)))
                {
                    frameBuffers[pair.Key].SeekToFrame(pair.Value.Segment, pair.Value.SegmentLoop, pair.Value.Frame);
                }
            }
            return CreateLightSequenceFromFrames((StreamingLightHardware)hardwareType, BlendChunk(frameCount));
        }

        /// <summary>
        /// Creates a light sequence from a list of frames.
        /// </summary>
        /// <param name="hardwareType">The hardware device the light sequence is for.</param>
        /// <param name="frames">The collection of frames to add to the device.</param>
        /// <returns>The created light sequence.</returns>
        private LightSequence CreateLightSequenceFromFrames(StreamingLightHardware hardwareType, ICollection<Frame> frames)
        {
            var sequence = new LightSequence(hardwareType, LightSequenceVersion)
            {
                Name = "Generated chunk light sequence",
            };
            var segment = new Segment();

            if(frames != null)
            {
                // Add each frame into the segment, and blend it into the last frame.
                foreach(var frame in frames)
                {
                    segment.AddFrame(frame);
                }
            }

            sequence.AddSegment(segment);

            return sequence;
        }

        #endregion

        /// <summary>
        /// Returns a list of names generated from the LightSequences contained in the frameBuffers.
        /// </summary>
        public Dictionary<byte, string> GetLightSequenceNames()
        {
            var lightSequenceNames = new Dictionary<byte, string>();

            lock(frameBuffers)
            {
                foreach(var frameBufferEntry in frameBuffers)
                {
                    if(frameBufferEntry.Value.SequenceLayer?.LightSequence != null)
                    {
                        lightSequenceNames.Add(
                            frameBufferEntry.Key,
                            frameBufferEntry.Value.SequenceLayer.LightSequence.Name);
                    }
                }
            }
            return lightSequenceNames;
        }

        #region Layer Manipulation

        /// <summary>
        /// Adds a light sequence on a layer with a blend effect.
        /// </summary>
        /// <param name="sequence">The light sequence to play.</param>
        /// <param name="layerNumber">The layer number to play the sequence on.</param>
        /// <param name="effect">The blend effect to use for the layer.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="sequence"/> is null.
        /// Thrown if <paramref name="layerNumber"/> is not zero and <paramref name="effect"/> is null.
        /// </exception>
        /// <exception cref="InvalidLightSequenceException">
        /// Thrown if <paramref name="sequence"/> does not have at least one light segment or if any
        /// of the light segments have zero frames.
        /// Thrown if the light device specified by <paramref name="sequence"/> does not support 
        /// light blending and <paramref name="effect"/> is not null.
        /// </exception>
        public void AddLayer(LightSequence sequence, byte layerNumber, IBlendEffect effect)
        {
            if(sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            if(sequence.Segments.Count == 0)
            {
                throw new InvalidLightSequenceException("The light sequence must have at least one segment.");
            }

            if(sequence.Segments.Any(segment => segment.Frames.Count == 0))
            {
                throw new InvalidLightSequenceException("All segments in the light sequence must have at least one frame.");
            }

            var maxLayerCount = sequence.LightDevice == StreamingLightHardware.CatalinaReelBackLights
                ? ReelBackLightMaxLayerCount
                : StandardMaxLayerCount;

            if(LayerCount >= maxLayerCount)
            {
                throw new InvalidLayerCountException(
                    $"Cannot add more than {maxLayerCount} layers to the {sequence.LightDevice}.");
            }

            if(layerNumber != 0)
            {
                if(effect == null)
                {
                    throw new ArgumentNullException(nameof(effect), "A blending effect is required for layers 1+.");
                }
            }

            // Check for devices that do not support light blending
            if(!DoesDeviceSupportBlending(sequence.LightDevice) && effect != null)
            {
                throw new InvalidLightSequenceException(
                    $"Light blending for the following device is not supported: {sequence.LightDevice}");
            }

            lock(frameBuffers)
            {
                var layer = new Layer(effect, sequence);
                if(frameBuffers.ContainsKey(layerNumber))
                {
                    frameBuffers[layerNumber].SequenceLayer = layer;
                    SavePreviousPosition(layerNumber, frameBuffers[layerNumber].GetCurrentPosition());
                }
                else
                {
                    var frameBuffer = new FrameBuffer(layer);
                    frameBuffers.Add(layerNumber, frameBuffer);
                }
            }
        }

        /// <summary>
        /// Removes a layer from the blending process.
        /// </summary>
        /// <param name="layerNumber">The layer number to remove.</param>
        public void RemoveLayer(byte layerNumber)
        {
            lock(frameBuffers)
            {
                if(!frameBuffers.ContainsKey(layerNumber))
                {
                    return;
                }

                frameBuffers.Remove(layerNumber);
                previousChunkPosition.Remove(layerNumber);
            }
        }

        /// <summary>
        /// Clears all the layers from the blend.
        /// </summary>
        public void Clear()
        {
            lock(frameBuffers)
            {
                frameBuffers.Clear();
                previousChunkPosition.Clear();
            }
        }

        /// <summary>
        /// Clears all layers from the blend except layer zero.
        /// </summary>
        public void ClearAllLayersButZero()
        {
            lock(frameBuffers)
            {
                var bufferList = frameBuffers.Keys.Where(key => key != 0).ToList();
                bufferList.ForEach(key => frameBuffers.Remove(key));
            }
        }

        /// <summary>
        /// Seeks the playback of a layer to a certain position.
        /// </summary>
        /// <param name="layerNumber">The layer number to change.</param>
        /// <param name="seekSegment">The segment number to seek to.</param>
        /// <param name="seekSegmentLoop">The loop count of the segment to seek to.</param>
        /// <param name="seekFrame">The frame number in the segment to seek to.</param>
        public void SeekLayer(byte layerNumber, int seekSegment, int seekSegmentLoop, int seekFrame)
        {
            lock(frameBuffers)
            {
                if(frameBuffers.ContainsKey(layerNumber))
                {
                    frameBuffers[layerNumber].SeekToFrame(seekSegment, seekSegmentLoop, seekFrame);
                }
            }
        }

        #endregion

        /// <summary>
        /// Determines if the specified light device supports light blending.
        /// </summary>
        /// <param name="hardware"><see cref="StreamingLightHardware"/> device to check.</param>
        /// <returns>
        /// <see langword="true"/> if light blending is supported for <paramref name="hardware"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool DoesDeviceSupportBlending(StreamingLightHardware hardware)
        {
            return !InvalidBlendingDevices.Contains(hardware);
        }

        /// <summary>
        /// Determines if all the layers have finished playing or not.
        /// </summary>
        /// <returns>True if all the layers have finished playing.</returns>
        public bool GetIsAtEnd()
        {
            bool atEnd;
            lock(frameBuffers)
            {
                atEnd = frameBuffers.Values.All(buffer => buffer.AtEnd);
            }

            return atEnd;
        }

        /// <summary>
        /// Blends the different layers together to create a set of blended frames.
        /// </summary>
        /// <returns>A list of blended frames.</returns>
        internal List<Frame> BlendChunk()
        {
            return BlendChunk(FrameBuffer.MaxChunkSize);
        }

        /// <summary>
        /// Blends the different layers together to create a set of blended frames.
        /// </summary>
        /// <param name="numberOfFrames">The number of frames to include.</param>
        /// <returns>A list of blended frames.</returns>
        private List<Frame> BlendChunk(int numberOfFrames)
        {
            if(frameBuffers.Count == 0 || LayerCount == 0)
            {
                return null;
            }

            lock(frameBuffers)
            {
                using(var bufferEnumerator = frameBuffers.Keys.GetEnumerator())
                {
                    // The enumerator needs to be advanced initially to get to the first element.
                    bool atEndOfBuffers;
                    bufferEnumerator.MoveNext();

                    List<Frame> frameChunk;
                    do
                    {
                        SavePreviousPosition(bufferEnumerator.Current,
                            frameBuffers[bufferEnumerator.Current].GetCurrentPosition());
                        frameChunk = frameBuffers[bufferEnumerator.Current].ReadChunk(numberOfFrames);
                        atEndOfBuffers = !bufferEnumerator.MoveNext();
                    } while(!atEndOfBuffers && frameChunk.Count == 0);

                    if(frameChunk.Count == 0)
                    {
                        return null;
                    }

                    var frameChunkColorFrameData =
                        new List<Color>(Enumerable.Repeat(Color.Black, frameChunk[0].ColorFrameData.Count));

                    while(!atEndOfBuffers)
                    {
                        var key = bufferEnumerator.Current;
                        SavePreviousPosition(key, frameBuffers[key].GetCurrentPosition());
                        var nextChunk = frameBuffers[key].ReadChunk(numberOfFrames);
                        if(nextChunk.Count == 0)
                        {
                            // If this frame buffer has reached the end, then there isn't anything to blend.
                            atEndOfBuffers = !bufferEnumerator.MoveNext();
                            continue;
                        }

                        var nextChunkColorFrameData =
                            new List<Color>(Enumerable.Repeat(Color.Black, nextChunk[0].ColorFrameData.Count));

                        var effect = frameBuffers[key].SequenceLayer.BlendEffect;

                        BlendFrames(nextChunk, frameChunk, frameChunkColorFrameData, nextChunkColorFrameData, effect);

                        atEndOfBuffers = !bufferEnumerator.MoveNext();
                    }

                    return frameChunk;
                }
            }
        }

        /// <summary>
        /// Blends the frames in two chunks together.
        /// </summary>
        /// <param name="nextChunk">The chunk to blend into <paramref name="frameChunk"/>.</param>
        /// <param name="frameChunk">The chunk to blend into.</param>
        /// <param name="frameChunkColorFrameData">The current color state of all the LEDs in the frame for <paramref name="frameChunk"/>.</param>
        /// <param name="nextChunkColorFrameData">The current color state of all the LEDs in the frame for <paramref name="nextChunk"/>.</param>
        /// <param name="effect">The blend effect to use.</param>
        private static void BlendFrames(IList<Frame> nextChunk, IList<Frame> frameChunk, List<Color> frameChunkColorFrameData,
            List<Color> nextChunkColorFrameData, IBlendEffect effect)
        {
            for(var frameIndex = 0; frameIndex < nextChunk.Count && frameIndex < frameChunk.Count; ++frameIndex)
            {
                if(nextChunk[frameIndex].ColorFrameData == null)
                {
                    continue;
                }

                if(frameChunk[frameIndex].ColorFrameData == null)
                {
                    frameChunk[frameIndex] = nextChunk[frameIndex];
                    continue;
                }

                for(var index = 0; index < frameChunk[frameIndex].ColorFrameData.Count; ++index)
                {
                    var color = frameChunk[frameIndex].ColorFrameData[index];
                    if(!color.IsLinkedColor)
                    {
                        frameChunkColorFrameData[index] = color;
                    }

                    color = nextChunk[frameIndex].ColorFrameData[index];
                    if(!color.IsLinkedColor)
                    {
                        nextChunkColorFrameData[index] = color;
                    }
                }

                var blendedFrame = effect.Blend(frameChunk[frameIndex], nextChunk[frameIndex],
                    frameChunkColorFrameData, nextChunkColorFrameData, false);

                frameChunk[frameIndex] = blendedFrame;
            }
        }

        /// <summary>
        /// Saves the passed in position into the previous chunk position dictionary.
        /// </summary>
        /// <param name="layerNumber">The layer number the position is for.</param>
        /// <param name="position">The light sequence position to save.</param>
        private void SavePreviousPosition(byte layerNumber, LightSequencePosition position)
        {
            if(previousChunkPosition.ContainsKey(layerNumber))
            {
                previousChunkPosition[layerNumber] = position;
            }
            else
            {
                previousChunkPosition.Add(layerNumber, position);
            }
        }
    }
}
