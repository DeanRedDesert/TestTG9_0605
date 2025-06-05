//-----------------------------------------------------------------------
// <copyright file = "UsbButtonEdgeLight.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas.Internal;

    /// <summary>
    /// Represents a USB button edge peripheral light hardware.
    /// </summary>
    public class UsbButtonEdgeLight : UsbAccentLight
    {
        #region Buttons

        /// <summary>
        /// The different button panel buttons.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
        public enum Button : byte
        {
            /// <summary>
            /// The first line button.
            /// </summary>
            Line1,
            /// <summary>
            /// The second line button.
            /// </summary>
            Line2,
            /// <summary>
            /// The third line button.
            /// </summary>
            Line3,
            /// <summary>
            /// The fourth line button.
            /// </summary>
            Line4,
            /// <summary>
            /// The fifth line button.
            /// </summary>
            Line5,
            /// <summary>
            /// The first bet button.
            /// </summary>
            Bet1,
            /// <summary>
            /// The second bet button.
            /// </summary>
            Bet2,
            /// <summary>
            /// The third bet button.
            /// </summary>
            Bet3,
            /// <summary>
            /// The fourth bet button.
            /// </summary>
            Bet4,
            /// <summary>
            /// The fifth bet button.
            /// </summary>
            Bet5,
            /// <summary>
            /// The double up button.
            /// </summary>
            DoubleUp,
            /// <summary>
            /// The bet one button.
            /// </summary>
            BetOne,
            /// <summary>
            /// The max bet button.
            /// </summary>
            MaxBet,
            /// <summary>
            /// The repeat the bet button.
            /// </summary>
            RepeatBet,
            /// <summary>
            /// All buttons.
            /// </summary>
            All,
        }

        /// <summary>
        /// Maps the button enum to the hardware ID of the button.
        /// </summary>
        private static readonly Dictionary<Button, byte> ButtonToButtonId = new Dictionary<Button, byte>
        {
            { Button.Line1, ButtonPanelButtonId.SelectNLines1 },
            { Button.Line2, ButtonPanelButtonId.SelectNLines2 },
            { Button.Line3, ButtonPanelButtonId.SelectNLines3 },
            { Button.Line4, ButtonPanelButtonId.SelectNLines4 },
            { Button.Line5, ButtonPanelButtonId.SelectNLines5 },
            { Button.Bet1, ButtonPanelButtonId.PlayNCredit1 },
            { Button.Bet2, ButtonPanelButtonId.PlayNCredit2 },
            { Button.Bet3, ButtonPanelButtonId.PlayNCredit3 },
            { Button.Bet4, ButtonPanelButtonId.PlayNCredit4 },
            { Button.Bet5, ButtonPanelButtonId.PlayNCredit5 },
            { Button.RepeatBet, ButtonPanelButtonId.RepeatTheBet },
            { Button.MaxBet, ButtonPanelButtonId.MaxBet },
            { Button.BetOne, ButtonPanelButtonId.BetOne },
            { Button.DoubleUp, ButtonPanelButtonId.DoubleUp },
            { Button.All, AllGroups },
        };

        /// <summary>
        /// The list of buttons available on the button panel.
        /// </summary>
        private static readonly List<byte> AvailableButtons = new List<byte>();

        #endregion

        #region Hardware Sequence IDs

        private const ushort ButtonPressSequenceId = 0xC000;
        private const ushort TivoliSingleColorSequenceId = 0xC001;
        private const ushort TivoliDoubleColorSequenceId = 0xC002;
        private const ushort TwoColorRotationSequenceId = 0xC004;
        private const ushort FourColorRotationSequenceId = 0xC005;
        private const ushort AutonomousModeSequenceId = 0xFFFD;

        #endregion

        /// <summary>
        /// Construct a USB button edge light.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbButtonEdgeLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {
            if(GroupCount > 0)
            {
                GroupCount = (ushort)(GroupCount + 1); // Add 1 to the group count since the foundation is 1 based with the buttons.
            }
        }

        /// <inheritdoc />
        internal override void SetFeatureDescription(LightFeatureDescription featureDescription)
        {
            base.SetFeatureDescription(featureDescription);
            if(GroupCount > 0)
            {
                GroupCount = (ushort)(GroupCount + 1); // Add 1 to the group count since the foundation is 1 based with the buttons.
            }
        }

        /// <summary>
        /// Gets if the button panel configuration has been set or not.
        /// </summary>
        internal static bool ButtonPanelConfigurationSet
        {
            get;
            set;
        }

        #region Accent Light Functions

        /// <summary>
        /// This sequence simulates a paint brush moving around the button "painting" the lights with a new color as it goes.
        /// After painting the entire button edge the process repeats with a new color.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
        /// <param name="delay">The number of milliseconds between light updates.</param>
        /// <param name="brushes">The brushes use to paint the lights.</param>
        /// <param name="colors">The colors to use with the brushes.</param>
        /// <param name="alternateColors">If true a different color is used for each brush.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="brushes"/> is null.
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="brushes"/> is not null but contains no brushes.
        /// </exception>
        public void RunPaintSequence(Button button, ushort delay, IList<Paintbrush> brushes, IList<Color> colors, bool alternateColors, TransitionMode transitionMode = TransitionMode.Immediate)
        {
            if(AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                RunPaintSequence(ButtonToButtonId[button], delay, brushes, colors, alternateColors, transitionMode);
            }
        }

        /// <summary>
        /// Configures a color flash sequence on the lights. The lights will cycle through the specified colors forever.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
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
        public void RunColorFlash(Button button, byte duration, IList<Color> colors, TransitionMode transitionMode = TransitionMode.Immediate)
        {
            if(AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                RunColorFlash(ButtonToButtonId[button], duration, colors, transitionMode);
            }
        }

        /// <summary>
        /// Configures a continuous color cross fade cycle. Similar to RunColorFlash but there is a fade between each color.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
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
        public void RunColorFade(Button button, ushort duration, ushort transitionTime, IList<Color> colors, TransitionMode transitionMode = TransitionMode.Immediate)
        {
            if(AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                RunColorFade(ButtonToButtonId[button], duration, transitionTime, colors, transitionMode);
            }
        }

        /// <summary>
        /// Cross fades colors in a specified direction over the lights.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
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
        public void RunCrossFade(Button button, ushort delay, AccentLightDirection direction, IList<Color> colors, TransitionMode transitionMode = TransitionMode.Immediate)
        {
            if(AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                RunCrossFade(ButtonToButtonId[button], delay, direction, colors, transitionMode);
            }
        }

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
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
        public void RunFadeToNewColor(Button button, ushort duration, Color currentColor, Color newColor, TransitionMode transitionMode = TransitionMode.Immediate)
        {
            if(AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                RunFadeToNewColor(ButtonToButtonId[button], duration, currentColor, newColor, transitionMode);
            }
        }

        #endregion Accent Light Functions

        /// <summary>
        /// This sequence tracks with the button's switch status. When the button is pressed the color cross fades to a new color and stays there
        /// until the button is released. At which point the lights return to the idle color.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
        /// <param name="fadeTime">The time in milliseconds to take to fade between the two colors.</param>
        /// <param name="idleColor">The color of the lights when the button is not pressed.</param>
        /// <param name="activeColor">The color of the lights when the button is pressed.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="idleColor"/> is equal to Color.Empty.
        /// Thrown if <paramref name="activeColor"/> is equal to Color.Empty.
        /// </exception>
        public void ConfigureButtonPressSequence(Button button, ushort fadeTime, Color idleColor, Color activeColor)
        {
            if(idleColor.IsEmpty)
            {
                throw new ArgumentException("The idle color cannot be empty.", nameof(idleColor));
            }

            if(activeColor.IsEmpty)
            {
                throw new ArgumentException("The active color cannot be empty.", nameof(activeColor));
            }

            if(AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(idleColor.GetRgb16().PackedColor);
                parameters.Add(activeColor.GetRgb16().PackedColor);
                parameters.Add(fadeTime);

                StartSequence(ButtonToButtonId[button], ButtonPressSequenceId, TransitionMode.Immediate, parameters);
            }
        }

        #region Tivoli Sequence

        /// <summary>
        /// Circles one set of lights around a button.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
        /// <param name="rotationTime">The number of milliseconds it takes the lights to do a full rotation.</param>
        /// <param name="direction">The direction the lights should rotate.</param>
        /// <param name="backgroundColor">The background color of the lights.</param>
        /// <param name="color">The color of the first set of lights.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="backgroundColor"/> is equal to Color.Empty.
        /// Thrown if <paramref name="color"/> is equal to Color.Empty.
        /// </exception>
        public void RunTivoliSequence(Button button, ushort rotationTime, LightDirection direction, Color backgroundColor, Color color, TransitionMode transitionMode = TransitionMode.Immediate)
        {
            RunTivoliSequence(button, rotationTime, direction, backgroundColor, color, Color.Empty, transitionMode);
        }

        /// <summary>
        /// Circles two sets of lights around a button.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
        /// <param name="rotationTime">The number of milliseconds it takes the lights to do a full rotation.</param>
        /// <param name="direction">The direction the lights should rotate.</param>
        /// <param name="backgroundColor">The background color of the lights.</param>
        /// <param name="color">The color of the first set of lights.</param>
        /// <param name="color2">The color of the second set of lights.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="backgroundColor"/> is equal to Color.Empty.
        /// Thrown if <paramref name="color"/> is equal to Color.Empty.
        /// </exception>
        public void RunTivoliSequence(Button button, ushort rotationTime, LightDirection direction, Color backgroundColor, Color color, Color color2, TransitionMode transitionMode = TransitionMode.Immediate)
        {
            if(backgroundColor == Color.Empty)
            {
                throw new ArgumentException("The background color cannot be empty.", nameof(backgroundColor));
            }

            if(color == Color.Empty)
            {
                throw new ArgumentException("The first color cannot be empty.", nameof(color));
            }

            if(DeviceAcquired && AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                var sequenceId = color2 == Color.Empty ? TivoliSingleColorSequenceId : TivoliDoubleColorSequenceId;

                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(backgroundColor.GetRgb16().PackedColor);
                parameters.Add(color.GetRgb16().PackedColor);

                if(sequenceId == TivoliDoubleColorSequenceId)
                {
                    parameters.Add(color2.GetRgb16().PackedColor);
                }
                parameters.Add(rotationTime);
                parameters.Add((byte)direction);

                StartSequence(ButtonToButtonId[button], sequenceId, transitionMode, parameters);
            }
        }

        #endregion

        #region Color Rotation Sequence

        /// <summary>
        /// Rotates two or four colors around the button.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
        /// <param name="rotationTime">The number of milliseconds it takes to complete a full rotation.</param>
        /// <param name="direction">The direction of the rotation.</param>
        /// <param name="colors">The list of colors to rotate, this should be a list of either 2 or 4 colors.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="colors"/> contains more than 4 colors.
        /// </exception>
        /// <remarks>
        /// The sequence that is ran is automatically determined based on the number of colors provided in <paramref name="colors"/>. Any colors
        /// after the first four are ignored.
        /// </remarks>
        public void RunColorRotation(Button button, ushort rotationTime, LightDirection direction, IList<Color> colors)
        {
            RunColorRotation(button, rotationTime, direction, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// Rotates two or four colors around the button.
        /// </summary>
        /// <param name="button">The button to apply the sequence to.</param>
        /// <param name="rotationTime">The number of milliseconds it takes to complete a full rotation.</param>
        /// <param name="direction">The direction of the rotation.</param>
        /// <param name="colors">The list of colors to rotate, this should be a list of either 2 or 4 colors.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="colors"/> contains more than 4 colors.
        /// </exception>
        /// <remarks>
        /// The sequence that is ran is automatically determined based on the number of colors provided in <paramref name="colors"/>. Any colors
        /// after the first four are ignored.
        /// </remarks>
        public void RunColorRotation(Button button, ushort rotationTime, LightDirection direction, IList<Color> colors, TransitionMode transitionMode)
        {
            const int maximumAllowedColors = 4;

            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(colors.Count < 2)
            {
                throw new InsufficientNumberOfColorsProvidedException(2, colors.Count);
            }

            if(colors.Count > maximumAllowedColors)
            {
                throw new ArgumentException(
                    $"This sequence only supports up to {maximumAllowedColors} colors.", nameof(colors));
            }

            if(DeviceAcquired && AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(rotationTime);
                parameters.Add((byte)direction);

                var sequenceId = TwoColorRotationSequenceId;
                if(colors.Count == maximumAllowedColors)
                {
                    sequenceId = FourColorRotationSequenceId;
                }

                ProcessColors(parameters, colors);
                StartSequence(ButtonToButtonId[button], sequenceId, transitionMode, parameters);
            }
        }

        #endregion

        /// <summary>
        /// Sets a button light to a specific solid color.
        /// </summary>
        /// <param name="button">The button to change.</param>
        /// <param name="color">The color to set the lights to.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="color"/> is empty.
        /// </exception>
        public void SetColor(Button button, Color color)
        {
            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(color));
            }

            if(CanLightCommandBeSent && AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                var lightState = new RgbLightState(0xFFFF, color.GetRgb16());

                try
                {
                    LightInterface.ControlLightsRgb(FeatureName, ButtonToButtonId[button], new List<RgbLightState> { lightState });
                }
                catch(LightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the on/off state for a button.
        /// </summary>
        /// <param name="button">The button to change.</param>
        /// <param name="lightOn">If the lights should be on or off.</param>
        public void SetLightState(Button button, bool lightOn)
        {
            if(CanLightCommandBeSent && AvailableButtons.Contains(ButtonToButtonId[button]))
            {
                try
                {
                    if(lightOn)
                    {
                        LightInterface.BitwiseLightControl(FeatureName, ButtonToButtonId[button], 0xFFFF, new List<bool> { true });
                    }
                    else
                    {
                        LightInterface.TurnOffGroup(FeatureName, ButtonToButtonId[button], TransitionMode.Immediate);
                    }

                }
                catch(LightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Resets the device to its default non-game controlled state.
        /// </summary>
        public void ResetToAutonomousMode()
        {
            StartSequence(AllGroups, AutonomousModeSequenceId, TransitionMode.Immediate, null);
        }

        #region Overrides of Object

        /// <inheritdoc />
        public override string ToString()
        {
            var text = new StringBuilder(base.ToString());
            text.AppendLine($"Panel Configuration Set: {ButtonPanelConfigurationSet}");
            text.Append("Available Buttons: ");
            foreach(var button in AvailableButtons)
            {
                text.AppendFormat("{0}, ", button);
            }
            text.AppendLine();

            return text.ToString();
        }

        #endregion

        /// <summary>
        /// Sets the button panel configuration from the cabinet interface.
        /// </summary>
        /// <param name="cabinetLib">The cabinet interface to use.</param>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="cabinetLib"/> is null.
        /// </exception>
        /// <remarks>
        /// If this function isn't called none of the sequences will run because it will
        /// appear as if the button panel has no buttons.
        /// </remarks>
        internal static void SetButtonPanelConfiguration(ICabinetLib cabinetLib)
        {
            if(cabinetLib == null)
            {
                throw new ArgumentNullException(nameof(cabinetLib));
            }

            AvailableButtons.Clear();
            var panelConfiguration = cabinetLib.GetButtonPanelConfigurations().FirstOrDefault();
            if(panelConfiguration?.Buttons != null)
            {
                AvailableButtons.Add(AllGroups); // Add the all groups ID so Button.All works.
                panelConfiguration.Buttons.ToList().ForEach(button => AvailableButtons.Add(button.ButtonId));
            }
            ButtonPanelConfigurationSet = true;
        }

        /// <summary>
        /// Clears the stored button panel configuration.
        /// </summary>
        internal static void ClearButtonPanelConfiguration()
        {
            AvailableButtons.Clear();
            ButtonPanelConfigurationSet = false;
        }

        /// <inheritdoc />
        protected override bool ShouldLightCategoryErrorBeReported(Exception exception)
        {
            var lightCategoryException = exception as LightCategoryException;

            // Ignore invalid group and feature ID error codes because there are frequent false
            // positives due to Foundation bugs with the VBP.
            return base.ShouldLightCategoryErrorBeReported(exception) &&
                   lightCategoryException != null &&
                   lightCategoryException.ErrorCode != LightErrorCode.INVALID_GROUP.ToString() &&
                   lightCategoryException.ErrorCode != LightErrorCode.INVALID_FEATURE_ID.ToString();
        }
    }
}
