// -----------------------------------------------------------------------
// <copyright file = "KeyFrameProcessor.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Processes a light sequence and adds key frames into it.
    /// </summary>
    internal class KeyFrameProcessor
    {
        private const int KeyFrameInterval = 100;

        /// <summary>
        /// Processes the light sequence and creates key frames in the sequence.
        /// </summary>
        /// <param name="sequence">The light sequence to process.</param>
        public void CreateKeyFrames(ILightSequence sequence)
        {
            if(sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            var frameCounter = 0;
            var frameColors = new List<Color>();
            foreach(var segment in sequence.Segments)
            {
                segment.ClearKeyFrameTable();
                for(var index = 0; index < segment.Frames.Count; index++)
                {
                    var frame = segment.Frames[index];
                    if(frameColors.Count == 0)
                    {
                        frameColors.AddRange(frame.ColorFrameData);
                    }
                    else
                    {
                        for(var colorIndex = 0; colorIndex < frameColors.Count; colorIndex++)
                        {
                            if(!frame.ColorFrameData[colorIndex].IsLinkedColor)
                            {
                                frameColors[colorIndex] = frame.ColorFrameData[colorIndex];
                            }
                        }
                    }

                    if(frameCounter % KeyFrameInterval == 0 ||
                        // If the segment loops, make the first frame of the segment a key frame regardless of
                        // where the frame count is.
                        (index == 0 && segment.Loop > 1))
                    {
                        // Replace the frame colors with the tracked color data. This eliminates any linked colors in the frame.
                        segment.Frames[index].UpdateColors(frameColors);

                        segment.MarkFrameAsKeyFrame(index);
                    }

                    frameCounter++;
                }
            }
        }
    }
}