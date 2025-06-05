//-----------------------------------------------------------------------
// <copyright file = "TwilightZone3DLight.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;

    /// <summary>
    /// The starting position of a brush or sequence.
    /// </summary>
    public enum TwilightZone3DLightStartPosition : byte
        {
            /// <summary>
            /// Starts from the right.
            /// </summary>
            Right = 0,

            /// <summary>
            /// Starts from the top right.
            /// </summary>
            TopRight,

            /// <summary>
            /// Starts from the top.
            /// </summary>
            Top,

            /// <summary>
            /// Starts from the top left.
            /// </summary>
            TopLeft,

            /// <summary>
            /// Starts from the left.
            /// </summary>
            Left,

            /// <summary>
            /// Starts from the bottom left.
            /// </summary>
            BottomLeft,

            /// <summary>
            /// Starts from the bottom.
            /// </summary>
            Bottom,

            /// <summary>
            /// Starts from the bottom right.
            /// </summary>
            BottomRight,
        }

    /// <summary>
    /// Represents a generic piece of twilight zone 3D light hardware.
    /// </summary>
    public abstract class TwilightZone3DLight : UsbIndividualLightControl
    {
        #region Light Data

        /// <summary>
        /// The maximum number of colors supported for this device.
        /// </summary>
        protected abstract short MaxNumberOfColorsSupported { get; }

        #endregion Light Data

        #region Hardware Sequence Ids

        /// <summary>
        /// Sets all lights to the 8-bit brightness value specified in the second parameter byte.
        /// </summary>
        private const uint SetBrightnessSequenceId = 0x0040;

        /// <summary>
        /// Sets all lights to the RGB 16 value specified in the two parameter bytes.
        /// </summary>
        private const uint SetColorSequenceId = 0x0041;

        /// <summary>
        /// Flashes between the state of the lights when the sequence starts, using a specific color.
        /// </summary>
        private const uint FlashColorSequenceId = 0x00A0;

        /// <summary>
        /// Simulates a paintbrush moving around the circle, "painting" the lights a new color as it goes. After painting
        /// the entire circle, the process repeats with a new color each time.
        /// </summary>
        private const uint PaintPlusSequenceSequenceId = 0x00A2;

        /// <summary>
        /// Flashes between colors.
        /// </summary>
        private const uint FlashBetweenColorsSequenceId = 0x00A3;

        /// <summary>
        /// Runs a pattern with stack pieces made from multiple lights that shift in on each half of the light group. When the
        /// stacked pieces have filled the light group restart the process with a new color.
        /// </summary>
        private const uint StackHalvesSequenceId = 0x00A8;

        /// <summary>
        /// Changes all lights to a single color.
        /// </summary>
        private const uint RandomColorSequenceId = 0x00AA;

        #endregion Hardware Sequence Ids

        #region Constructor

        /// <summary>
        /// Constructs a TwilightZone3D light device.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal TwilightZone3DLight(string featureName, LightFeatureDescription featureDescription,
            IPeripheralLights peripheralLights) : base(featureName, featureDescription, peripheralLights)
        {
        }

        #endregion Constructor

        #region Validation Methods

        /// <summary>
        /// Validates the given <paramref name="colors"/>.
        /// </summary>
        /// <param name="colors">The color list.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the number of colors in <paramref name="colors"/> is not in an acceptable range.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if any color in the color list is empty.
        /// </exception>
        protected void ValidateColorList(IList<Color> colors)
        {
            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(!colors.Any() || colors.Count > MaxNumberOfColorsSupported)
            {
                throw new ArgumentOutOfRangeException(nameof(colors),
                    $"The number of colors provided {colors.Count} is out of the " +
                    $"acceptable range. There has to be at least 1 color but less than {MaxNumberOfColorsSupported} colors.");
            }

            if(colors.Any(color => color.IsEmpty))
            {
                throw new ArgumentException("The color cannot be empty.");
            }
        }

        /// <summary>
        /// Validates the number of brushes supported by this device.
        /// </summary>
        /// <param name="brushes">A list of <see cref="Paintbrush"/></param> objects.
        protected abstract void ValidateBrushes(IList<Paintbrush> brushes);

        #endregion Validation Methods

        #region UsbPeripheralLight

        /// <inheritdoc />
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="color"/> is empty.
        /// </exception>
        public override void SetColor(byte groupId, Color color)
        {
            ValidateGroupId(groupId);

            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(color));
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(color.GetRgb16().PackedColor);

            StartSequence(groupId, SetColorSequenceId, TransitionMode.Immediate, parameters);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Iterates over a list of colors and adds their RGB16 representation to the parameter list.
        /// </summary>
        /// <param name="list">The list of bytes to add to.</param>
        /// <param name="colors">The list of colors to add.</param>
        protected void ConvertPackedColorsToBytes(IList<byte> list, IList<Color> colors)
        {
            foreach(var color in colors)
            {
                list.Add(color.GetRgb16().PackedColor);
            }
        }

        /// <summary>
        /// Sets the brightness of all the lights in the specified group.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="brightness">The desired brightness.</param>
        /// <remarks>
        /// Overriden to run the set brightness sequence command on the twilight zone 3D light hardware.
        /// </remarks>
        public override void SetBrightness(byte groupId, byte brightness)
        {
            ValidateGroupId(groupId);

            // A byte array of the sequence to apply starting with 0 as the reserved field.
            // The brightness to be set is specified in the second parameter bytes.
            var parameters = new List<byte> {0, brightness};

            StartSequence(groupId, SetBrightnessSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Flashes between the state of lights when the sequence starts, using a specified color.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="numberOfFlashes">
        /// The number of times to flash before ending the sequence. A value of 0 means flashes indefinitely.
        /// </param>
        /// <param name="duration">The duration of a single flash, in 50 millisecond increments. A duration of 1 means
        /// that the light is off for 25 milliseconds and on for 25 milliseonds.</param>
        /// <param name="color">Color to display for half of the flash duration.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="color"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero.
        /// </exception>
        protected void FlashColor(byte groupId, byte numberOfFlashes, byte duration, Color color)
        {
            ValidateGroupId(groupId);

            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(color));
            }

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The flash duration cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>
            {
                numberOfFlashes,
                duration,
            };

            parameters.Add(color.GetRgb16().PackedColor);

            StartSequence(groupId, FlashColorSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Runs a paint plus sequence on the device.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="duration">
        /// The number of milliseconds between light updates. This sets how fast the lights are "painted".
        /// </param>
        /// <param name="brushes">The number of brushes used to paint the lights at the same time.</param>
        /// <param name="colors">The colors used to paint the lights.</param>
        /// <param name="alternateColors">Boolean flag to indicate if a different color is used for each brush.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="brushes"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="brushes"/> contains an empty list.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero or if the number of <paramref name="brushes"/>
        /// exceeds the maximum number of brushes supported.
        /// </exception>
        protected void RunPaintPlusSequence(byte groupId, ushort duration, IList<Paintbrush> brushes, IList<Color> colors,
            bool alternateColors)
        {
            ValidateGroupId(groupId);

            ValidateColorList(colors);

            ValidateBrushes(brushes);

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(duration);

            parameters.Add(Convert.ToByte(brushes.Count - 1));

            parameters.Add(Convert.ToByte(colors.Count));

            parameters.Add(Convert.ToByte(alternateColors));

            foreach(var brush in brushes)
            {
                var brushData = Convert.ToByte(brush.StartPosition);
                if(brush.Direction == LightDirection.Clockwise)
                {
                    brushData |= 8;
                }
                if(brush.AlternateDirection)
                {
                    brushData |= 16;
                }
                parameters.Add(brushData);
            }

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(groupId, PaintPlusSequenceSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Flashes between colors.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="duration">Time to display each color in 25 millisecond increments.</param>
        /// <param name="looping">Flag that indicates whether or loop flashing or not.</param>
        /// <param name="colors">The colors to display.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero.
        /// </exception>
        protected void FlashBetweenColors(byte groupId, byte duration, bool looping, IList<Color> colors)
        {
            ValidateGroupId(groupId);

            ValidateColorList(colors);

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            if(looping)
            {
                duration += 128;
            }

            // Add hold time.
            parameters.Add(duration);

            // Add number of colors.
            parameters.Add(Convert.ToByte(colors.Count));

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(groupId, FlashBetweenColorsSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Runs patterns with stack pieces made from multiple lights that shift in on each half of the light group.
        /// When the stacked pieces have filled the light group restart the process with a new color.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="entryPosition">
        /// The position on the wheel where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="dropSpeed">
        /// Milliseconds between each advancement of a piece from the entry position toward its resting position.
        /// </param>
        /// <param name="pieceSizeRatio">
        /// Ratio of half of the group's lights to the length of each piece. This can also be thought of as the number of
        /// pieces required to fill half of the group's lights.
        /// </param>
        /// <param name="gapSizeRatio">
        /// Ratio of half of the group's lights to the length of the gap between pieces. 0 = introduce a new piece as
        /// soon as the previous piece has come to rest.
        /// </param>
        /// <param name="colors">
        /// The list of colors to transition between.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the piece size ration is zero.
        /// </exception>
        protected void RunStackHalves(byte groupId, TwilightZone3DLightStartPosition entryPosition, ushort dropSpeed, byte pieceSizeRatio,
            byte gapSizeRatio, IList<Color> colors)
        {
            ValidateGroupId(groupId);

            ValidateColorList(colors);

            if(pieceSizeRatio == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pieceSizeRatio), "The piece size ratio cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>
            {
                0, // Add a reserved byte.
                Convert.ToByte(entryPosition) // Add entry position.
            };

            // Add drop speed.
            parameters.Add(dropSpeed);

            // Add piece size ratio.
            parameters.Add(pieceSizeRatio);

            // Add gap size ratio.
            parameters.Add(gapSizeRatio);

            parameters.Add(Convert.ToByte(colors.Count));

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(groupId, StackHalvesSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Fills each half of the group's lights with a gradient between the requested colors.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="sequenceId">
        /// The sequence ID for this command.
        /// </param>
        /// <param name="entryPosition">
        /// The position on the wheel where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="transitionLength">
        /// Number of group lights used to transition from one color to the next.
        /// </param>
        /// <param name="transitionTime">
        /// Milliseconds to transition from one color to the next.
        /// </param>
        /// <param name="colors">
        /// The list of colors to transition between.
         /// </param>
         /// <exception cref="ArgumentOutOfRangeException">
         /// Thrown if the <paramref name="transitionLength"/> or the <paramref name="transitionTime"/> is zero.
         /// </exception>
        protected void RunGradientBetweenColors(byte groupId, uint sequenceId, TwilightZone3DLightStartPosition entryPosition, byte transitionLength,
            ushort transitionTime, IList<Color> colors)
        {
            ValidateGroupId(groupId);

            ValidateColorList(colors);

            if(transitionLength == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionLength), "The transition length cannot be zero.");
            }

            if(transitionTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionTime), "The transition time cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>
            {
                Convert.ToByte(entryPosition),
                transitionLength
            };

            parameters.Add(transitionTime);

            parameters.Add(Convert.ToByte(colors.Count));

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(groupId, sequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Changes the lights on the device to a single color.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="color">
        /// Color to change all lights to.
        /// </param>
        /// <param name="duration">
        /// Time it takes to change all lights in milliseconds.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="color"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero.
        /// </exception>
        protected void ChangeColor(byte groupId, Color color, ushort duration)
        {
            ValidateGroupId(groupId);

            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(color));
            }

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(color.GetRgb16().PackedColor);

            parameters.Add(duration);

            StartSequence(groupId, RandomColorSequenceId, TransitionMode.Immediate, parameters);
        }

        #endregion Protected Methods

    }
}
