//-----------------------------------------------------------------------
// <copyright file = "IPeripheralLights.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Interface for controlling peripheral lights.
    /// </summary>
    public interface IPeripheralLights
    {
        #region Device Enumeration and Acquisition

        /// <summary>
        /// Get a list of the connected light devices.
        /// </summary>
        /// <returns>A description of each of the available light devices.</returns>
        /// <remarks>
        /// UsePropertiesWhereAppropriate is suppressed because this function may have overhead that would be
        /// inconsistent with the expected behavior of properties. 
        /// </remarks>
        IEnumerable<LightFeatureDescription> GetLightDevices();

        /// <summary>
        /// Gets if the device must be acquired before sending commands.
        /// </summary>
        bool RequiresDeviceAcquisition { get; }

        #endregion

        #region Manual Light Control

        /// <summary>
        /// Turn off all of the lights within the specified group.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="transitionMode">The transition mode to use when turning off the group.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        void TurnOffGroup(string featureId, byte groupNumber, TransitionMode transitionMode);

        /// <summary>
        /// Set the brightness for the specified lights in the given group and feature.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="lightStates">List of lights and the desired state of each light.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="InvalidLightException">
        /// Thrown when one of the specified lights is not in the light group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="lightStates"/> is null.
        /// </exception>
        void ControlLightsMonochrome(string featureId, byte groupNumber, IEnumerable<MonochromeLightState> lightStates);

        /// <summary>
        /// Set the brightness a series of lights in the given group and feature.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to adjust the brightness for.</param>
        /// <param name="brightnesses">
        /// A list of desired brightness levels. The first number is the desired brightness for startingLight,
        /// subsequent levels are consecutively applied to the lights after startingLight.
        /// </param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the startingLight, or the brightness list, exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="brightnesses"/> is null.
        /// </exception>
        void ControlLightsMonochrome(string featureId, byte groupNumber, ushort startingLight, IEnumerable<byte> brightnesses);

        /// <summary>
        /// Set the color for the specified lights in the given group and feature.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="lightStates">List of lights and the desired state of each light.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="InvalidLightException">
        /// Thrown when one of the specified lights is not in the light group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="lightStates"/> is null.
        /// </exception>
        void ControlLightsRgb(string featureId, byte groupNumber, IEnumerable<RgbLightState> lightStates);

        /// <summary>
        /// Set the color a series of lights in the given group and feature.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to adjust the color for.</param>
        /// <param name="colors">
        /// List of desired light colors. The first number is the desired color for startingLight, subsequent colors
        /// are consecutively applied to the lights after startingLight.
        /// </param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the startingLight, or the color list, exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="colors"/> is null.
        /// </exception>
        void ControlLightsRgb(string featureId, byte groupNumber, ushort startingLight, IEnumerable<Rgb16> colors);

        #endregion

        #region Sequence Control

        /// <summary>
        /// Start a light sequence on the device.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="sequenceNumber">The number which identifies the desired sequence.</param>
        /// <param name="transitionMode">The transition mode to use when turning off the group.</param>
        /// <param name="parameters">
        /// Byte array containing the parameters for the sequence. If the sequence does not require any parameters,
        /// then null may be passed.
        /// </param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        void StartSequence(string featureId, byte groupNumber, uint sequenceNumber, TransitionMode transitionMode,
                           byte[] parameters);

        /// <summary>
        /// Check if a sequence is running on the device.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="sequenceNumber">The number which identifies the desired sequence.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        bool IsSequenceRunning(string featureId, byte groupNumber, uint sequenceNumber);

        #endregion

        #region Bitwise Light Control

        /// <summary>
        /// Set the bitwise Boolean light state of the specified series of lights.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to control.</param>
        /// <param name="lightStates">The desired on/off state for the starting light and subsequent lights.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the startingLight, or the light states, exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="lightStates"/> is null.
        /// </exception>
        void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                 IEnumerable<bool> lightStates);

        /// <summary>
        /// Set the bitwise light intensity of the specified series of lights.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to control.</param>
        /// <param name="lightIntensities">The desired light intensity for the starting light and subsequent lights.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the startingLight, or the light intensities, exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="lightIntensities"/> is null.
        /// </exception>
        void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                 IEnumerable<BitwiseLightIntensity> lightIntensities);

        /// <summary>
        /// Set the bitwise 4-bit light color of the specified series of lights.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to control.</param>
        /// <param name="lightColors">The desired light color for the starting light and subsequent lights.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the startingLight, or the light color, exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="lightColors"/> is null.
        /// </exception>
        void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                 IEnumerable<BitwiseLightColor> lightColors);

        /// <summary>
        /// Set the bitwise 6-bit light color of the specified series of lights.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to control.</param>
        /// <param name="lightColors">The desired light color for the starting light and subsequent lights.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the startingLight, or the light color, exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="lightColors"/> is null.
        /// </exception>
        void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                 IEnumerable<Rgb6> lightColors);

        /// <summary>
        /// Set the bitwise light state of the specified series of lights.
        /// </summary>
        /// <param name="featureId">The ID of the device being controlled.</param>
        /// <param name="groupNumber">The number of the target light group.</param>
        /// <param name="startingLight">The first light to control.</param>
        /// <param name="bitsPerLight">
        /// The number of bits of control per light.
        ///     <list type="bullet">
        ///         <item>0: Reserved or device specific.</item>
        ///         <item>1: Each bit represents the on/off state of the light.</item>
        ///         <item>2: Two bit light intensity. The BitwiseLightIntensity enumeration lists the potential values.</item>
        ///         <item>3: Reserved or device specific.</item>
        ///         <item>4: Four bit light control. The BitwiseLightColor enumeration lists the potential values.</item>
        ///         <item>5: Reserved or device specific.</item>
        ///         <item>6: Six bit color. Five bits of red, six bits of green, and five bits of blue.</item>
        ///         <item>7-0xFF: Reserved or device specific.</item>
        ///     </list>
        /// </param>
        /// <param name="lightData">The packed data for light control.</param>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the startingLight exceeds the number of lights in the group.
        /// </exception>
        /// <exception cref="InvalidLightStateSizeException">
        /// Thrown when the packed light state data does not match the expected size for the number of lights being
        /// controlled.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when either <paramref name="featureId"/> or <paramref name="lightData"/> is null.
        /// </exception>
        void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                 byte bitsPerLight,
                                 byte[] lightData);

        #endregion
    }
}
