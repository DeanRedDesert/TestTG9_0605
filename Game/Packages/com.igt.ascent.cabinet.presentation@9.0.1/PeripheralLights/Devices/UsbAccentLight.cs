//-----------------------------------------------------------------------
// <copyright file = "UsbAccentLight.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// The direction the light pattern should move.
    /// </summary>
    /// <remarks>
    /// Design rule is suppressed because this enum represents a hardware data field which is 1 byte.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum AccentLightDirection : byte
    {
        /// <summary>
        /// Move from left to right.
        /// </summary>
        LeftToRight = 0,
        /// <summary>
        /// Move from top to bottom.
        /// </summary>
        TopToBottom = 1,
        /// <summary>
        /// Move from right to left.
        /// </summary>
        RightToLeft = 2,
        /// <summary>
        /// Move from bottom to top.
        /// </summary>
        BottomToTop = 3,
    }

    /// <summary>
    /// The starting position of a brush or sequence.
    /// </summary>
    /// <remarks>
    /// The enum values are explicit since they map to values stored in firmware.
    /// Design rule is suppressed because this enum represents a hardware data field which is 1 byte.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1028:EnumStorageShouldBeInt32")]
    public enum AccentLightStartPosition : byte
    {
        /// <summary>
        /// The right side.
        /// </summary>
        Right = 0,
        /// <summary>
        /// The top right corner.
        /// </summary>
        TopRight = 1,
        /// <summary>
        /// The top side.
        /// </summary>
        Top = 2,
        /// <summary>
        /// The top left corner.
        /// </summary>
        TopLeft = 3,
        /// <summary>
        /// The left side.
        /// </summary>
        Left = 4,
        /// <summary>
        /// The bottom left corner.
        /// </summary>
        BottomLeft = 5,
        /// <summary>
        /// The bottom side.
        /// </summary>
        Bottom = 6,
        /// <summary>
        /// The button right corner.
        /// </summary>
        BottomRight = 7
    }

    /// <summary>
    /// Represents a USB device that implements the accent light sequences.
    /// </summary>
    public class UsbAccentLight : UsbPeripheralLight
    {
        #region Hardware Sequence IDs

        private const ushort PaintSequenceId = 0x00A2;
        private const ushort ColorFlashSequenceId = 0xC006;
        private const ushort ColorFadeSequenceId = 0xC007;
        private const ushort CrossFadeSequenceId = 0xC009;
        private const ushort FadeToNewColorSequenceId = 0xC00A;

        #endregion

        /// <summary>
        /// Construct a USB accent light.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbAccentLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {

        }

        /// <summary>
        /// This sequence simulates a paint brush moving around the button "painting" the lights with a new color as it goes.
        /// After painting the entire button edge the process repeats with a new color.
        /// </summary>
        /// <param name="groupId">The number of the group to apply the sequence to.</param>
        /// <param name="delay">The number of milliseconds between light updates.</param>
        /// <param name="brushes">The brushes use to paint the lights.</param>
        /// <param name="colors">The colors to use with the brushes.</param>
        /// <param name="alternateColors">If true a different color is used for each brush.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups in the peripheral.
        /// Thrown if <paramref name="delay"/> is 0.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        /// Thrown if <paramref name="brushes"/> is null.
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="brushes"/> is not null but contians no brushes.
        /// </exception>
        internal void RunPaintSequence(byte groupId, ushort delay, IList<Paintbrush> brushes, IList<Color> colors,
            bool alternateColors, TransitionMode transitionMode)
        {
            if(DeviceAcquired && groupId >= GroupCount && groupId != AllGroups)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId),
                    $"The group ID was {groupId} but the device {HardwareType} reported {GroupCount} groups.");
            }

            if(delay == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "The delay cannot be 0.");
            }

            if(brushes == null)
            {
                throw new ArgumentNullException(nameof(brushes));
            }
            
            if(brushes.Count == 0)
            {
                throw new ArgumentException("The list of brushes cannot be empty.", nameof(brushes));
            }

            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();

                parameters.Add(delay);
                parameters.Add((byte)(brushes.Count - 1));
                parameters.Add((byte)colors.Count);
                parameters.Add(Convert.ToByte(alternateColors));

                // This should stay a for loop since the order matters when it is sent to the device.
                // ReSharper disable once ForCanBeConvertedToForeach
                for(var index = 0; index < brushes.Count; index++)
                {
                    var brush = brushes[index];
                    var brushData = (byte)brush.StartPosition;
                    if(brush.Direction == LightDirection.Clockwise)
                    {
                        brushData += 8; // Set bit 3
                    }

                    if(brush.AlternateDirection)
                    {
                        brushData += 16; // Set bit 4
                    }

                    parameters.Add(brushData);
                }

                ProcessColors(parameters, colors);
                StartSequence(groupId, PaintSequenceId, transitionMode, parameters);
            }
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
                var parameters = new List<byte> { duration, (byte)colors.Count };
                ProcessColors(parameters, colors);

                StartSequence(groupId, ColorFlashSequenceId, transitionMode, parameters);
            }
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
        internal void RunColorFade(byte groupId, ushort duration, ushort transitionTime, IList<Color> colors, TransitionMode transitionMode)
        {
            if(DeviceAcquired && groupId >= GroupCount && groupId != AllGroups)
            {
                throw new ArgumentOutOfRangeException(nameof(groupId),
                    $"The group ID was {groupId} but the device {GroupCount} reported {HardwareType} groups.");
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

                ProcessColors(parameters, colors);

                StartSequence(groupId, ColorFadeSequenceId, transitionMode, parameters);
            }
        }

        /// <summary>
        /// Cross fades colors in a specified direction over the lights.
        /// </summary>
        /// <param name="groupId">The number of the group to apply the sequence to.</param>
        /// <param name="delay">The number of milliseconds between changing colors.</param>
        /// <param name="direction">The direction the cross fade should go.</param>
        /// <param name="colors">The list of colors to use in the cross fade.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of groups in the peripheral.
        /// Thrown if <paramref name="delay"/> is 0.
        /// Thrown if <paramref name="colors"/> contains more than 255 colors.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        /// <exception cref="InsufficientNumberOfColorsProvidedException">
        /// Thrown if <paramref name="colors"/> contains less than 2 colors.
        /// </exception>
        internal void RunCrossFade(byte groupId, ushort delay, AccentLightDirection direction, IList<Color> colors, TransitionMode transitionMode)
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

            if(delay == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(delay), "The delay cannot be 0.");
            }

            if(colors.Count < 2)
            {
                throw new InsufficientNumberOfColorsProvidedException(2, colors.Count);
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
                parameters.Add((byte)colors.Count);

                ProcessColors(parameters, colors);
                StartSequence(groupId, CrossFadeSequenceId, transitionMode, parameters);
            }
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
        internal void RunFadeToNewColor(byte groupId, ushort duration, Color currentColor, Color newColor, TransitionMode transitionMode)
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
        /// Iterates over a list of colors and adds their RGB16 representation to the parameter list.
        /// </summary>
        /// <param name="parameters">The list of parameters.</param>
        /// <param name="colors">The list of colors to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="colors"/> is null.
        /// </exception>
        protected static void ProcessColors(IList<byte> parameters, IList<Color> colors)
        {
            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }

            for(var index = 0; index < colors.Count; index++)
            {
                parameters.Add(colors[index].GetRgb16().PackedColor);
            }
        }
    }
}
