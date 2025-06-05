//-----------------------------------------------------------------------
// <copyright file = "UsbIndividualLightControl.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Communication.Cabinet;

    /// <summary>
    /// Abstract class for implementing light devices which support individual light control.
    /// </summary>
    /// <devdoc>
    /// The class does not contain any abstract methods, but it is made abstract to prevent its direct use.
    /// </devdoc>
    public abstract class UsbIndividualLightControl : UsbPeripheralLight
    {
        /// <summary>
        /// Construct a USB light device with individually controllable lights.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbIndividualLightControl(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        { }

        /// <summary>
        /// Set the color of a specific light.
        /// </summary>
        /// <param name="groupId">
        /// ID for the requested group. Cannot be <see cref="UsbLightBase.AllGroups"/>.
        /// </param>
        /// <param name="lightId">ID for the requested light.</param>
        /// <param name="color">Color to set the light to.</param>
        /// <exception cref="ArgumentException">Thrown when the color is empty. The color cannot be empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either the <paramref name="groupId"/> or <paramref name="lightId"/> are not in range.
        /// </exception>
        public virtual void SetColor(byte groupId, ushort lightId, Color color)
        {
            if(color.IsEmpty)
            {
                throw new ArgumentException("The color cannot be empty.", nameof(color));
            }

            if(DeviceAcquired)
            {
                if(groupId >= GroupCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId));
                }

                var info = GetGroupInformation(groupId);
                if(lightId >= info.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(lightId));
                }

                var lightState = new RgbLightState(lightId, color.GetRgb16());
                try
                {
                    LightInterface.ControlLightsRgb(FeatureName, groupId, new List<RgbLightState> {lightState});
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
        /// Set the color for all lights in a group individually.
        /// </summary>
        /// <param name="groupId">
        /// ID for the requested group. Cannot be <see cref="UsbLightBase.AllGroups" />.
        /// </param>
        /// <param name="colors">The desired color of each light.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="groupId"/> is out of range.
        /// </exception>
        public virtual void SetColor(byte groupId, IEnumerable<Color> colors)
        {
            if(DeviceAcquired)
            {
                if(groupId >= GroupCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId));
                }

                LightInterface.ControlLightsRgb(FeatureName, groupId, colors.Select((color, index) =>
                                                        new RgbLightState((ushort) index, color.GetRgb16())));
            }
        }

        /// <summary>
        /// Turn a specific light on or off.
        /// </summary>
        /// <param name="groupId">
        /// ID for the requested group. Cannot be <see cref="UsbLightBase.AllGroups" />.
        /// </param>
        /// <param name="lightId">ID for the requested light.</param>
        /// <param name="state">The on/off state of the light.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either the <paramref name="groupId"/> or <paramref name="lightId"/> are not in range.
        /// </exception>
        public virtual void SetLightState(byte groupId, ushort lightId, bool state)
        {
            if(DeviceAcquired)
            {
                if(groupId >= GroupCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId));
                }

                var info = GetGroupInformation(groupId);
                if(lightId >= info.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(lightId));
                }

                try
                {
                    LightInterface.BitwiseLightControl(FeatureName, groupId, lightId, new List<bool> { state });
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
        /// Turn all lights individually on or off in a group.
        /// </summary>
        /// <param name="groupId">
        /// ID for the requested group. Cannot be <see cref="UsbLightBase.AllGroups" />.
        /// </param>
        /// <param name="states">The on/off state of each light.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="groupId"/> is out of range.
        /// </exception>
        public virtual void SetLightState(byte groupId, IEnumerable<bool> states)
        {
            if(DeviceAcquired)
            {
                if(groupId >= GroupCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId));
                }

                LightInterface.BitwiseLightControl(FeatureName, groupId, 0, states);
            }
        }

        /// <summary>
        /// Set the monochrome brightness of all lights in a group.
        /// </summary>
        /// <param name="groupId">
        /// ID for the requested group. Cannot be <see cref="UsbLightBase.AllGroups" />.
        /// </param>
        /// <param name="brightness">Desired monochrome brightness for all lights.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="groupId"/> is out of range.
        /// </exception>
        public virtual void SetBrightness(byte groupId, byte brightness)
        {
            if(DeviceAcquired)
            {
                if(groupId >= GroupCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId));
                }

                var info = GetGroupInformation(groupId);

                LightInterface.ControlLightsMonochrome(FeatureName, groupId, 0, Enumerable.Range(0, info.Count).Select(i => brightness));
            }
        }

        /// <summary>
        /// Set the monochrome brightness of a specific light.
        /// </summary>
        /// <param name="groupId">
        /// ID for the requested group. Cannot be <see cref="UsbLightBase.AllGroups" />.
        /// </param>
        /// <param name="lightId">ID for the requested light.</param>
        /// <param name="brightness">Desired monochrome brightness.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either the <paramref name="groupId"/> or <paramref name="lightId"/> are not in range.
        /// </exception>
        public virtual void SetBrightness(byte groupId, ushort lightId, byte brightness)
        {
            if(DeviceAcquired)
            {
                if(groupId >= GroupCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId));
                }

                var info = GetGroupInformation(groupId);
                if(lightId >= info.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(lightId));
                }

                LightInterface.ControlLightsMonochrome(FeatureName, groupId, lightId, new List<byte> { brightness });
            }
        }

        /// <summary>
        /// Set the monochrome brightness of all lights individually in a group.
        /// </summary>
        /// <param name="groupId">
        /// ID for the requested group. Cannot be <see cref="UsbLightBase.AllGroups" />.
        /// </param>
        /// <param name="brightness">Desired monochrome brightness of each light.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when the <paramref name="groupId"/> is out of range.
        /// </exception>
        public virtual void SetBrightness(byte groupId, IEnumerable<byte> brightness)
        {
            if(DeviceAcquired)
            {
                if(groupId >= GroupCount && groupId != AllGroups)
                {
                    throw new ArgumentOutOfRangeException(nameof(groupId));
                }

                LightInterface.ControlLightsMonochrome(FeatureName, groupId, 0, brightness);
            }
        }
    }
}
