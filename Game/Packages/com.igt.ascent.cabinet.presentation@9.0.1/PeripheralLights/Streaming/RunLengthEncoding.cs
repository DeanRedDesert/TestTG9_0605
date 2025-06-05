//-----------------------------------------------------------------------
// <copyright file = "RunLengthEncoding.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------


namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    ///     Implements the run length encoding for the IGT USB Feature 121.
    /// </summary>
    internal static class RunLengthEncoding
    {
        private const byte LinkedColorBitMask = 0x80;
        private const byte MaximumNumberOfColors = 128;

        /// <summary>
        ///     Encode a list of colors into a run length encoded byte list.
        /// </summary>
        /// <param name="input">The colors to encode.</param>
        /// <returns>The run length encoded data.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown if <paramref name="input" /> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="input" /> is empty.
        /// </exception>
        public static List<byte> Encode(IList<Color> input)
        {
            if(input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if(input.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(input),
                    "There must be at least one color in the input list.");
            }

            var optimizedData = OptimizeColorData(input);
            var output = new List<byte>();
            var colorQueue = new Queue<Color>();

            foreach(var colorData in optimizedData)
            {
                if(colorData.Count > 1)
                {
                    if(colorQueue.Count > 0)
                    {
                        // If there are colors in the queue they need to be written out first.
                        // The GetBytesFromColor method will empty the queue out.
                        output.AddRange(GetBytesFromColor(colorQueue));
                    }

                    // RLE encode this color since it repeats multiple times.
                    output.AddRange(GetBytesFromColor(colorData.Color, colorData.Count));
                }
                else
                {
                    if(colorQueue.Count == MaximumNumberOfColors)
                    {
                        // Flush the queue out since 128 is the maximum allowed colors.
                        // The GetBytesFromColor method will empty the queue out.
                        output.AddRange(GetBytesFromColor(colorQueue));
                    }

                    colorQueue.Enqueue(colorData.Color);
                }
            }

            // Make sure any remaining colors are encoded.
            if(colorQueue.Count > 0)
            {
                output.AddRange(GetBytesFromColor(colorQueue));
            }

            return output;
        }

        /// <summary>
        ///     Decodes a run length encoded byte stream into their list of colors.
        /// </summary>
        /// <param name="input">The run length encoded data.</param>
        /// <returns>The list of colors.</returns>
        /// <exception cref="System.ArgumentNullException">
        ///     Thrown if <paramref name="input" /> is null.
        /// </exception>
        /// <exception cref="RunLengthDecodingException">
        ///     Thrown if the data provided in <paramref name="input" />
        ///     is invalid.
        /// </exception>
        public static List<Color> Decode(List<byte> input)
        {
            if(input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var output = new List<Color>();

            for(var index = 0; index < input.Count;)
            {
                var rleEnabled = (input[index] & 0x80) == 0x80;
                // + 1 since Feature 121 spec says the encoded data is number of colors - 1
                var numberOfColors = (input[index] & 0x7F) + 1;

                if(rleEnabled)
                {
                    if(index + 2 >= input.Count)
                    {
                        throw new RunLengthDecodingException(
                            $"The input data is less than the amount required. Expected more than {index + 2} bytes but was {input.Count} bytes.");
                    }

                    var color = GetColorFromBytes(input[index + 2], input[index + 1]);

                    while(numberOfColors > 0)
                    {
                        output.Add(color);
                        numberOfColors--;
                    }

                    index += 3;
                }
                else
                {
                    if(index + numberOfColors * 2 >= input.Count)
                    {
                        throw new RunLengthDecodingException(
                            $"The input data is less than the amount required. Expected more than {index + numberOfColors * 2} bytes but was {input.Count} bytes.");
                    }

                    // Move past the header byte.
                    index++;
                    while(numberOfColors > 0)
                    {
                        var color = GetColorFromBytes(input[index + 1], input[index]);
                        output.Add(color);
                        numberOfColors--;
                        index += 2;
                    }
                }
            }

            return output;
        }

        /// <summary>
        ///     Creates a byte array representing the color in run length encoded RGB15.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <param name="count">The number of times this color is repeated.</param>
        /// <returns>The run length encoded form of the passed in color.</returns>
        private static byte[] GetBytesFromColor(Color color, byte count)
        {
            // -1 from the number of colors per the Feature 121 spec.
            var header = (byte)(0x80 | ((count - 1) & 0x7F));
            var output = new byte[3];
            output[0] = header;

            ColorSpaceConverter.ConvertColorToBytes(color, out var lower, out var upper);

            output[1] = lower;
            output[2] = upper;

            return output;
        }

        /// <summary>
        ///     Creates a byte array representing the different colors in the queue.
        /// </summary>
        /// <param name="colors">The colors to convert.</param>
        /// <returns>The non-RLE color data.</returns>
        private static byte[] GetBytesFromColor(Queue<Color> colors)
        {
            // -1 from the number of colors per the Feature 121 spec.
            var header = (byte)((colors.Count - 1) & 0x7F);
            // The output array size is 1 byte for the header and
            // then each color is two bytes.
            var output = new byte[1 + colors.Count * 2];
            output[0] = header;

            var index = 1;
            while(colors.Count > 0)
            {
                ColorSpaceConverter.ConvertColorToBytes(colors.Dequeue(), out var lower, out var upper);
                output[index++] = lower;
                output[index++] = upper;
            }

            return output;
        }

        /// <summary>
        ///     Converts a 16-bit RGB15 color to a RGB32 color.
        /// </summary>
        /// <param name="upper">The upper color byte.</param>
        /// <param name="lower">The lower color byte.</param>
        /// <returns>The RGB32 color represented by the bytes.</returns>
        private static Color GetColorFromBytes(byte upper, byte lower)
        {
            Color newColor;

            if((upper & LinkedColorBitMask) != LinkedColorBitMask)
            {
                var baseRed = (upper & 0x7C) >> 2;
                var baseGreen = ((upper & 0x03) << 3) | ((lower & 0xE0) >> 5);
                var baseBlue = lower & 0x1F;

                var red = Convert.ToByte(baseRed * 255 / 31);
                var green = Convert.ToByte(baseGreen * 255 / 31);
                var blue = Convert.ToByte(baseBlue * 255 / 31);

                newColor = Color.FromRgb(red, green, blue);
            }
            else
            {
                newColor = Color.LinkedColor;
            }

            return newColor;
        }

        /// <summary>
        ///     Optimize the color data for run length encoding. Adjacent colors that repeat are
        ///     condensed into a single entry.
        /// </summary>
        /// <param name="colors">The list of colors in the frame.</param>
        /// <returns>The optimized list of colors.</returns>
        private static List<ColorData> OptimizeColorData(IList<Color> colors)
        {
            var data = new List<ColorData>();
            byte count = 1;
            var lastColor = colors[0];

            foreach(var currentColor in colors.Skip(1))
            {
                // The 128 check is there because the count is only 7-bits.
                if(currentColor == lastColor && count < MaximumNumberOfColors)
                {
                    count++;
                }
                else
                {
                    data.Add(new ColorData(lastColor, count));
                    count = 1;
                    lastColor = currentColor;
                }
            }

            data.Add(new ColorData(lastColor, count));

            return data;
        }

        /// <summary>
        ///     The color data information struct.
        /// </summary>
        private struct ColorData
        {
            /// <summary>
            ///     Construct a new instance given a color and a count.
            /// </summary>
            /// <param name="color">The color.</param>
            /// <param name="count">The number of times this color occurs.</param>
            public ColorData(Color color, byte count)
                : this()
            {
                Color = color;
                Count = count;
            }

            /// <summary>
            ///     Gets the color.
            /// </summary>
            public Color Color { get; }

            /// <summary>
            ///     Gets the number of times the color occurs.
            /// </summary>
            public byte Count { get; }
        }
    }
}