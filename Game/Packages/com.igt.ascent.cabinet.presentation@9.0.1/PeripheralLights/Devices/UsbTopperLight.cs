//-----------------------------------------------------------------------
// <copyright file = "UsbTopperLight.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// Represents a USB topper peripheral light hardware.
    /// </summary>
    public class UsbTopperLight : UsbPeripheralLight
    {
        /// <summary>
        /// The default refresh rate.
        /// </summary>
        public const byte DefaultRefreshRate = 0;
        /// <summary>
        /// The default direction of the light pattern.
        /// </summary>
        public const LightDirection DefaultDirection = LightDirection.Clockwise;
        /// <summary>
        /// The hardware constant used to indicate that the colors will be specified in the parameters.
        /// </summary>
        protected const byte CustomColorPalette = 8;
        /// <summary>
        /// The hardware sequence ID for the Suzo Happ topper.
        /// </summary>
        protected const uint TopperSequenceId = 0x009D;

        /// <summary>
        /// Construct a USB topper light
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbTopperLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {

        }

        /// <summary>
        /// Runs one of the Suzo Happ sequences on the topper.
        /// </summary>
        /// <param name="sequence">The light sequence to run.</param>
        /// <param name="colors">The list of colors to use.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="colors"/> is null.</exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 3 colors.
        /// </exception>
        public void RunTopperSequence(SuzoHappSequence sequence, IList<Color> colors)
        {
            RunTopperSequence(sequence, colors, DefaultDirection, DefaultRefreshRate);
        }

        /// <summary>
        /// Runs one of the Suzo Happ sequences on the topper.
        /// </summary>
        /// <param name="sequence">The light sequence to run.</param>
        /// <param name="colors">The list of colors to use.</param>
        /// <param name="direction">The direction the pattern should run.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="colors"/> is null and the sequence is not 'Off'.</exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 3 colors and the sequence is not 'Off'.
        /// </exception>
        public void RunTopperSequence(SuzoHappSequence sequence, IList<Color> colors, LightDirection direction)
        {
            RunTopperSequence(sequence, colors, direction, DefaultRefreshRate);
        }

        /// <summary>
        /// Runs one of the Suzo Happ sequences on the topper.
        /// </summary>
        /// <param name="sequence">The light sequence to run.</param>
        /// <param name="colors">The list of colors to use. There must be at least 3 colors specified unless the sequence is 'off'.</param>
        /// <param name="direction">The direction the pattern should run.</param>
        /// <param name="refreshRate">The rate at which the pattern refreshes/advances.</param>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="colors"/> is null and the sequence is not 'Off'.</exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 3 colors and the sequence is not 'Off'.
        /// </exception>
        public void RunTopperSequence(SuzoHappSequence sequence, IList<Color> colors, LightDirection direction, byte refreshRate)
        {
            if(sequence != SuzoHappSequence.Off)
            {
                if (colors == null)
                {
                    throw new ArgumentNullException(nameof(colors));
                }

                if (colors.Count < 3)
                {
                    throw new InsufficientNumberOfColorsProvidedException(3, colors.Count);
                }
            }

            if(CanLightCommandBeSent && FeatureDescription != null)
            {
                var parameters = new List<byte> { (byte)sequence, refreshRate, (byte)direction, CustomColorPalette };

                if(colors != null)
                {
                    for (var index = 0; index <= 2; index++)
                    {
                        var color = colors[index].GetRgb16();
                        parameters.Add(color.PackedColor);
                    }
                }

                StartSequence(0, TopperSequenceId, TransitionMode.Immediate, parameters);
            }
        }
    }
}
