//-----------------------------------------------------------------------
// <copyright file = "TwilightZone3DMonitorBezel.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Communication.Cabinet;

    /// <summary>
    /// The twilight zone 3D monitor bezel.
    /// </summary>
    public sealed class TwilightZone3DMonitorBezel : TwilightZone3DLight
    {
        #region Light Data

        /// <summary>
        /// The number of brushes supported.
        /// </summary>
        private static readonly ushort[] NumberOfBrushesSupported = { 1, 2, 4, 8 };

        /// <summary>
        /// The maximum number of segments supported for this device.
        /// </summary>
        private const ushort MaxNumberOfSegmentsSupported = 64;

        #endregion Light Data

        #region Hardware Sequence Ids

        /// <summary>
        /// Cross fades between colors.
        /// </summary>
        private const uint CrossFadeSequenceId = 0x00A4;

        /// <summary>
        /// Fills each half of the group's lights with a gradient between the requested colors.
        /// </summary>
        private const uint FillEachHalfGradientSequenceId = 0x00A5;

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
        /// Randomly change lights through the requested gradient. The sequence creates a sparkle type effect.
        /// </summary>
        private const uint RandomColorsSequenceId = 0x00AB;

        /// <summary>
        /// Rotates equal length segments of multiple colors through opposing halves.
        /// </summary>
        private const uint RotateOpposingHalves = 0xC001;

        #endregion Hardware Sequence Ids

        #region Constructor

        /// <summary>
        /// Constructs a TwilightZone3DMonitorBezel.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal TwilightZone3DMonitorBezel(string featureName, LightFeatureDescription featureDescription,
            IPeripheralLights peripheralLights) : base(featureName, featureDescription, peripheralLights)
        {
        }

        #endregion

        #region TwilightZone3DLight

        /// <inheritdoc />
        protected override short MaxNumberOfColorsSupported => 16;

        /// <inheritdoc />
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="groupId"/> is invalid.
        /// </exception>
        protected override void ValidateGroupId(byte groupId, bool allowAllGroups)
        {
            if(groupId != AllGroups)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId), "Group id must be AllGroups.");
            }
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <param name="brushes"></param> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="brushes"/> contains an empty list.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the number of <paramref name="brushes"/> exceeds the maximum number of brushes supported.
        /// </exception>
        protected override void ValidateBrushes(IList<Paintbrush> brushes)
        {
            if(brushes == null)
            {
                throw new ArgumentNullException(nameof(brushes));
            }

            if(brushes.Count == 0)
            {
                throw new ArgumentException("The list of paint brushes cannot be empty.", nameof(brushes));
            }

            var totalBrushes = brushes.Count;
            if(NumberOfBrushesSupported.All(brush => brush != totalBrushes))
            {
                var message = new StringBuilder("The valid number of brushes are:");
                foreach(var numberOfBrushes in NumberOfBrushesSupported)
                {
                    message.Append(" ");
                    message.Append(numberOfBrushes);
                }
                throw new ArgumentOutOfRangeException(nameof(brushes), message.ToString());
            }
        }

        #endregion TwilightZone3DLight

        #region Public Methods

        /// <summary>
        /// Sets the brightness of all the lights in the specified group.
        /// </summary>
        /// <param name="brightness">The desired brightness.</param>
        public void SetBrightness(byte brightness)
        {
            SetBrightness(AllGroups, brightness);
        }

        /// <summary>
        /// Sets the <paramref name="color"/> of the lights on the monitor bezel.
        /// </summary>
        /// <param name="color">The color to set the lights to.</param>
        public void SetColor(Color color)
        {
            SetColor(AllGroups, color);
        }

        /// <summary>
        /// Sets all the lights to a single color,
        /// and all the lights on the blank light devices to a universal color.
        /// </summary>
        /// <param name="color">The light color to set the lights to.</param>
        /// <param name="universalColor">The universal color to set the blank light devices to.</param>
        public void SetColor(Color color, Color universalColor)
        {
            SetColor(AllGroups, color, universalColor);
        }

        /// <summary>
        /// Flashes between the state of lights when the sequence starts, using a specified color.
        /// </summary>
        /// <param name="numberOfFlashes">
        /// The number of times to flash before ending the sequence. A value of 0 means flashes indefinitely.
        /// </param>
        /// <param name="duration">The duration of a single flash, in 50 millisecond increments.</param>
        /// <param name="color">Color to display for half of the flash duration.</param>
        public void FlashColor(byte numberOfFlashes, byte duration, Color color)
        {
            FlashColor(AllGroups, numberOfFlashes, duration, color);
        }

        /// <summary>
        /// Runs a paint plus sequence on the lights.
        /// </summary>
        /// <param name="brushSpeed">
        /// The number of milliseconds between brush movements.
        /// </param>
        /// <param name="brushes">The number of brushes used to paint the lights at the same time.</param>
        /// <param name="colors">The colors used to paint the lights.</param>
        /// <param name="alternateColors">Boolean flag to indicate if a different color is used for each brush.</param>
        public void RunPaintPlusSequence(ushort brushSpeed, IList<Paintbrush> brushes, IList<Color> colors, bool alternateColors)
        {
            RunPaintPlusSequence(AllGroups, brushSpeed, brushes, colors, alternateColors);
        }

        /// <summary>
        /// Flashes between colors.
        /// </summary>
        /// <param name="duration">Time to display each color in 25 millisecond increments.</param>
        /// <param name="colors">The colors to display.</param>
        public void FlashBetweenColors(byte duration, IList<Color> colors)
        {
            FlashBetweenColors(AllGroups, duration, false, colors);
        }

        /// <summary>
        /// Cross fades the entire group through multiple colors.
        /// </summary>
        /// <remarks>
        /// This sequence can be used to do a finite transition by setting the repeat field to 0.
        /// </remarks>
        /// <param name="duration">The number of milliseconds to pause between each color.</param>
        /// <param name="transitionTime">The number of milliseconds to transition from one color to the next.</param>
        /// <param name="repeat">
        /// Flag that indicates whether to loop the colors or not. 0 = End on last color, and 1 = cross fade continuously.
        /// </param>
        /// <param name="colors">The colors to fade through, in order.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="duration"/> or the <paramref name="transitionTime"/> is zero.
        /// </exception>
        public void RunCrossFade(ushort duration, ushort transitionTime, bool repeat, IList<Color> colors)
        {
            ValidateColorList(colors);

            if(duration == 0)
            {
                throw  new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be zero.");
            }

            if(transitionTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionTime), "The transition time cannot be zero.");
            }

            // Do not use object or collection initializer because of the byte list extension method.
            // ReSharper disable once UseObjectOrCollectionInitializer
            var parameters = new List<byte>();

            // Add inter-color delay.
            parameters.Add(duration);

            // Add transition time.
            parameters.Add(transitionTime);

            // Add whether or not it should repeat.
            parameters.Add(repeat ? (byte)1 : (byte)0);

            // Add number of colors.
            parameters.Add(Convert.ToByte(colors.Count));

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(AllGroups, CrossFadeSequenceId, TransitionMode.Immediate, parameters);
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
        public void RunGradientBetweenColors(TwilightZone3DLightStartPosition entryPosition, byte transitionLength,
            ushort transitionTime, IList<Color> colors)
        {
            RunGradientBetweenColors(AllGroups, FillEachHalfGradientSequenceId, entryPosition, transitionLength,
                transitionTime, colors);
        }

        /// <summary>
        /// Rotates equal-length segments of multiple color clockwise or counterclockwise. The <paramref name="transition"/>
        /// flag be used to fill the group between sets of colors or to start the sequence with the first pattern initialized.
        /// </summary>
        /// <param name="lightDirection">
        /// The chase pattern direction.
        /// </param>
        /// <param name="entryPosition">
        /// The position on the device where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="transition">
        /// A boolean flag used to determine how the colors are filled. 0 = The sequence starts filled with the first pattern
        /// and subsequent patterns immediately follow the last rotated segment. 1 = Start by pushing out the old contents and
        /// fill the group between patterns with the most recent color.
        /// </param>
        /// <param name="rotationSpeed">
        /// Time between movements, in milliseconds.
        /// </param>
        /// <param name="numberOfRotations">
        /// Number of segments to rotate into the group. This determines when to transition (if requested) and when to advance
        /// the pattern colors.
        /// </param>
        /// <param name="numberOfSegments">
        /// Number of equally sized segments to divide the group into.
        /// </param>
        /// <param name="numberOfPatternColors">
        /// The number of colors to repeat throughout the rotated segments and the number of colors to advance for each new
        /// pattern set.
        /// </param>
        /// <param name="colors">
        /// A list of colors to transition between.
        /// </param>
        public void RotateSegments(LightDirection lightDirection, TwilightZone3DLightStartPosition entryPosition, bool transition,
            ushort rotationSpeed, ushort numberOfRotations, byte numberOfSegments, byte numberOfPatternColors, IList<Color> colors)
        {
            var parameters = new List<byte>();

            RotateSegmentHelper(parameters, entryPosition, transition, rotationSpeed, numberOfRotations, numberOfSegments,
                numberOfPatternColors, colors);

            StartSequence(AllGroups, lightDirection == LightDirection.Clockwise ? RotateSegmentsClockwiseSequenceId :
                RotateSegmentsCounterClockwiseSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Runs patterns with stack pieces made from multiple lights that shift in on each half of the lights on the monitor
        /// bezel. When the stacked pieces have filled the lights on the monitor bezel restart the process with a new color.
        /// </summary>
        /// <param name="entryPosition">
        /// The position on the wheel where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="dropSpeed">
        /// Milliseconds between each advancement of a piece from the entry position toward its resting position.
        /// </param>
        /// <param name="pieceSizeRatio">
        /// Ratio of half of the lights on the monitor bezel to the length of each piece. This can also be thought of as the
        /// number of pieces required to fill half of the lights on the monitor bezel.
        /// </param>
        /// <param name="gapSizeRatio">
        /// Ratio of half of the lights on the monitor bezel to the length of the gap between pieces. 0 = introduce a new piece
        /// as soon as the previous piece has come to rest.
        /// </param>
        /// <param name="colors">
        /// The list of colors to transition between.
        /// </param>
        public void RunStackHalves(TwilightZone3DLightStartPosition entryPosition, ushort dropSpeed, byte pieceSizeRatio,
            byte gapSizeRatio, IList<Color> colors)
        {
            RunStackHalves(Convert.ToByte(AllGroups), entryPosition, dropSpeed, pieceSizeRatio, gapSizeRatio, colors);
        }

        /// <summary>
        /// Changes the lights on the monitor bezel to a single color.
        /// </summary>
        /// <param name="color">
        /// Color to change all lights to.
        /// </param>
        /// <param name="duration">
        /// Time it takes to change all lights in milliseconds.
        /// </param>
        public void ChangeColor(Color color, ushort duration)
        {
            ChangeColor(Convert.ToByte(AllGroups), color, duration);
        }

        /// <summary>
        /// Changes each light to the next lower color (by index) each step. If the provided colors form a gradient then this
        /// gives the effect that each random point is fading out.
        /// </summary>
        /// <remarks>
        /// The number of new lights introduced each step can be specified, which allows for aesthetic tuning. If the number of
        /// colors is 1 then this sequence returns the Sequence 
        /// </remarks>
        /// <param name="updateDelay">Milliseconds between each update.</param>
        /// <param name="numberOfNewLightsPerUpdate">The number of new random points to introduce each update.</param>
        /// <param name="initialize">
        /// Flag to initialize the original display. 0 = Randomly update over the original contents.
        /// 1 = Begin with the entire group randomized through once.
        /// </param>
        /// <param name="colors">
        /// The colors to transition between.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="updateDelay"/> is zero.
        /// </exception>
        public void ChangeColors(ushort updateDelay, byte numberOfNewLightsPerUpdate, bool initialize, IList<Color> colors)
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

            parameters.Add(numberOfNewLightsPerUpdate);

            parameters.Add(Convert.ToByte(initialize));

            parameters.Add(Convert.ToByte(colors.Count));

            ConvertPackedColorsToBytes(parameters, colors);

            StartSequence(AllGroups, RandomColorsSequenceId, TransitionMode.Immediate, parameters);
        }

        /// <summary>
        /// Rotate equal length segments of multiple colors through opposing halves. The transition flag can be used to fill
        /// the group between sets of colors or to start the sequence with the first pattern initialized.
        /// </summary>
        /// <param name="entryPosition">
        /// The position on the device where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="transition">
        /// A boolean flag used to determine how the colors are filled. 0 = The sequence starts filled with the first pattern
        /// and subsequent patterns immediately follow the last rotated segment. 1 = Start by pushing out the old contents and
        /// fill the group between patterns with the most recent color.
        /// </param>
        /// <param name="duration">
        /// Time between movements, in milliseconds.
        /// </param>
        /// <param name="numberOfRotations">
        /// Number of segments to rotate into the group. This determines when to transition (if requested) and when to advance
        /// the pattern colors.
        /// </param>
        /// <param name="numberOfSegments">
        /// Number of equally sized segments to divide the group into.
        /// </param>
        /// <param name="numberOfPatternColors">
        /// The number of colors to repeat throughout the rotated segments and the number of colors to advance for each new
        /// pattern set.
        /// </param>
        /// <param name="colors">
        /// A list of colors to transition between.
        /// </param>
        public void RotateSegmentsOnOpposingHalves(TwilightZone3DLightStartPosition entryPosition, bool transition,
            ushort duration, ushort numberOfRotations, byte numberOfSegments, byte numberOfPatternColors, IList<Color> colors)
        {
            var parameters = new List<byte>();

            RotateSegmentHelper(parameters, entryPosition, transition, duration, numberOfRotations, numberOfSegments,
                numberOfPatternColors, colors);

            StartSequence(AllGroups, RotateOpposingHalves, TransitionMode.Immediate, parameters);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Helper function used to set up the parameters needed for segment rotations.
        /// </summary>
        /// <param name="parameters">
        /// The list of bytes to send to the device driver.
        /// </param>
        /// <param name="entryPosition">
        /// The position on the device where the pattern starts. The pattern runs in 45 degrees increments.
        /// </param>
        /// <param name="transition">
        /// A boolean flag used to determine how the colors are filled. 0 = The sequence starts filled with the first pattern
        /// and subsequent patterns immediately follow the last rotated segment. 1 = Start by pushing out the old contents and
        /// fill the group between patterns with the most recent color.
        /// </param>
        /// <param name="rotationSpeed">
        /// Time between movements, in milliseconds.
        /// </param>
        /// <param name="numberOfRotations">
        /// Number of segments to rotate into the group. This determines when to transition (if requested) and when to advance
        /// the pattern colors.
        /// </param>
        /// <param name="numberOfSegments">
        /// Number of equally sized segments to divide the group into.
        /// </param>
        /// <param name="numberOfPatternColors">
        /// The number of colors to repeat throughout the rotated segments and the number of colors to advance for each new
        /// pattern set.
        /// </param>
        /// <param name="colors">
        /// A list of colors to transition between.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="rotationSpeed"/> is zero.
        /// </exception>
        private void RotateSegmentHelper(IList<byte> parameters, TwilightZone3DLightStartPosition entryPosition, bool transition,
            ushort rotationSpeed, ushort numberOfRotations, byte numberOfSegments, byte numberOfPatternColors, IList<Color> colors)
        {
            ValidateSegments(numberOfSegments);

            ValidateColorList(colors);

            if(rotationSpeed == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rotationSpeed), "The rotation speed cannot be zero.");
            }

            // Add transition control.
            if(transition)
            {
                entryPosition += 16;
            }
            parameters.Add(Convert.ToByte(entryPosition));

            // Add rotation speed.
            parameters.Add(rotationSpeed);

            // Add number of rotations.
            parameters.Add(numberOfRotations);

            // Add number of segments.
            parameters.Add(numberOfSegments);

            // Add number of pattern colors.
            parameters.Add(numberOfPatternColors);

            // Add number of colors.
            parameters.Add(Convert.ToByte(colors.Count));

            // Add colors.
            ConvertPackedColorsToBytes(parameters, colors);
        }

        /// <summary>
        /// Validates the number of segments supported by this device.
        /// </summary>
        /// <param name="segments">The number of segments to validate.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the <paramref name="segments"/> is not in an acceptable range.
        /// </exception>
        private void ValidateSegments(byte segments)
        {
            if(segments == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(segments), "The number of segments cannot be 0.");
            }

            if(segments > MaxNumberOfSegmentsSupported)
            {
                throw new ArgumentOutOfRangeException(nameof(segments),
                    $"The number of segments {segments} is greater than {MaxNumberOfSegmentsSupported}, " +
                    "which is the maximum number of segments allowed.");
            }
        }

        #endregion Private Methods

    }
}
