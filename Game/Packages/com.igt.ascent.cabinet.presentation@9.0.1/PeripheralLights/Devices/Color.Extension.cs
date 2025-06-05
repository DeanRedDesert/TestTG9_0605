//-----------------------------------------------------------------------
// <copyright file = "Color.Extension.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using Communication.Cabinet;

    /// <summary>
    /// Extensions to the color struct.
    /// </summary>
    public static class ColorExtension
    {
        /// <summary>
        /// Creates the RGB 6 representation of the current color in the RGBAX format 2.2.2.0.0.
        /// </summary>
        /// <param name="color">The color to convert from.</param>
        /// <returns>The RGB 6 color.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the color is uninitialized.
        /// </exception>
        public static Rgb6 GetRgb6(this Color color)
        {
            if(color.IsEmpty)
            {
                throw new InvalidOperationException("The color is uninitialized.");
            }

            // Perform a color space conversion
            var alphaRatio = color.A / 256.0;
            var newRed = Convert.ToByte(Math.Floor(color.R * alphaRatio) * 3 / 255);
            var newGreen = Convert.ToByte(Math.Floor(color.G * alphaRatio) * 3 / 255);
            var newBlue = Convert.ToByte(Math.Floor(color.B * alphaRatio) * 3 / 255);

            return new Rgb6(newRed, newGreen, newBlue);
        }

        /// <summary>
        /// Creates the RGB 16 representation of the current color in the RGBAX format 5.6.5.0.0.
        /// </summary>
        /// <param name="color">The color to convert from.</param>
        /// <returns>The RGB 16 color.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the color is uninitialized.
        /// </exception>
        public static Rgb16 GetRgb16(this Color color)
        {
            if(color.IsEmpty)
            {
                throw new InvalidOperationException("The color is uninitialized.");
            }

            // Perform a color space conversion
            var alphaRatio = color.A / 256.0;
            var newRed = Convert.ToByte(Math.Floor(color.R * alphaRatio) * 31 / 255);
            var newGreen = Convert.ToByte(Math.Floor(color.G * alphaRatio) * 63 / 255);
            var newBlue = Convert.ToByte(Math.Floor(color.B * alphaRatio) * 31 / 255);

            return new Rgb16(newRed, newGreen, newBlue);
        }

        /// <summary>
        /// Creates the RGB 15 representation of the current color in the RGBAX format 5.5.5.0.0.
        /// </summary>
        /// <param name="color">The color to convert from.</param>
        /// <returns>The RGB 15 color.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the color is uninitialized.
        /// </exception>
        public static Rgb15 GetRgb15(this Color color)
        {
            if(color.IsEmpty)
            {
                throw new InvalidOperationException("The color is uninitialized.");
            }

            // Perform a color space conversion.
            var alphaRatio = color.A / 256.0;
            var newRed = Convert.ToByte(Math.Floor(color.R * alphaRatio) * 31 / 255);
            var newGreen = Convert.ToByte(Math.Floor(color.G * alphaRatio) * 31 / 255);
            var newBlue = Convert.ToByte(Math.Floor(color.B * alphaRatio) * 31 / 255);

            return new Rgb15(newRed, newGreen, newBlue);
        }
    }
}
