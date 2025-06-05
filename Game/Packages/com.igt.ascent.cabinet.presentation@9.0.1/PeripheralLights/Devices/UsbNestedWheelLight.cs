//-----------------------------------------------------------------------
// <copyright file = "UsbNestedWheelLight.cs" company = "IGT">
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
    /// The USB nested wheels light hardware.
    /// </summary>
    /// <remarks>
    /// The GroupCount for this hardware device is 3. Ring = 0, Pointer = 1, and Backlight = 2.
    /// </remarks>
    public class UsbNestedWheelLight : TwilightZone3DLight
    {
        #region Light Data

        /// <summary>
        /// The maximum number of brushes supported for this device.
        /// </summary>
        private const ushort MaxNumberOfBrushesSupported = 8;

        /// <summary>
        /// The number of light groups supported by this device.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private const byte groupCount = 3;

        /// <summary>
        /// The group ids of the lights for the nested wheels device.
        /// </summary>
        private enum GroupId : byte
        {
            /// <summary>
            /// The ring around the wheel.
            /// </summary>
            Ring = 0,

            /// <summary>
            /// The pointers on the wheel.
            /// </summary>
            Pointers,

            /// <summary>
            /// The backlight of the wheel.
            /// </summary>
            Backlight
        }

        #region Hardware Sequence Ids

        /// <summary>
        /// Flashes all lights on and off.
        /// </summary>
        private const uint FlashAllLightsSequenceId = 0x0042;

        /// <summary>
        /// Cross fades between colors.
        /// </summary>
        private const uint CrossFadeSequenceId = 0x00A4;

        /// <summary>
        /// Cross fade transitions between colors.
        /// </summary>
        private const uint CrossFadeTransitionId = 0x00A5;

        /// <summary>
        /// Chase pattern that runs clockwise.
        /// </summary>
        private const uint ChaseClockwiseSequenceId = 0x009E;

        /// <summary>
        /// Chase pattern that runs counter clockwise.
        /// </summary>
        private const uint ChaseCounterClockwiseSequenceId = 0x009F;

        /// <summary>
        /// Runs a chase pattern clockwise.  After a pattern has completed the requested number of rotations the entire group
        /// gradually fills a solid color and the process repeats in a new color.
        /// </summary>
        private const uint RotateSegmentsClockwiseSequenceId = 0x00A6;

        /// <summary>
        /// Runs a chase pattern counter clockwise.  After a pattern has completed the requested number of rotations the entire
        /// group gradually fills a solid color and the process repeats in a new color.
        /// </summary>
        private const uint RotateSegmentsCounterClockwiseSequenceId = 0x00A7;

        /// <summary>
        /// Fill each half of the group's lights with a gradient between the requested colors.
        /// </summary>
        private const uint FillEachHalfGradientSequenceId = 0x00A9;

        /// <summary>
        /// Randomly change lights through the requested gradient. The sequence creates a sparkle type effect.
        /// </summary>
        private const uint RandomColorsSequenceId = 0x00AB;

        /// <summary>
        /// A single contiguous chunk of lights chases around the light group in the clockwise direction.
        /// </summary>
        private const uint ChaseSequenceClockwiseSequenceId = 0xC000;

        /// <summary>
        /// A single contiguous chunk of lights chases around the light group in the counterclockwise direction.
        /// </summary>
        private const uint ChaseSequenceCounterclockwiseSequenceId = 0xC001;

        /// <summary>
        /// Flashes any number of pointers between two colors.
        /// </summary>
        private const uint FlashPointersBetweenTwoColorsSequenceId = 0xC002;

        /// <summary>
        /// The attract sequence.
        /// </summary>
        private const uint AttractSequenceId = 0xFFFF;

        #endregion Hardware Sequence Ids

        #endregion Light Data

        #region Constructor

        /// <summary>
        /// Constructs a UsbNestedWheelsLightBase.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbNestedWheelLight(string featureName, LightFeatureDescription featureDescription,
            IPeripheralLights peripheralLights) : base(featureName, featureDescription, peripheralLights)
        {
            GroupCount = groupCount;
        }

        #endregion

        #region TwilightZone3DLight

        /// <inheritdoc />
        protected override short MaxNumberOfColorsSupported => 16;

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="brushes"/> is null.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the there is no brush defined in <paramref name="brushes"/> or the number of brushes in
        /// <paramref name="brushes"/> exceeds the maximum number of brushes supported by this device.
        /// </exception>
        protected override void ValidateBrushes(IList<Paintbrush> brushes)
        {
            if(brushes == null)
            {
                throw new ArgumentNullException(nameof(brushes));
            }

            if(!brushes.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(brushes), "There has to be at least 1 paint brush.");
            }

            if(brushes.Count > MaxNumberOfBrushesSupported)
            {
                throw new ArgumentOutOfRangeException(nameof(brushes),
                    $"The number of brushes {brushes.Count} exceeds the maximum " +
                    $"number of brushes supported {MaxNumberOfBrushesSupported} by this device");
            }
        }

        #endregion

        #region Backlights

        /// <summary>
        /// Turns on the backlights.
        /// </summary>
        public void TurnBacklightsOn()
        {
            SetColor(Convert.ToByte(GroupId.Backlight), Color.White);
        }

        /// <summary>
        /// Turns off the backlights.
        /// </summary>
        public void TurnBacklightsOff()
        {
            SetColor(Convert.ToByte(GroupId.Backlight), Color.Black);
        }

        #endregion Backlights

        #region Pointers

        /// <summary>
        /// Sets the <paramref name="color"/> of the lights on the pointers.
        /// </summary>
        /// <param name="color">The color to set the lights to.</param>
        public void SetColorOnPointers(Color color)
        {
            SetColor(Convert.ToByte(GroupId.Pointers), color);
        }

        /// <summary>
        /// Sets a pointer color on the wheel given the <paramref name="lightId"/>.
        /// </summary>
        /// <param name="lightId">The pointer light ID associated with the pointer.</param>
        /// <param name="color">The color to set the pointer to.</param>
        public void SetPointerColor(ushort lightId, Color color)
        {
            SetColor(Convert.ToByte(GroupId.Pointers), lightId, color);
        }

        /// <summary>
        /// Sets the color on all the pointers individually given the <paramref name="colors"/> list.
        /// </summary>
        /// <param name="colors">The desired color for each pointer.</param>
        public void SetPointerColors(IEnumerable<Color> colors)
        {
            SetColor(Convert.ToByte(GroupId.Pointers), colors);
        }

        /// <summary>
        /// Sets the brightness of the lights on the pointers.
        /// </summary>
        /// <param name="brightness">The desired brightness.</param>
        public void SetBrightnessOnPointers(byte brightness)
        {
            SetBrightness(Convert.ToByte(GroupId.Pointers), brightness);
        }

        /// <summary>
        /// Flashes lights on the pointers on and off.
        /// </summary>
        public void FlashPointers()
        {
            FlashLightsOnAndOff(Convert.ToByte(GroupId.Pointers));
        }

        /// <summary>
        /// Flashes between the state of lights when the sequence starts, using a specified color.
        /// </summary>
        /// <param name="numberOfFlashes">
        /// The number of times to flash before ending the sequence. A value of 0 means flashes indefinitely.
        /// </param>
        /// <param name="duration">The duration of a single flash, in 50 millisecond increments.</param>
        /// <param name="color">Color to display for half of the flash duration.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="duration"/> is not in 50 millisecond increments.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero.
        /// </exception>
        public void FlashColorOnPointers(byte numberOfFlashes, byte duration, Color color)
        {
            FlashColor(Convert.ToByte(GroupId.Pointers), numberOfFlashes, duration, color);
        }

        /// <summary>
        /// Flashes between colors on the pointers.
        /// </summary>
        /// <param name="duration">Time to display each color in 25 millisecond increments.</param>
        /// <param name="looping">Flag that indicates whether or loop flashing or not.</param>
        /// <param name="colors">The colors to display.</param>
        public void FlashBetweenColorsOnPointers(byte duration, bool looping, IList<Color> colors)
        {
            FlashBetweenColors(Convert.ToByte(GroupId.Pointers), duration, looping, colors);
        }

        /// <summary>
        /// Fades between colors on the pointers.
        /// </summary>
        /// <param name="duration">The number of milliseconds to display each color.</param>
        /// <param name="transitionTime">The number of milliseconds it takes to transition between colors.</param>
        /// <param name="colors">The list of colors to fade between.</param>
        public void CrossFadeBetweenColorsOnPointers(ushort duration, ushort transitionTime, IList<Color> colors)
        {
            CrossFadeBetweenColors(Convert.ToByte(GroupId.Pointers), duration, transitionTime, colors);
        }

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        /// <param name="transitionTime">The number of milliseconds it takes for the color transition.</param>
        /// <param name="currentColor">The current color of the lights.</param>
        /// <param name="newColor">The new color of the lights.</param>
        public void CrossFadeToNewColorOnPointers(ushort transitionTime, Color currentColor, Color newColor)
        {
            CrossFadeToNewColor(Convert.ToByte(GroupId.Pointers), transitionTime, currentColor, newColor);
        }

        /// <summary>
        /// Flashes any number of pointers between two colors.
        /// </summary>summary>
        /// <remarks>
        /// All pointers omitted from the parameters to send are left with their current color. The flashing is automatically
        /// synchronized with any flashing on other groups and/or other Feature 101 interfaces. Any invalid configuration of
        /// parameters will result in the message being rejected and all the pointers for that interface being turned off.
        /// </remarks>
        /// <param name="duration">Flash duration time in 50 millisecond increments.</param>
        /// <param name="records">Back-to-back <see cref="PointerFlashRecord"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="records"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if there is no pointer flash record in the <paramref name="records"/> list.
        /// </exception>
        public void FlashPointersBetweenTwoColors(byte duration, IList<PointerFlashRecord> records)
        {
            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            if(records == null)
            {
                throw new ArgumentNullException(nameof(records));
            }

            if(!records.Any())
            {
                throw new ArgumentException("There is no pointer flash record provided.", nameof(records));
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>
            {
                duration,
                Convert.ToByte(records.Count)
            };

            foreach(var record in records)
            {
                parameters.Add(record.PointerIndex);
                parameters.Add(record.Color1.GetRgb16().PackedColor);
                parameters.Add(record.Color2.GetRgb16().PackedColor);
            }

            StartSequence(Convert.ToByte(GroupId.Pointers), FlashPointersBetweenTwoColorsSequenceId, TransitionMode.Immediate,
                parameters);
        }

        #endregion End Pointers

        #region Light Ring

        /// <summary>
        /// Sets the <paramref name="color"/> of the lights on the light ring.
        /// </summary>
        /// <param name="color">The color to set the lights to.</param>
        public void SetColorOnLightRing(Color color)
        {
            SetColor(Convert.ToByte(GroupId.Ring), color);
        }

        /// <summary>
        /// Sets the brightness of the lights on the light ring.
        /// </summary>
        /// <param name="brightness">The desired brightness.</param>
        public void SetBrightnessOnLightRing(byte brightness)
        {
            SetBrightness(Convert.ToByte(GroupId.Ring), brightness);
        }

        /// <summary>
        /// Flashes lights on the light ring on and off.
        /// </summary>
        public void FlashLightRing()
        {
            FlashLightsOnAndOff(Convert.ToByte(GroupId.Ring));
        }

        /// <summary>
        /// Runs a chase pattern clockwise or counterclockwise on the light ring.
        /// </summary>
        /// <param name="direction">The chase direction.</param>
        /// <param name="numberOfDashes">The number of dashes to display, evenly spaced, throughout the light ring.</param>
        /// <param name="foregroundColor">The first color of the chase sequence.</param>
        /// <param name="backgroundColor">The second color of the chase sequence.</param>
        /// <param name="duration">Time it takes to do one full rotation, in milliseconds.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="foregroundColor"/> or the <paramref name="backgroundColor"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> or the <paramref name="numberOfDashes"/> is zero.
        /// </exception>
        public void RunColorChaseOnLightRing(LightDirection direction, byte numberOfDashes, Color foregroundColor,
                                             Color backgroundColor, ushort duration)
        {
            if(foregroundColor.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(foregroundColor));
            }

            if(backgroundColor.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(backgroundColor));
            }

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            if(numberOfDashes == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfDashes), "The number of dashes to display is zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(foregroundColor.GetRgb16().PackedColor);

            parameters.Add(backgroundColor.GetRgb16().PackedColor);

            parameters.Add(numberOfDashes);

            parameters.Add(duration);

            StartSequence(Convert.ToByte(GroupId.Ring),
                direction == LightDirection.Clockwise ? ChaseClockwiseSequenceId : ChaseCounterClockwiseSequenceId,
                TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Flashes between the state of lights when the sequence starts, using a specified color.
        /// </summary>
        /// <param name="numberOfFlashes">
        /// The number of times to flash before ending the sequence. A value of 0 means flashes indefinitely.
        /// </param>
        /// <param name="duration">The duration of a single flash, in 50 millisecond increments.</param>
        /// <param name="color">Color to display for half of the flash duration.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="duration"/> is not in 50 millisecond increments.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero.
        /// </exception>
        public void FlashColorOnLightRing(byte numberOfFlashes, byte duration, Color color)
        {
            FlashColor(Convert.ToByte(GroupId.Ring), numberOfFlashes, duration, color);
        }

        /// <summary>
        /// Runs a paint plus sequence on the light ring.
        /// </summary>
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
        public void RunPaintPlusSequenceOnLightRing(ushort duration, IList<Paintbrush> brushes, IList<Color> colors,
                                                    bool alternateColors)
        {
            RunPaintPlusSequence(Convert.ToByte(GroupId.Ring), duration, brushes, colors, alternateColors);
        }

        /// <summary>
        /// Flashes between colors on the light ring.
        /// </summary>
        /// <param name="duration">Time to display each color in 25 millisecond increments.</param>
        /// <param name="looping">Flag that indicates whether or loop flashing or not.</param>
        /// <param name="colors">The colors to display.</param>
        public void FlashBetweenColorsOnLightRing(byte duration, bool looping, IList<Color> colors)
        {
            FlashBetweenColors(Convert.ToByte(GroupId.Ring), duration, looping, colors);
        }

        /// <summary>
        /// Fades between colors on the light ring.
        /// </summary>
        /// <param name="duration">The number of milliseconds to display each color.</param>
        /// <param name="transitionTime">The number of milliseconds it takes to transition between colors.</param>
        /// <param name="colors">The list of colors to fade between.</param>
        public void CrossFadeBetweenColorsOnLightRing(ushort duration, ushort transitionTime, IList<Color> colors)
        {
            CrossFadeBetweenColors(Convert.ToByte(GroupId.Ring), duration, transitionTime, colors);
        }

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        /// <param name="transitionTime">The number of milliseconds it takes for the color transition.</param>
        /// <param name="currentColor">The current color of the lights.</param>
        /// <param name="newColor">The new color of the lights.</param>
        public void CrossFadeToNewColorOnLightRing(ushort transitionTime, Color currentColor, Color newColor)
        {
            CrossFadeToNewColor(Convert.ToByte(GroupId.Ring), transitionTime, currentColor, newColor);
        }

        /// <summary>
        /// Runs a chase pattern clockwise or counterclockwise on the light ring.
        /// </summary>
        /// <param name="lightDirection">
        /// The chase pattern direction.
        /// </param>
        /// <param name="entryPosition">
        /// The position on the wheel where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="numberOfDashes">
        /// The number of dashes to display, evently spaced, throughout the light
        /// group once the entire chase pattern has entered the group.
        /// </param>
        /// <param name="duration">
        /// The time it takes to do one full rotation, in milliseconds.
        /// </param>
        /// <param name="numberOfRotations">
        /// The number of rotations around the group before beginning the transition to the next color.
        /// </param>
        /// <param name="colors">
        /// The list of colors to transition between.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="numberOfDashes"/> or the <paramref name="duration"/> or the 
        /// <paramref name="numberOfRotations"/> is zero.
        /// </exception>
        public void RotateSegmentsOnLightRing(LightDirection lightDirection, TwilightZone3DLightStartPosition entryPosition,
                                              byte numberOfDashes, ushort duration, byte numberOfRotations, IList<Color> colors)
        {
            ValidateColorList(colors);

            if(numberOfDashes == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfDashes), "The number of dashes cannot be zero.");
            }

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            if(numberOfRotations == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfRotations), "The number of rotations cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>
            {
                0, // Add a reserved byte.
                Convert.ToByte(entryPosition), // The patterns comes in on the right if it is not specified.
                numberOfDashes
            };

            // Add cycle duration.
            parameters.Add(duration);

            // Add the number of rotations around the group before beginning the transition to the next color.
            parameters.Add(numberOfRotations);

            parameters.Add(Convert.ToByte(colors.Count));

            // Add colors.
            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(Convert.ToByte(GroupId.Ring),
                lightDirection == LightDirection.Clockwise ? RotateSegmentsClockwiseSequenceId :
                RotateSegmentsCounterClockwiseSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Runs patterns with stack pieces made from multiple lights that shift in on each half of the light ring.
        /// When the stacked pieces have filled the light ring restart the process with a new color.
        /// </summary>
        /// <param name="entryPosition">
        /// The position on the wheel where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="dropSpeed">
        /// Milliseconds between each advancement of a piece from the etnry position toward its resting position.
        /// </param>
        /// <param name="pieceSizeRatio">
        /// Ratio of half of the ring's lights to the length of each piece. This can also be thought of as the number of
        /// pieces required to fill half of the ring's lights.
        /// </param>
        /// <param name="gapSizeRatio">
        /// Ratio of half of the ring's lights to the length of the gap between pieces. 0 = introduce a new piece as
        /// soon as the previous piece has come to rest.
        /// </param>
        /// <param name="colors">
        /// The list of colors to transition between.
        /// </param>
        public void RunStackHalvesOnLightRing(TwilightZone3DLightStartPosition entryPosition, ushort dropSpeed,
                                              byte pieceSizeRatio, byte gapSizeRatio, IList<Color> colors)
        {
            RunStackHalves(Convert.ToByte(GroupId.Ring), entryPosition, dropSpeed, pieceSizeRatio, gapSizeRatio, colors);
        }

        /// <summary>
        /// Fills each half of the group's lights with a gradient between the requested colors.
        /// </summary>
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
        public void RunGradientBetweenColorsOnLightRing(TwilightZone3DLightStartPosition entryPosition, byte transitionLength,
                                                        ushort transitionTime, IList<Color> colors)
        {
            RunGradientBetweenColors(Convert.ToByte(GroupId.Ring), FillEachHalfGradientSequenceId, entryPosition,
                transitionLength, transitionTime, colors);
        }

        /// <summary>
        /// Changes the color on the light ring to a single color.
        /// </summary>
        /// <param name="color">
        /// Color to change all lights to.
        /// </param>
        /// <param name="duration">
        /// Time it takes to change all lights in milliseconds.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> is zero.
        /// </exception>
        public void ChangeColorOnLightRing(Color color, ushort duration)
        {
            ChangeColor(Convert.ToByte(GroupId.Ring), color, duration);
        }

        /// <summary>
        /// Changes the colors on the light ring. This sequence creates a sparkle type effect.
        /// </summary>
        /// <param name="updateDelay">
        /// Milliseconds between each update of the pattern on the lights.
        /// </param>
        /// <param name="colors">
        /// The colors to transition between.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="updateDelay"/> is zero.
        /// </exception>
        public void ChangeColorsOnLightRing(ushort updateDelay, IList<Color> colors)
        {
            ValidateColorList(colors);

            if(updateDelay == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(updateDelay), "The update delay cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(updateDelay);

            parameters.Add(Convert.ToByte(colors.Count));

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(Convert.ToByte(GroupId.Ring), RandomColorsSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Runs a single continguous chunk of lights chases around the light ring in the clockwise/counterclockwise direction.
        /// </summary>
        /// <param name="lightDirection">
        /// The chase direction.
        /// </param>
        /// <param name="chaseColor">
        /// Color for the single contiguous chunk of lights.
        /// </param>
        /// <param name="backgroundColor">
        /// Color for the rest of the ligth group.
        /// </param>
        /// <param name="cycleDuration">
        /// Time it takes to do one full rotation, in milliseconds.
        /// </param>
        /// <param name="chaserSizeRatio">
        /// Ratio of the group's lights to the length of the contiguous chunk.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="chaseColor"/> or the <paramref name="backgroundColor"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thown if the <paramref name="cycleDuration"/> or the <paramref name="chaserSizeRatio"/> is zero.
        /// </exception>
        public void RunChaseOnLightRing(LightDirection lightDirection, Color chaseColor, Color backgroundColor,
                                        ushort cycleDuration, byte chaserSizeRatio)
        {
            if(chaseColor.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(chaseColor));
            }

            if(backgroundColor.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(backgroundColor));
            }

            if(cycleDuration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cycleDuration), "The cycle duration cannot be zero.");
            }

            if(chaserSizeRatio == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chaserSizeRatio), "The chase size ratio cannot be zero.");
            }

            // Do not use object or collection initializer because of the extended method used to add a packed color.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(chaseColor.GetRgb16().PackedColor);

            parameters.Add(backgroundColor.GetRgb16().PackedColor);

            parameters.Add(cycleDuration);

            parameters.Add(chaserSizeRatio);

            StartSequence(Convert.ToByte(GroupId.Ring),
                lightDirection == LightDirection.Clockwise ? ChaseSequenceClockwiseSequenceId :
                ChaseSequenceCounterclockwiseSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Run the attract pattern on the light ring.
        /// </summary>
        public void RunAttractOnLightRing()
        {
            StartSequence(Convert.ToByte(GroupId.Ring), AttractSequenceId, TransitionMode.Immediate, null);
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Flashes the lights on and off.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the flash to.</param>
        private void FlashLightsOnAndOff(byte groupId)
        {
            ValidateGroupId(groupId);

            StartSequence(groupId, FlashAllLightsSequenceId, TransitionMode.Immediate, null);
        }

        /// <summary>
        /// Fades between colors.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="duration">The number of milliseconds to display each color.</param>
        /// <param name="transitionTime">The number of milliseconds it takes to transition between colors.</param>
        /// <param name="colors">The list of colors to fade between.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> or the <paramref name="transitionTime"/> is zero.
        /// </exception>
        private void CrossFadeBetweenColors(byte groupId, ushort duration, ushort transitionTime, IList<Color> colors)
        {
            ValidateGroupId(groupId);

            ValidateColorList(colors);

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            if(transitionTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionTime), "The transition time cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(duration);

            parameters.Add(transitionTime);

            parameters.Add(Convert.ToByte(colors.Count));

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(groupId, CrossFadeSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        /// <param name="groupId">The number of the light group to apply the sequence to.</param>
        /// <param name="transitionTime">The number of milliseconds it takes for the color transition.</param>
        /// <param name="currentColor">The current color of the lights.</param>
        /// <param name="newColor">The new color of the lights.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="currentColor"/> or the <paramref name="newColor"/> is empty.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="transitionTime"/> is zero.
        /// </exception>
        private void CrossFadeToNewColor(byte groupId, ushort transitionTime, Color currentColor, Color newColor)
        {
            ValidateGroupId(groupId);

            if(currentColor.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(currentColor));
            }

            if(newColor.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(newColor));
            }

            if(transitionTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionTime), "The transition time cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            parameters.Add(transitionTime);

            parameters.Add(currentColor.GetRgb16().PackedColor);

            parameters.Add(newColor.GetRgb16().PackedColor);

            StartSequence(groupId, CrossFadeTransitionId, TransitionMode.Immediate, parameters);
        }

        #endregion Helper Functions
    }
}
