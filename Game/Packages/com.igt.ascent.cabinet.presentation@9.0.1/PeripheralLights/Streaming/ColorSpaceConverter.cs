// -----------------------------------------------------------------------
// <copyright file = "ColorSpaceConverter.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;

    /// <summary>
    /// Class used for converting a color to RGB555. 
    /// </summary>
    public static class ColorSpaceConverter
    {
        /// <summary>
        /// Convert streaming light color to two bytes.
        /// </summary>
        /// <param name="color">Color to convert.</param>
        /// <param name="lowerByte">Lower byte.</param>
        /// <param name="upperByte">Upper byte.</param>
        public static void ConvertColorToBytes(Color color, out byte lowerByte, out byte upperByte)
        {
            if(!color.IsLinkedColor)
            {
                // Perform a color space conversion.
                var alphaRatio = color.A / 255.0;
                var newRed = Convert.ToByte(Math.Floor(color.R * alphaRatio) * 31 / 255);
                var newGreen = Convert.ToByte(Math.Floor(color.G * alphaRatio) * 31 / 255);
                var newBlue = Convert.ToByte(Math.Floor(color.B * alphaRatio) * 31 / 255);

                var packedColor = (ushort)((newRed & 0x1F) << 10);
                packedColor |= (ushort)((newGreen & 0x1F) << 5);
                packedColor |= (ushort)(newBlue & 0x1F);

                lowerByte = (byte)(packedColor & 0xFF);
                upperByte = (byte)((packedColor & 0xFF00) >> 8);
            }
            else
            {
                // The inherited/transparent color has a specific byte sequence.
                lowerByte = 0;
                upperByte = 0x80;
            }
        }

        /// <summary>
        /// Convert a streaming light color to a packed color. 
        /// </summary>
        /// <param name="color">Color to convert.</param>
        /// <returns>Converted packed color.</returns>
        public static ushort ConvertColorToPackedColor(Color color)
        {
            ushort packedColor;

            if(!color.IsLinkedColor)
            {
                // Perform a color space conversion.
                var alphaRatio = color.A / 255.0;
                var newRed = Convert.ToByte(Math.Floor(color.R * alphaRatio) * 31 / 255);
                var newGreen = Convert.ToByte(Math.Floor(color.G * alphaRatio) * 31 / 255);
                var newBlue = Convert.ToByte(Math.Floor(color.B * alphaRatio) * 31 / 255);

                packedColor = (ushort)((newRed & 0x1F) << 10);
                packedColor |= (ushort)((newGreen & 0x1F) << 5);
                packedColor |= (ushort)(newBlue & 0x1F);

            }
            else
            {
                // The inherited/transparent color has a specific byte sequence.
                packedColor = 0x8000;
            }

            return packedColor;


        }
    }
}