//-----------------------------------------------------------------------
// <copyright file = "UsbHandleLight.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// Represents the lights on a cabinet pull handle.
    /// </summary>
    public class UsbHandleLight : UsbAccentLight
    {
        #region Hardware Sequence IDs

        private const ushort CustomChaseSequenceId = 0xC00B;
        private const ushort DualFadeSequenceId = 0xC00D;
        private const ushort HandlePulseSequenceId = 0xC00E;
        private const ushort HandleChaseSequenceId = 0xC00F;

        #endregion

        #region Device Constants

        private const byte HandleGroupId = 0;
        private const int NumberOfHandleLeds = 7;
        private const int MaximumNumberOfColors = 16;

        #endregion

        /// <summary>
        /// Construct a USB handle light.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbHandleLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {

        }

        /// <summary>
        /// Sets all the lights to a single color.
        /// </summary>
        /// <param name="color">The color to set the lights to.</param>
        public void SetColor(Color color)
        {
            SetColor(HandleGroupId, color);
        }

        /// <summary>
        /// Sets all the lights to a single color,
        /// and all the lights on the blank light devices to a universal color.
        /// </summary>
        /// <param name="color">The light color to set the lights to.</param>
        /// <param name="universalColor">The universal color to set the blank light devices to.</param>
        public void SetColor(Color color, Color universalColor)
        {
            SetColor(HandleGroupId, color, universalColor);
        }

        #region Accent Light Sequences

        /// <summary>
        /// This sequence simulates a paint brush moving over the handle "painting" the lights with a new color as it goes.
        /// After painting the entire button edge the process repeats with a new color.
        /// </summary>
        /// <param name="delay">The number of milliseconds between light updates.</param>
        /// <param name="brushes">The brushes use to paint the lights.</param>
        /// <param name="colors">The colors to use with the brushes.</param>
        /// <param name="alternateColors">If true a different color is used for each brush.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="brushes"/> is null.
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="brushes"/> is not null but contians no brushes.
        /// </exception>
        public void RunPaintSequence(ushort delay, IList<Paintbrush> brushes, IList<Color> colors,
            bool alternateColors)
        {
            RunPaintSequence(delay, brushes, colors, alternateColors, TransitionMode.Immediate);
        }

        /// <summary>
        /// This sequence simulates a paint brush moving over the handle "painting" the lights with a new color as it goes.
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
        /// Thrown if <paramref name="brushes"/> is not null but contians no brushes.
        /// </exception>
        public void RunPaintSequence(ushort delay, IList<Paintbrush> brushes, IList<Color> colors,
            bool alternateColors, TransitionMode transitionMode)
        {
            RunPaintSequence(HandleGroupId, delay, brushes, colors, alternateColors, transitionMode);
        }

        /// <summary>
        /// Configures a color flash sequence on the lights. The lights will cycle through the specified colors forever.
        /// </summary>
        /// <param name="duration">The amount of time to display each color in 25ms increments.</param>
        /// <param name="colors">The list of colors to cycle through.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="duration"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
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
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunColorFlash(byte duration, IList<Color> colors, TransitionMode transitionMode)
        {
            // The base function will handle the null exception.
            // This device has tighter limits on the number of colors that can be used.
            if(colors != null && colors.Count > MaximumNumberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(colors),
                    $"The list of colors contains {colors.Count} colors but the maximum allowed is {MaximumNumberOfColors}.");
            }

            RunColorFlash(HandleGroupId, duration, colors, transitionMode);
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
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
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
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunColorFade(ushort duration, ushort transitionTime, IList<Color> colors, TransitionMode transitionMode)
        {
            // The base function will handle the null exception.
            // This device has tighter limits on the number of colors that can be used.
            if(colors != null && colors.Count > MaximumNumberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(colors),
                    $"The list of colors contains {colors.Count} colors but the maximum allowed is {MaximumNumberOfColors}.");
            }

            RunColorFade(HandleGroupId, duration, transitionTime, colors, transitionMode);
        }

        /// <summary>
        /// Cross fades colors in a specified direction over the lights.
        /// </summary>
        /// <param name="delay">The number of milliseconds between changing colors.</param>
        /// <param name="direction">The direction the cross fade should go.</param>
        /// <param name="colors">The list of colors to use in the cross fade.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="delay"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
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
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        public void RunCrossFade(ushort delay, AccentLightDirection direction, IList<Color> colors, TransitionMode transitionMode)
        {
            // The base function will handle the null exception.
            // This device has tighter limits on the number of colors that can be used.
            if(colors != null && colors.Count > MaximumNumberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(colors),
                    $"The list of colors contains {colors.Count} colors but the maximum allowed is {MaximumNumberOfColors}.");
            }

            RunCrossFade(HandleGroupId, delay, direction, colors, transitionMode);
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
            RunFadeToNewColor(HandleGroupId, duration, currentColor, newColor, transitionMode);
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
        /// Thrown if the numbers of colors in <paramref name="colors"/> is greater than 16.
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
        /// Thrown if the numbers of colors in <paramref name="colors"/> is greater than 16.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 1 color.
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
            
            if(colors.Count > MaximumNumberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(colors),
                    $"The list of colors contains {colors.Count} colors but the maximum allowed is {MaximumNumberOfColors}.");
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

                StartSequence(HandleGroupId, CustomChaseSequenceId, transitionMode, parameters);
            }
        }

        #endregion

        #region Dual Fade Sequence

        /// <summary>
        /// A host defined number of segments are alternately cross-faded between two
        /// predefined colors.
        /// </summary>
        /// <param name="transitionTime">The number of milliseconds between color transitions.</param>
        /// <param name="holdTime">
        /// The number of milliseconds it takes to display fully transitioned colors.
        /// </param>
        /// <param name="numberOfSegments">The number of segments (2 to 16).</param>
        /// <param name="colors">The predefined colors for the fade.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="transitionTime"/> or <paramref name="holdTime"/> are 0.
        /// Thrown if <paramref name="numberOfSegments"/> is out of range.
        /// </exception>
        /// <example>
        /// <![CDATA[
        /// lightObject.RunDualFadeSequence(100, 250, 8, DualFadeSequenceColor.RedBlue);
        /// ]]>
        /// </example>
        public void RunDualFadeSequence(ushort transitionTime, ushort holdTime,
            byte numberOfSegments, DualFadeSequenceColor colors)
        {
            RunDualFadeSequence(transitionTime, holdTime, numberOfSegments, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// A host defined number of segments are alternately cross-faded between two
        /// predefined colors.
        /// </summary>
        /// <param name="transitionTime">The number of milliseconds between color transitions.</param>
        /// <param name="holdTime">
        /// The number of milliseconds it takes to display fully transitioned colors.
        /// </param>
        /// <param name="numberOfSegments">The number of segments (2 to 16).</param>
        /// <param name="colors">The predefined colors for the fade.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="transitionTime"/> or <paramref name="holdTime"/> are 0.
        /// Thrown if <paramref name="numberOfSegments"/> is out of range.
        /// </exception>
        /// <example>
        /// <![CDATA[
        /// lightObject.RunDualFadeSequence(100, 250, 8, DualFadeSequenceColor.RedBlue, TransitionMode.Immediate);
        /// ]]>
        /// </example>
        public void RunDualFadeSequence(ushort transitionTime, ushort holdTime,
            byte numberOfSegments, DualFadeSequenceColor colors, TransitionMode transitionMode)
        {
            if(transitionTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionTime),
                    "The transition time cannot be 0.");
            }

            if(holdTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(holdTime),
                    "The hold time cannot be 0.");
            }

            if(numberOfSegments < 2 || numberOfSegments > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfSegments),
                    "The number of segments must be from 2 to 16.");
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(transitionTime);
                parameters.Add(holdTime);
                parameters.Add(numberOfSegments);
                parameters.Add((byte)colors);

                StartSequence(HandleGroupId, DualFadeSequenceId, transitionMode,
                    parameters);
            }
        }

        #endregion

        #region Handle Pulse Sequence

        /// <summary>
        /// Pulses the handle lights by cross fading between a list of colors while the handle is being pulled.
        /// </summary>
        /// <param name="colors">The list of up to 16 colors to cross fade between.</param>
        /// <param name="setDefaultColor">
        /// If true the handle will return to the first color in the list when the sequence is over.
        /// If false the handle will stay at the last color in the list when the sequence is over.
        /// </param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
        /// </exception>
        /// <exception cref="InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than two colors.
        /// </exception>
        public void RunHandlePulseSequence(IList<Color> colors, bool setDefaultColor)
        {
            RunHandlePulseSequence(colors, setDefaultColor, TransitionMode.Immediate);
        }

        /// <summary>
        /// Pulses the handle lights by cross fading between a list of colors while the handle is being pulled.
        /// </summary>
        /// <param name="colors">The list of up to 16 colors to cross fade between.</param>
        /// <param name="setDefaultColor">
        /// If true the handle will return to the first color in the list when the sequence is over.
        /// If false the handle will stay at the last color in the list when the sequence is over.
        /// </param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="colors"/> contains more than 16 colors.
        /// </exception>
        /// <exception cref="InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than two colors.
        /// </exception>
        public void RunHandlePulseSequence(IList<Color> colors, bool setDefaultColor, TransitionMode transitionMode)
        {
            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(colors.Count > MaximumNumberOfColors)
            {
                throw new ArgumentOutOfRangeException(nameof(colors),
                    $"The list of colors contains {colors.Count} colors but the maximum allowed is {MaximumNumberOfColors}.");
            }

            if(colors.Count < 2)
            {
                throw new InsufficientNumberOfColorsProvidedException(2, colors.Count);
            }

            if(CanLightCommandBeSent)
            {
                var parameters = new List<byte> {(byte)(setDefaultColor ? 1 : 0), Convert.ToByte(colors.Count)};
                ProcessColors(parameters, colors);

                StartSequence(HandleGroupId, HandlePulseSequenceId, transitionMode, parameters);
            }
        }

        #endregion

        #region Handle Chase Sequence

        /// <summary>
        /// Divides the handle lights between the requested colors. Lights chase downward during the downward portion of a
        /// handle pull and upward as the handle returns to its rest position.
        /// </summary>
        /// <param name="stepDelay">The time delay between steps in milliseconds.</param>
        /// <param name="colors">The colors to display. The number of colors cannot exceed 7.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="stepDelay"/> is 0 or if <paramref name="colors"/> has more than 7 colors in it.
        /// </exception>
        /// <exception cref="InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than one color.
        /// </exception>
        public void RunHandleChaseSequence(ushort stepDelay, IList<Color> colors)
        {
            RunHandleChaseSequence(stepDelay, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// Divides the handle lights between the requested colors. Lights chase downward during the downward portion of a
        /// handle pull and upward as the handle returns to its rest position.
        /// </summary>
        /// <param name="stepDelay">The time delay between steps in milliseconds.</param>
        /// <param name="colors">The colors to display. The number of colors cannot exceed 7.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="stepDelay"/> is 0 or if <paramref name="colors"/> has more than 7 colors in it.
        /// </exception>
        /// <exception cref="InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than one color.
        /// </exception>
        public void RunHandleChaseSequence(ushort stepDelay, IList<Color> colors, TransitionMode transitionMode)
        {
            if(stepDelay == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stepDelay), "The step delay cannot be zero.");
            }

            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(colors.Count == 0)
            {
                throw new InsufficientNumberOfColorsProvidedException(1, colors.Count);
            }

            if(colors.Count > NumberOfHandleLeds)
            {
                throw new ArgumentOutOfRangeException(nameof(colors),
                    $"{colors.Count} colors were provided however the maximum number allowed is {NumberOfHandleLeds}.");
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(stepDelay);
                parameters.Add(Convert.ToByte(colors.Count));
                ProcessColors(parameters, colors);

                StartSequence(HandleGroupId, HandleChaseSequenceId, transitionMode, parameters);
            }
        }

        #endregion
    }
}
