//-----------------------------------------------------------------------
// <copyright file = "UsbLegacyBacklight.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// Represents USB legacy back light hardware.
    /// </summary>
    public class UsbLegacyBacklight : UsbIndividualLightControl
    {
        #region Light Data

        private const byte GroupId = 0;

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
        /// Flashes all lights on and off.
        /// </summary>
        private const uint FlashAllLightsSequenceId = 0x0042;

        /// <summary>
        /// Configures a color flash sequence on the lights
        /// </summary>
        private const ushort ColorFlashSequenceId = 0xC006;

        /// <summary>
        /// Configures a continuous color cross fade cycle.
        /// </summary>
        private const ushort ColorFadeSequenceId = 0xC007;

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        private const ushort FadeToNewColorSequenceId = 0xC00A;

        /// <summary>
        /// Resets the device to its default non-game controlled state.
        /// </summary>
        private const ushort AutonomousModeSequenceId = 0xFFFD;

        #endregion

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a UsbLegacyBacklight.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        public UsbLegacyBacklight(string featureName, LightFeatureDescription featureDescription,
            IPeripheralLights peripheralLights) : base(featureName, featureDescription, peripheralLights)
        {
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Iterates over a list of colors and returns their RGB16 byte representation.
        /// </summary>
        /// <param name="colors">The list of colors to return.</param>
        protected IEnumerable<byte> ConvertPackedColorsToBytes(IList<Color> colors)
        {
            foreach(var color in colors)
            {
                yield return (byte)color.GetRgb16().PackedColor;
            }
        }

        #endregion

        #region LegacyBacklight

        /// <summary>
        /// Sets the brightness of all the lights in the specified group.
        /// </summary>
        /// <param name="brightness">The desired brightness.</param>
        public void SetBrightness(byte brightness)
        {
            SetBrightness(GroupId, brightness);
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
        /// Sets the color of all the lights in the specified group.
        /// </summary>
        /// <param name="color">The desired color.</param>
        public void SetColor(Color color)
        {
            SetColor(GroupId, color);
        }

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

        /// <summary>
        /// Flashes the lights on and off.
        /// </summary>
        public void FlashLightsOnAndOff()
        {
            FlashLightsOnAndOff(GroupId);
        }

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
            RunColorFlash(GroupId, duration, colors, transitionMode);
        }

        /// <summary>
        /// Configures a color flash sequence on the lights. The lights will cycle through the specified colors forever.
        /// </summary>
        /// <param name="groupId">The number of the group to apply the sequence to.</param>
        /// <param name="duration">The amount of time to display each color in 25ms increments.</param>
        /// <param name="colors">The list of colors to cycle through.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups in the peripheral.
        /// Thrown if <paramref name="duration"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="IGT.Game.Core.Presentation.PeripheralLights.InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        internal void RunColorFlash(byte groupId, byte duration, IList<Color> colors, TransitionMode transitionMode)
        {
            if(DeviceAcquired && groupId >= GroupCount && groupId != AllGroups)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId),
                    $"The group ID was {groupId} but the device {HardwareType} reported {GroupCount} groups.");
            }

            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(colors.Count < 2)
            {
                throw new InsufficientNumberOfColorsProvidedException(2, colors.Count);
            }

            if(colors.Count > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(colors), "There cannot be more than 255 colors specified.");
            }

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be 0.");
            }

            if(CanLightCommandBeSent)
            {
                var parameters = new List<byte> {duration, (byte)colors.Count};
                parameters.AddRange(ConvertPackedColorsToBytes(colors));

                StartSequence(groupId, ColorFlashSequenceId, transitionMode, parameters);
            }
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
        public void RunColorFade(ushort duration, ushort transitionTime, IList<Color> colors,
            TransitionMode transitionMode)
        {
            RunColorFade(GroupId, duration, transitionTime, colors, transitionMode);
        }

        /// <summary>
        /// Configures a continuous color cross fade cycle. Similar to RunColorFlash but there is a fade between each color.
        /// </summary>
        /// <param name="groupId">The number of the group to apply the sequence to.</param>
        /// <param name="duration">The number of milliseconds to display each color.</param>
        /// <param name="transitionTime">The number of milliseconds it takes to transition to a new color.</param>
        /// <param name="colors">The list of colors to cycle through.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups in the peripheral.
        /// Thrown if <paramref name="duration"/> is 0.
        /// Thrown if <paramref name="transitionTime"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        internal void RunColorFade(byte groupId, ushort duration, ushort transitionTime, IList<Color> colors,
            TransitionMode transitionMode)
        {
            if(DeviceAcquired && groupId >= GroupCount && groupId != AllGroups)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId),
                    $"The group ID was {groupId} but the device {HardwareType} reported {GroupCount} groups.");
            }

            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be 0.");
            }

            if(colors.Count < 2)
            {
                throw new InsufficientNumberOfColorsProvidedException(2, colors.Count);
            }

            if(colors.Count > 255)
            {
                throw new ArgumentOutOfRangeException(nameof(colors), "There cannot be more than 255 colors specified.");
            }

            if(transitionTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionTime), "The transition time cannot be 0.");
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();

                parameters.Add(duration);
                parameters.Add(transitionTime);
                parameters.Add((byte)colors.Count);
                parameters.AddRange(ConvertPackedColorsToBytes(colors));

                StartSequence(groupId, ColorFadeSequenceId, transitionMode, parameters);
            }
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
            RunFadeToNewColor(GroupId, duration, currentColor, newColor, transitionMode);
        }

        /// <summary>
        /// Fades the lights from one color to a new color and holds at the new color.
        /// </summary>
        /// <param name="groupId">The number of the group to apply the sequence to.</param>
        /// <param name="duration">The number of milliseconds to take for the color transition.</param>
        /// <param name="currentColor">The current color of the lights.</param>
        /// <param name="newColor">The new color of the lights.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups in the peripheral.
        /// Thrown if <paramref name="duration"/> is 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="currentColor"/> equals Color.Empty.
        /// Thrown if <paramref name="newColor"/> equals Color.Empty.
        /// </exception>
        internal void RunFadeToNewColor(byte groupId, ushort duration, Color currentColor, Color newColor,
            TransitionMode transitionMode)
        {
            if(DeviceAcquired && groupId >= GroupCount && groupId != AllGroups)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId),
                    $"The group ID was {groupId} but the device {HardwareType} reported {GroupCount} groups.");
            }

            if(duration == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(duration), "The duration cannot be 0.");
            }

            if(currentColor.IsEmpty)
            {
                throw new ArgumentException("The current color cannot be empty.", nameof(currentColor));
            }

            if(newColor.IsEmpty)
            {
                throw new ArgumentException("The new color cannot be empty.", nameof(newColor));
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(duration);
                parameters.Add(currentColor.GetRgb16().PackedColor);
                parameters.Add(newColor.GetRgb16().PackedColor);

                StartSequence(groupId, FadeToNewColorSequenceId, transitionMode, parameters);
            }
        }

        /// <summary>
        /// Resets the device to its default non-game controlled state.
        /// </summary>
        public void ResetToAutonomousMode()
        {
            StartSequence(AllGroups, AutonomousModeSequenceId, TransitionMode.Immediate, null);
        }

        #endregion
    }
}