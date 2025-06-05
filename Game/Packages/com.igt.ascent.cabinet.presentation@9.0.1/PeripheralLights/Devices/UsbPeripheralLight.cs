//-----------------------------------------------------------------------
// <copyright file = "UsbPeripheralLight.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas.Internal;

    /// <summary>
    /// This class represents a USB light device that
    /// is controlled via the IPeripheralLights interface.
    /// </summary>
    public class UsbPeripheralLight : UsbLightBase
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="UsbPeripheralLight"/>.
        /// </summary>
        /// <param name="featureName">The feature name of the light device.</param>
        /// <param name="featureDescription">The feature description of the light device.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="peripheralLights"/> is null.
        /// </exception>
        internal UsbPeripheralLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription)
        {
            LightInterface = peripheralLights ?? throw new ArgumentNullException(nameof(peripheralLights));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the interface used to control the peripheral lights.
        /// </summary>
        protected internal IPeripheralLights LightInterface { get; internal set; }

        /// <summary>
        /// Gets the flag indicating if a light command can be sent over the interface or not.
        /// </summary>
        protected bool CanLightCommandBeSent => DeviceAcquired || !LightInterface.RequiresDeviceAcquisition;

        #endregion

        #region UsbLightBase Overrides

        /// <inheritdoc/>
        /// <exception cref="LightCategoryException">
        /// Thrown if an error occurs when setting the color.
        /// </exception>
        public override void SetColor(byte groupId, Color color)
        {
            ValidateGroupId(groupId, true);

            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(color));
            }

            if(CanLightCommandBeSent)
            {
                var lightState = new RgbLightState(0xFFFF, color.GetRgb16());

                try
                {
                    LightInterface.ControlLightsRgb(FeatureName, groupId, new List<RgbLightState> { lightState });
                }
                catch(LightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }

                FlagClientContentPresent();
            }
        }

        /// <inheritdoc/>
        /// <exception cref="LightCategoryException">
        /// Thrown if an error occurs when setting the light state.
        /// </exception>
        public override void SetLightState(byte groupId, bool lightOn)
        {
            ValidateGroupId(groupId, true);

            if(CanLightCommandBeSent)
            {
                try
                {
                    if(lightOn)
                    {
                        LightInterface.BitwiseLightControl(FeatureName, groupId, 0xFFFF, new List<bool> { true });
                    }
                    else
                    {
                        LightInterface.TurnOffGroup(FeatureName, groupId, TransitionMode.Immediate);
                    }

                }
                catch(LightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }

                // SetLightState is not counted as client light content.
                // Do not set the client content present flag.
            }
        }

        /// <inheritdoc/>
        internal override void SetUniversalColor(Color universalColor)
        {
            SettingNonClientContent = true;

            SetColor(AllGroups, universalColor);

            SettingNonClientContent = false;
        }

        /// <inheritdoc/>
        protected override bool ShouldLightCategoryErrorBeReported(Exception exception)
        {
            return exception is LightCategoryException lightCategoryException &&
                   lightCategoryException.ErrorCode != LightErrorCode.CLIENT_DOES_NOT_OWN_RESOURCE.ToString() &&
                   lightCategoryException.ErrorCode != LightErrorCode.UNKNOWN_DRIVER_ERROR.ToString();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines if a specified sequence is running or not.
        /// </summary>
        /// <param name="groupId">The light group id.</param>
        /// <param name="sequenceId">The sequence id to check for. The AllGroups group cannot be used here.</param>
        /// <returns>
        /// True if the sequence specified by <paramref name="sequenceId"/> is running; False otherwise.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupId"/> is larger than the number of light groups on the peripheral.
        /// </exception>
        public bool IsSequenceRunning(byte groupId, ushort sequenceId)
        {
            ValidateGroupId(groupId);

            return DeviceAcquired && LightInterface.IsSequenceRunning(FeatureName, groupId, sequenceId);
        }

        /// <summary>
        /// Starts a sequence on the light peripheral and suppresses any errors if the device is not acquired.
        /// </summary>
        /// <param name="groupNumber">The number of the group to apply the sequence to.</param>
        /// <param name="sequenceNumber">The number which identifies the desired sequence.</param>
        /// <param name="transitionMode">The transition mode to use when turning off the group.</param>
        /// <param name="parameters">
        /// Byte list containing the parameters for the sequence. If the sequence does not require any parameters,
        /// then null may be passed.
        /// </param>
        /// <exception cref="LightCategoryException">
        /// Thrown if an error occurs when starting the sequence.
        /// </exception>
        protected void StartSequence(byte groupNumber, uint sequenceNumber, TransitionMode transitionMode,
                                     IList<byte> parameters)
        {
            try
            {
                if(CanLightCommandBeSent)
                {
                    LightInterface.StartSequence(FeatureName, groupNumber, sequenceNumber, transitionMode,
                                                 parameters?.ToArray());
                }
            }
            catch(LightCategoryException ex)
            {
                if(ShouldLightCategoryErrorBeReported(ex))
                {
                    throw;
                }
            }

            FlagClientContentPresent();
        }

        /// <summary>
        /// Set the color for the specified lights in the given group and feature.
        /// </summary>
        /// <param name="groupId">The ID of the target light group.</param>
        /// <param name="lightStates">List of lights and the desired state of each light.</param>
        public void ControlLightsRgb(byte groupId, IEnumerable<RgbLightState> lightStates)
        {
            ValidateGroupId(groupId);

            if(CanLightCommandBeSent)
            {
                try
                {
                    LightInterface.ControlLightsRgb(FeatureName, groupId, lightStates);
                }
                catch(LightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }

                FlagClientContentPresent();
            }
        }

        /// <summary>
        /// Set the color a series of lights in the given group and feature.
        /// </summary>
        /// <param name="groupId">The ID of the target light group.</param>
        /// <param name="startingLight">The first light to adjust the color for.</param>
        /// <param name="colors">
        /// List of desired light colors. The first number is the desired color for startingLight, subsequent colors
        /// are consecutively applied to the lights after startingLight.
        /// </param>
        public void ControlLightsRgb(byte groupId, ushort startingLight, IEnumerable<Color> colors)
        {
            ValidateGroupId(groupId);

            if(CanLightCommandBeSent)
            {
                try
                {
                    LightInterface.ControlLightsRgb(FeatureName, groupId, startingLight,
                        colors.Select(color => color.GetRgb16()));
                }
                catch(LightCategoryException ex)
                {
                    if(ShouldLightCategoryErrorBeReported(ex))
                    {
                        throw;
                    }
                }

                FlagClientContentPresent();
            }
        }

        #endregion
    }
}
