// -----------------------------------------------------------------------
// <copyright file = "LightSequenceOptimizer.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Optimizes the content of light sequences.
    /// </summary>
    internal class LightSequenceOptimizer
    {
        /// <summary>
        /// Run all optimizations on the light sequence.
        /// </summary>
        /// <param name="sequence">The light sequence to optimize.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="sequence"/> is null.
        /// </exception>
        public void OptimizeLightSequence(ILightSequence sequence)
        {
            if(sequence == null)
            {
                throw new ArgumentNullException(nameof(sequence));
            }

            var frameColors = new List<Color>();
            foreach(var frame in sequence.Segments.SelectMany(segment => segment))
            {
                if(frameColors.Count == 0)
                {
                    frameColors.AddRange(frame.ColorFrameData);
                    continue;
                }

                List<Color> frameColorsCopy = null;

                for(var colorIndex = 0; colorIndex < frame.ColorFrameData.Count; colorIndex++)
                {
                    if(!frame.ColorFrameData[colorIndex].IsLinkedColor)
                    {
                        if(frameColors[colorIndex] == frame.ColorFrameData[colorIndex])
                        {
                            // This could be a linked color.
                            if(frameColorsCopy == null)
                            {
                                frameColorsCopy = new List<Color>(frame.ColorFrameData);
                            }

                            frameColorsCopy[colorIndex] = Color.LinkedColor;
                        }
                        else
                        {
                            frameColors[colorIndex] = frame.ColorFrameData[colorIndex];
                        }
                    }
                }

                if(frameColorsCopy != null)
                {
                    frame.UpdateColors(frameColorsCopy);
                }
            }
        }
    }
}