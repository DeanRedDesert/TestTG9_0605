//-----------------------------------------------------------------------
// <copyright file = "TransparentColorBlend.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.ChromaBlend
{
    using System;
    using System.Collections.Generic;
    using Streaming;

    /// <summary>
    /// Does a color blend where one color is the transparent color and anything on a
    /// lower level is passed through the transparency.
    /// </summary>
    public class TransparentColorBlend : IBlendEffect
    {
        private readonly Color transparentColor;

        /// <summary>
        /// Construct a new transparent color blend given the
        /// transparent color.
        /// </summary>
        /// <param name="transparentColor">The foreground color to treat as transparent.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="transparentColor"/> is empty.
        /// </exception>
        public TransparentColorBlend(Color transparentColor)
        {
            if(transparentColor.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(transparentColor));
            }

            this.transparentColor = transparentColor;
        }

        /// <inheritdoc />
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if any of the parameters are null.
        /// </exception>
        /// <exception cref="InvalidLightSequenceException">
        /// Thrown if the background frame and the foreground frame do not have the same number of colors.
        /// </exception>
        public Frame Blend(Frame background, Frame foreground,
            List<Color> backgroundLedState, List<Color> foregroundLedState, bool foregroundLooped)
        {
            if(background == null)
            {
                throw new ArgumentNullException(nameof(background));
            }

            if(foreground == null)
            {
                throw new ArgumentNullException(nameof(foreground));
            }

            if(backgroundLedState == null)
            {
                throw new ArgumentNullException(nameof(backgroundLedState));
            }

            if(foregroundLedState == null)
            {
                throw new ArgumentNullException(nameof(foregroundLedState));
            }

            var backgroundColors = background.ColorFrameData;
            var foregroundColors = foreground.ColorFrameData;

            if(backgroundColors.Count != foregroundColors.Count)
            {
                throw new InvalidLightSequenceException(
                    $"The background frame has {backgroundColors.Count} colors and the foreground has {foregroundColors.Count}. They must be equal.");
            }

            var maxLedIndex = foregroundColors.Count;
            var newFrameColors = new List<Color>(maxLedIndex);
            
            for(var index = 0; index < maxLedIndex; index++)
            {
                if(foregroundLedState[index] == transparentColor)
                {
                    // Use the led state instead of the background to handle the cases of linked colors.
                    newFrameColors.Add(foregroundLooped ? backgroundLedState[index] : backgroundColors[index]);
                }
                else
                {
                    newFrameColors.Add(foregroundColors[index]);
                }

            }

            return new Frame(newFrameColors)
            {
                DisplayTime = background.DisplayTime,
                TransitionTime = background.TransitionTime,
                TransitionType = background.TransitionType,
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Blending with transparent color {transparentColor}.";
        }
    }
}
