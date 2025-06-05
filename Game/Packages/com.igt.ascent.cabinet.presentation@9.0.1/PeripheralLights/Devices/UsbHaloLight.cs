//-----------------------------------------------------------------------
// <copyright file = "UsbHaloLight.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// Represents a USB halo peripheral light hardware.
    /// </summary>
    public class UsbHaloLight : UsbAccentLight
    {
        #region Hardware Sequence IDs

        private const ushort RainbowFadeSequenceId = 0xC008;
        private const ushort CustomChaseSequenceId = 0xC00B;
        private const ushort RotateHalvesSequenceId = 0xC00C;

        #endregion

        /// <summary>
        /// The group ID the halo lights all use.
        /// </summary>
        protected const byte HaloLightGroupId = AllGroups;

        /// <summary>
        /// Construct a USB halo light.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbHaloLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {

        }

        /// <summary>
        /// Sets all the lights to a single color.
        /// </summary>
        /// <param name="color">The color to set the lights to.</param>
        public void SetColor(Color color)
        {
            SetColor(HaloLightGroupId, color);
        }

        /// <summary>
        /// Sets all the lights to a single color,
        /// and all the lights on the blank light devices to a universal color.
        /// </summary>
        /// <param name="color">The light color to set the lights to.</param>
        /// <param name="universalColor">The universal color to set the blank light devices to.</param>
        public void SetColor(Color color, Color universalColor)
        {
            SetColor(HaloLightGroupId, color, universalColor);
        }

        #region Accent Functions

        /// <summary>
        /// This sequence simulates a paint brush moving around the light device "painting" the lights with a new color as it goes.
        /// After painting the entire button edge the process repeats with a new color.
        /// </summary>
        /// <param name="delay">The number of milliseconds between light updates.</param>
        /// <param name="brushes">The brushes use to paint the lights.</param>
        /// <param name="colors">The colors to use with the brushes.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="brushes"/> is null.
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="brushes"/> is not null but contains no brushes.
        /// </exception>
        public void RunPaintSequence(ushort delay, IList<Paintbrush> brushes, IList<Color> colors)
        {
            RunPaintSequence(delay, brushes, colors, false, TransitionMode.Immediate);
        }

        /// <summary>
        /// This sequence simulates a paint brush moving around the light device "painting" the lights with a new color as it goes.
        /// After painting the entire button edge the process repeats with a new color.
        /// </summary>
        /// <param name="delay">The number of milliseconds between light updates.</param>
        /// <param name="brushes">The brushes use to paint the lights.</param>
        /// <param name="colors">The colors to use with the brushes.</param>
        /// <param name="alternateColors">If true a different color is used for each brush.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="brushes"/> is null.
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="brushes"/> is not null but contains no brushes.
        /// </exception>
        public void RunPaintSequence(ushort delay, IList<Paintbrush> brushes, IList<Color> colors, bool alternateColors, TransitionMode transitionMode)
        {
            RunPaintSequence(HaloLightGroupId, delay, brushes, colors, alternateColors, transitionMode);
        }

        /// <summary>
        /// Configures a color flash sequence on the lights. The lights will cycle through the specified colors forever.
        /// </summary>
        /// <param name="duration">The amount of time to display each color in 25ms increments.</param>
        /// <param name="colors">The list of colors to cycle through.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunColorFlash(byte duration, IList<Color> colors)
        {
            RunColorFlash(duration, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// Configures a color flash sequence on the lights. The lights will cycle through the specified colors forever.
        /// </summary>
        /// <param name="duration">The amount of time to display each color in 25ms increments.</param>
        /// <param name="colors">The list of colors to cycle through.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunColorFlash(byte duration, IList<Color> colors, TransitionMode transitionMode)
        {
            RunColorFlash(HaloLightGroupId, duration, colors, transitionMode);
        }

        /// <summary>
        /// Configures a continuous color cross fade cycle. Similar to RunColorFlash but there is a fade between each color.
        /// </summary>
        /// <param name="duration">The number of milliseconds to display each color.</param>
        /// <param name="transitionTime">The number of milliseconds it takes to transition to a new color.</param>
        /// <param name="colors">The list of colors to cycle through.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is 0.
        /// Thrown if <paramref name="transitionTime"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunColorFade(ushort duration, ushort transitionTime, IList<Color> colors)
        {
            RunColorFade(duration, transitionTime, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// Configures a continuous color cross fade cycle. Similar to RunColorFlash but there is a fade between each color.
        /// </summary>
        /// <param name="duration">The number of milliseconds to display each color.</param>
        /// <param name="transitionTime">The number of milliseconds it takes to transition to a new color.</param>
        /// <param name="colors">The list of colors to cycle through.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is 0.
        /// Thrown if <paramref name="transitionTime"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunColorFade(ushort duration, ushort transitionTime, IList<Color> colors, TransitionMode transitionMode)
        {
            RunColorFade(HaloLightGroupId, duration, transitionTime, colors, transitionMode);
        }

        /// <summary>
        /// Cross fades colors in a specified direction over the lights.
        /// </summary>
        /// <param name="delay">The number of milliseconds between changing colors.</param>
        /// <param name="direction">The direction the cross fade should go.</param>
        /// <param name="colors">The list of colors to use in the cross fade.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunCrossFade(ushort delay, AccentLightDirection direction, IList<Color> colors)
        {
            RunCrossFade(delay, direction, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// Cross fades colors in a specified direction over the lights.
        /// </summary>
        /// <param name="delay">The number of milliseconds between changing colors.</param>
        /// <param name="direction">The direction the cross fade should go.</param>
        /// <param name="colors">The list of colors to use in the cross fade.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunCrossFade(ushort delay, AccentLightDirection direction, IList<Color> colors, TransitionMode transitionMode)
        {
            RunCrossFade(HaloLightGroupId, delay, direction, colors, transitionMode);
        }

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        /// <param name="duration">The number of milliseconds to take for the color transition.</param>
        /// <param name="currentColor">The current color of the lights.</param>
        /// <param name="newColor">The new color of the lights.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is 0.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="currentColor"/> equals Color.Empty.
        /// Thrown if <paramref name="newColor"/> equals Color.Empty.
        /// </exception>
        public void RunFadeToNewColor(ushort duration, Color currentColor, Color newColor)
        {
            RunFadeToNewColor(duration, currentColor, newColor, TransitionMode.Immediate);
        }

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        /// <param name="duration">The number of milliseconds to take for the color transition.</param>
        /// <param name="currentColor">The current color of the lights.</param>
        /// <param name="newColor">The new color of the lights.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is 0.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="currentColor"/> equals Color.Empty.
        /// Thrown if <paramref name="newColor"/> equals Color.Empty.
        /// </exception>
        public void RunFadeToNewColor(ushort duration, Color currentColor, Color newColor, TransitionMode transitionMode)
        {
            RunFadeToNewColor(HaloLightGroupId, duration, currentColor, newColor, transitionMode);
        }

        #endregion

        #region Rainbow Fade Sequence

        /// <summary>
        /// This sequence is a multi-color rainbow fade over the entire light device.
        /// </summary>
        /// <param name="delay">The number of milliseconds between color changes.</param>
        /// <param name="direction">The direction of the fade.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// </exception>
        public void RunRainbowFade(ushort delay, HaloLightDirection direction)
        {
            RunRainbowFade(delay, direction, TransitionMode.Immediate);
        }

        /// <summary>
        /// This sequence is a multi-color rainbow fade over the entire light device.
        /// </summary>
        /// <param name="delay">The number of milliseconds between color changes.</param>
        /// <param name="direction">The direction of the fade.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// </exception>
        public void RunRainbowFade(ushort delay, HaloLightDirection direction,
            TransitionMode transitionMode)
        {
            if(delay == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "The delay cannot be zero.");
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
               // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(delay);
                parameters.Add((byte)direction);

                StartSequence(HaloLightGroupId, RainbowFadeSequenceId, transitionMode,
                    parameters);
            }
        }

        #endregion

        #region Chase Sequence

        /// <summary>
        /// This sequence is a light chase around the lights, there can be any number of colors and any number of segments. 
        /// </summary>
        /// <param name="delay">The number of milliseconds between steps.</param>
        /// <param name="direction">The direction of the chase.</param>
        /// <param name="segmentCount">The number of light segments to use to make the chase sequence.</param>
        /// <param name="colors">The list of colors to use.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="segmentCount"/> is 0.
        /// Thrown if the numbers of colors in <paramref name="colors"/> is greater than 255.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 3 colors.
        /// </exception>
        public void RunChaseSequence(ushort delay, LightDirection direction, byte segmentCount, IList<Color> colors)
        {
            RunChaseSequence(delay, direction, segmentCount, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// This sequence is a light chase around the lights, there can be any number of colors and any number of segments. 
        /// </summary>
        /// <param name="delay">The number of milliseconds between steps.</param>
        /// <param name="direction">The direction of the chase.</param>
        /// <param name="segmentCount">The number of light segments to use to make the chase sequence.</param>
        /// <param name="colors">The list of colors to use.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="segmentCount"/> is 0.
        /// Thrown if the numbers of colors in <paramref name="colors"/> is greater than 255.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 3 colors.
        /// </exception>
        public void RunChaseSequence(ushort delay, LightDirection direction, byte segmentCount, IList<Color> colors, TransitionMode transitionMode)
        {
            if(segmentCount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(segmentCount), "The segment count cannot be 0.");
            }

            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(colors.Count == 0)
            {
                throw new InsufficientNumberOfColorsProvidedException(1, colors.Count);
            }

            if(colors.Count > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(colors), "There cannot be more than 255 colors specified.");
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(delay);
                parameters.Add((byte)direction);
                parameters.Add(segmentCount);
                parameters.Add((byte)colors.Count);

                ProcessColors(parameters, colors);

                StartSequence(HaloLightGroupId, CustomChaseSequenceId, transitionMode, parameters);
            }
        }

        #endregion

        #region Rotate Halves Sequence

        /// <summary>
        /// This sequence has two halves that either converge or separate. Each half can have any number of colors.
        /// </summary>
        /// <param name="delay">The number of milliseconds between steps.</param>
        /// <param name="startPosition">The starting position of the sequence.</param>
        /// <param name="halfA">The configuration for the first half of the lights.</param>
        /// <param name="halfB">The configuration for the second half of the lights.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="halfA"/> or <paramref name="halfB"/> contain a null color lists.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="halfA"/> or <paramref name="halfB"/> contains less than two colors.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="halfA"/> or <paramref name="halfB"/> contains more than 255 colors.
        /// </exception>
        public void RunRotateHalvesSequence(ushort delay, AccentLightStartPosition startPosition, HalfSequenceConfig halfA, HalfSequenceConfig halfB)
        {
            RunRotateHalvesSequence(delay, startPosition, halfA, halfB, TransitionMode.Immediate);
        }

        /// <summary>
        /// This sequence has two halves that either converge or separate. Each half can have any number of colors.
        /// </summary>
        /// <param name="delay">The number of milliseconds between steps.</param>
        /// <param name="startPosition">The starting position of the sequence.</param>
        /// <param name="halfA">The configuration for the first half of the lights.</param>
        /// <param name="halfB">The configuration for the second half of the lights.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="halfA"/> or <paramref name="halfB"/> contain a null color lists.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="halfA"/> or <paramref name="halfB"/> contains less than two colors.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="halfA"/> or <paramref name="halfB"/> contains more than 255 colors.
        /// </exception>
        public void RunRotateHalvesSequence(ushort delay, AccentLightStartPosition startPosition, HalfSequenceConfig halfA, HalfSequenceConfig halfB, TransitionMode transitionMode)
        {
            Action<string, HalfSequenceConfig> checkHalfSequenceConfig = (parameterName, config) =>
            {
                if(config.Colors == null)
                {
                    throw new ArgumentNullException(parameterName + ".colors");
                }

                if(config.Colors.Count < 2)
                {
                    throw new InsufficientNumberOfColorsProvidedException(2, config.Colors.Count);
                }

                if(config.Colors.Count > 255)
                {
                    throw new ArgumentOutOfRangeException(parameterName + ".colors", "There cannot be more than 255 colors specified.");
                }
            };

            checkHalfSequenceConfig("HalfA", halfA);
            checkHalfSequenceConfig("HalfB", halfB);

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(delay);
                parameters.Add((byte)halfA.Direction);
                parameters.Add((byte)halfB.Direction);
                parameters.Add((byte)halfA.Colors.Count);
                parameters.Add((byte)halfB.Colors.Count);
                parameters.Add((byte)startPosition);

                ProcessColors(parameters, halfA.Colors);
                ProcessColors(parameters, halfB.Colors);

                StartSequence(HaloLightGroupId, RotateHalvesSequenceId, transitionMode, parameters);
            }
        }

        #endregion
    }
}
