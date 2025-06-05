//-----------------------------------------------------------------------
// <copyright file = "TransparentColorBlendWithReplacement.cs" company = "IGT">
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
    /// lower level is passed through the transparency. If the search color is found
    /// on the foreground it is replaced with the specified replacement color.
    /// </summary>
    public class TransparentColorBlendWithReplacement : IBlendEffect
    {
        private readonly Color transparentColor;
        private readonly Color searchColor;
        private Color replacementColor;

        /// <summary>
        /// Construct a new transparent color blend given the
        /// transparent color and the search color.
        /// </summary>
        /// <param name="transparentColor">The foregound color to treat as transparent.</param>
        /// <param name="searchColor">The color to look for in the foreground to replace with the replacement color.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="transparentColor"/> or <paramref name="searchColor"/> is empty.
        /// </exception>
        public TransparentColorBlendWithReplacement(Color transparentColor, Color searchColor)
        {
            if(transparentColor.IsEmpty)
            {
                throw new ArgumentException("The transparent color cannot be empty.", nameof(transparentColor));
            }

            if(searchColor.IsEmpty)
            {
                throw new ArgumentException("The search color cannot be empty.", nameof(searchColor));
            }

            this.transparentColor = transparentColor;
            this.searchColor = searchColor;
            ReplacementColor = Color.HotPink;
        }

        /// <summary>
        /// Gets and sets the color to replace the search color with in the foreground.
        /// </summary>
        public Color ReplacementColor
        {
            get => replacementColor;
            set
            {
                if(value.IsEmpty)
                {
                    throw new ArgumentException("The replacement color cannot be empty.", nameof(value));
                }
                replacementColor = value;
            }
        }

        #region IBlendEffect Members

        /// <inheritdoc />
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if any of the parameters are null.
        /// </exception>
        /// <exception cref="InvalidLightSequenceException">
        /// Thrown if the background frame and the foreground frame do not have the same number of colors.
        /// </exception>
        public Frame Blend(Frame background, Frame foreground, List<Color> backgroundLedState, List<Color> foregroundLedState, bool foregroundLooped)
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
                else if(foregroundLedState[index] == searchColor)
                {
                    newFrameColors.Add(ReplacementColor);
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

        #endregion

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"Blending with transparent color {transparentColor}. Replacing color {searchColor} with {ReplacementColor}.";
        }
    }
}
