//-----------------------------------------------------------------------
// <copyright file = "VirtualPeripheralLights.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Standalone implementation of the lights interface.
    /// </summary>
    public class VirtualPeripheralLights : IPeripheralLights
    {
        #region Nested Classes

        /// <summary>
        /// Class which encapsulates information about a light feature.
        /// </summary>
        private class LightFeature : ILightFeatureState
        {
            /// <summary>
            /// Description of the feature.
            /// </summary>
            public LightFeatureDescription Description { get; }

            /// <summary>
            /// Current state of the groups in the feature.
            /// </summary>
            public Dictionary<byte, LightGroupState> GroupStates { get; }

            /// <summary>
            /// Construct a new light feature.
            /// </summary>
            /// <param name="description">Description of the feature.</param>
            public LightFeature(LightFeatureDescription description)
            {
                Description = description;
                GroupStates = new Dictionary<byte, LightGroupState>();

                foreach(var lightGroup in description.Groups)
                {
                    GroupStates[lightGroup.GroupNumber] = new LightGroupState(lightGroup.Count);
                }
            }

            #region ILightFeatureState Implementation

            /// <inheritdoc/>
            IDictionary<byte, ILightGroupState> ILightFeatureState.GroupStates
            {
                get
                {
                    return
                        GroupStates.ToDictionary<KeyValuePair<byte, LightGroupState>, byte, ILightGroupState>(
                            lightGroupState => lightGroupState.Key, lightGroupState => lightGroupState.Value);
                }
            }

            /// <inheritdoc/>
            public string FeatureId => Description.FeatureId;

            #endregion
        }

        #endregion

        #region Private Constants

        /// <summary>
        /// Constant which indicates all groups are to be controlled.
        /// </summary>
        private const ushort AllGroups = 0xFF;

        #endregion

        #region Private Fields

        /// <summary>
        /// List of the configured light features.
        /// </summary>
        private readonly Dictionary<string, LightFeature> lightFeatures = new Dictionary<string, LightFeature>();

        #endregion

        #region Constructor

        /// <summary>
        /// Construct an instance of the VirtualPeripheralLights class with the given features.
        /// </summary>
        /// <param name="lightFeatureDescriptions">List of features available.</param>
        public VirtualPeripheralLights(IEnumerable<LightFeatureDescription> lightFeatureDescriptions)
        {
            foreach(var description in lightFeatureDescriptions)
            {
                lightFeatures[description.FeatureId] = new LightFeature(description);
            }
        }

        #endregion

        #region IPeripheralLights Members

        #region Device Enumeration and Acquisition

        /// <inheritdoc/>
        public IEnumerable<LightFeatureDescription> GetLightDevices()
        {
            return from lightFeature in lightFeatures select lightFeature.Value.Description;
        }

        /// <inheritdoc/>
        public bool RequiresDeviceAcquisition => true;

        #endregion

        #region Manual Light Control

        /// <inheritdoc/>
        public void TurnOffGroup(string featureId, byte groupNumber, TransitionMode transitionMode)
        {
            var feature = GetFeature(featureId);
            HandleGroups(groupNumber, feature, currentGroup =>
                                                   {
                                                       var groupState = feature.GroupStates[currentGroup];
                                                       groupState.TurnOffLights();
                                                   });
        }

        /// <inheritdoc/>
        public void ControlLightsMonochrome(string featureId, byte groupNumber,
                                            IEnumerable<MonochromeLightState> lightStates)
        {
            var feature = GetFeature(featureId);

            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }

            HandleGroups(groupNumber, feature, currentGroup =>
                                                   {
                                                       var groupState = feature.GroupStates[currentGroup];
                                                       foreach(var lightState in lightStates)
                                                       {
                                                           if(!groupState.SetLightState(lightState))
                                                           {
                                                               throw new InvalidLightException(featureId, currentGroup,
                                                                                               lightState.LightNumber);
                                                           }
                                                       }
                                                   });
        }

        /// <inheritdoc/>
        public void ControlLightsMonochrome(string featureId, byte groupNumber, ushort startingLight,
                                            IEnumerable<byte> brightnesses)
        {
            var feature = GetFeature(featureId);

            if(brightnesses == null)
            {
                throw new ArgumentNullException(nameof(brightnesses));
            }
            var brightnessList = brightnesses.ToList();

            HandleGroups(groupNumber, feature, (currentGroup, group) =>
                                                   {
                                                       var groupState = feature.GroupStates[currentGroup];

                                                       for(ushort brightnessIndex = 0;
                                                           brightnessIndex < brightnessList.Count;
                                                           brightnessIndex++)
                                                       {
                                                           var lightNumber = (ushort)(startingLight + brightnessIndex);
                                                           if(!groupState.SetLightState(
                                                                    new MonochromeLightState(
                                                                        lightNumber,
                                                                        brightnessList.ElementAt(brightnessIndex))))
                                                           {
                                                               throw new ArgumentOutOfRangeException(
                                                                   string.Format(CultureInfo.InvariantCulture,
                                                                                 "Light: {0} is not within the group range of {1} - {2}.",
                                                                                 lightNumber, 0,
                                                                                 group.Count));
                                                           }
                                                       }
                                                   });
        }

        /// <inheritdoc/>
        public void ControlLightsRgb(string featureId, byte groupNumber, IEnumerable<RgbLightState> lightStates)
        {
            var feature = GetFeature(featureId);

            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }

            HandleGroups(groupNumber, feature, currentGroup =>
            {
                var groupState = feature.GroupStates[currentGroup];
                foreach(var lightState in lightStates)
                {
                    if(!groupState.SetLightState(lightState))
                    {
                        throw new InvalidLightException(featureId, currentGroup, lightState.LightNumber);
                    }
                }
            });
        }

        /// <inheritdoc/>
        public void ControlLightsRgb(string featureId, byte groupNumber, ushort startingLight, IEnumerable<Rgb16> colors)
        {
            var feature = GetFeature(featureId);

            if(colors == null)
            {
                throw new ArgumentNullException(nameof(colors));
            }
            var enumerable = colors.ToList();

            HandleGroups(groupNumber, feature, (currentGroup, group) =>
                                                   {

                                                       var groupState = feature.GroupStates[currentGroup];
                                                       for(ushort colorIndex = 0;
                                                           colorIndex < enumerable.Count;
                                                           colorIndex++)
                                                       {
                                                           var lightNumber = (ushort)(startingLight + colorIndex);
                                                           if(
                                                               !groupState.SetLightState(new RgbLightState
                                                                                             (lightNumber,
                                                                                              enumerable.ElementAt(
                                                                                                  colorIndex)
                                                                                             )))
                                                           {
                                                               throw new ArgumentOutOfRangeException(
                                                                   string.Format(CultureInfo.InvariantCulture,
                                                                                 "Light: {0} is not within the group range of {1} - {2}.",
                                                                                 lightNumber, 0,
                                                                                 group.Count));
                                                           }
                                                       }
                                                   });
        }

        #endregion

        #region Sequence Control

        /// <inheritdoc/>
        public void StartSequence(string featureId, byte groupNumber, uint sequenceNumber, TransitionMode transitionMode,
                                  byte[] parameters)
        {
            var feature = GetFeature(featureId);

            HandleGroups(groupNumber, feature, currentGroup => feature.GroupStates[currentGroup].SetActiveSequence(
                sequenceNumber));
        }

        /// <inheritdoc/>
        public bool IsSequenceRunning(string featureId, byte groupNumber, uint sequenceNumber)
        {
            var feature = GetFeature(featureId);
            GetGroup(feature, groupNumber);
            return feature.GroupStates[groupNumber].IsSequenceActive(sequenceNumber);
        }

        #endregion

        #region Bitwise Light Control

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<bool> lightStates)
        {
            var feature = GetFeature(featureId);
            var group = GetGroup(feature, groupNumber);

            if(lightStates == null)
            {
                throw new ArgumentNullException(nameof(lightStates));
            }
            var enumerable = lightStates.ToList();

            //Standalone does not need to keep the bitwise state. Represent the state using the manual control information.
            var groupState = feature.GroupStates[groupNumber];
            for(ushort stateIndex = 0; stateIndex < enumerable.Count; stateIndex++)
            {
                var lightNumber = (ushort)(startingLight + stateIndex);
                if(!groupState.SetLightState(lightNumber, enumerable.ElementAt(stateIndex)))
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format(CultureInfo.InvariantCulture,
                                      "Light: {0} is not within the group range of {1} - {2}.", lightNumber, 0,
                                      group.Count));
                }
            }
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<BitwiseLightIntensity> lightIntensities)
        {
            var feature = GetFeature(featureId);
            var group = GetGroup(feature, groupNumber);

            if(lightIntensities == null)
            {
                throw new ArgumentNullException(nameof(lightIntensities));
            }
            var bitwiseLightIntensities = lightIntensities.ToList();

            var groupState = feature.GroupStates[groupNumber];
            for(ushort intensityIndex = 0; intensityIndex < bitwiseLightIntensities.Count; intensityIndex++)
            {
                var lightNumber = (ushort)(startingLight + intensityIndex);
                if(!groupState.SetLightState(lightNumber, bitwiseLightIntensities.ElementAt(intensityIndex)))
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format(CultureInfo.InvariantCulture,
                                      "Light: {0} is not within the group range of {1} - {2}.", lightNumber, 0,
                                      group.Count));
                }
            }
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<BitwiseLightColor> lightColors)
        {
            var feature = GetFeature(featureId);
            var group = GetGroup(feature, groupNumber);

            if(lightColors == null)
            {
                throw new ArgumentNullException(nameof(lightColors));
            }
            var bitwiseLightColors = lightColors.ToList();

            var groupState = feature.GroupStates[groupNumber];
            for(ushort colorIndex = 0; colorIndex < bitwiseLightColors.Count; colorIndex++)
            {
                var lightNumber = (ushort)(startingLight + colorIndex);
                if(!groupState.SetLightState(lightNumber, bitwiseLightColors.ElementAt(colorIndex)))
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format(CultureInfo.InvariantCulture,
                                      "Light: {0} is not within the group range of {1} - {2}.", lightNumber, 0,
                                      group.Count));
                }
            }
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight,
                                        IEnumerable<Rgb6> lightColors)
        {
            var feature = GetFeature(featureId);
            var group = GetGroup(feature, groupNumber);

            if(lightColors == null)
            {
                throw new ArgumentNullException(nameof(lightColors));
            }
            var enumerable = lightColors.ToList();

            var groupState = feature.GroupStates[groupNumber];
            for(ushort colorIndex = 0; colorIndex < enumerable.Count; colorIndex++)
            {
                var lightNumber = (ushort)(startingLight + colorIndex);
                if(!groupState.SetLightState(lightNumber, enumerable.ElementAt(colorIndex)))
                {
                    throw new ArgumentOutOfRangeException(
                        string.Format(CultureInfo.InvariantCulture,
                                      "Light: {0} is not within the group range of {1} - {2}.", lightNumber, 0,
                                      group.Count));
                }
            }
        }

        /// <inheritdoc/>
        public void BitwiseLightControl(string featureId, byte groupNumber, ushort startingLight, byte bitsPerLight,
                                        byte[] lightData)
        {
            var feature = GetFeature(featureId);
            var group = GetGroup(feature, groupNumber);

            if(lightData == null)
            {
                throw new ArgumentNullException(nameof(lightData));
            }

            //The maximum number of lights being controlled.
            var possibleLights = group.Count - startingLight;

            //Verify the data size.
            var maxBytesForLights = (int)Math.Ceiling(possibleLights * bitsPerLight / 8f);
            if(lightData.Length > maxBytesForLights)
            {
                throw new InvalidLightStateSizeException(featureId, groupNumber, maxBytesForLights, lightData.Length);
            }

            switch(bitsPerLight)
            {
                case 1:
                    {
                        var lightStates = lightData.SelectMany(x => EnumerateBits(x).Reverse()).Take(group.Count);
                        BitwiseLightControl(featureId, groupNumber, startingLight, lightStates);
                        break;
                    }
                case 2:
                    {
                        var intensities =
                            lightData.SelectMany(x => EnumerateBitPairs(x).Reverse()).Take(group.Count).Cast<BitwiseLightIntensity>();
                        BitwiseLightControl(featureId, groupNumber, startingLight, intensities);
                        break;
                    }
                case 4:
                    {
                        var colors =
                            lightData.SelectMany(x => EnumerateNibbles(x).Reverse()).Take(group.Count).Cast<BitwiseLightColor>();
                        BitwiseLightControl(featureId, groupNumber, startingLight, colors);
                        break;
                    }
                case 6:
                    {
                        var colors = EnumerateSixBitColors(lightData);
                        BitwiseLightControl(featureId, groupNumber, startingLight, colors);
                        break;
                    }
            }
        }

        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Method which allows for handling control of a single or multiple groups.
        /// </summary>
        /// <param name="groupNumber">
        /// The group number to apply the action to. This can either be a specific group or the AllGroups constant.
        /// </param>
        /// <param name="feature">The feature the group(s) is in.</param>
        /// <param name="groupAction">The action to perform on the group.</param>
        private static void HandleGroups(byte groupNumber, LightFeature feature, Action<byte> groupAction)
        {
            if(groupNumber != AllGroups)
            {
                //Get the group number for verification.
                GetGroup(feature, groupNumber);
                groupAction(groupNumber);
            }
            else
            {
                foreach(var groupState in feature.GroupStates)
                {
                    groupAction(groupState.Key);
                }
            }
        }

        /// <summary>
        /// Method which allows for handling control of a single or multiple groups.
        /// </summary>
        /// <param name="groupNumber">
        /// The group number to apply the action to. This can either be a specific group or the AllGroups constant.
        /// </param>
        /// <param name="feature">The feature the group(s) is in.</param>
        /// <param name="groupAction">The action to perform on the group.</param>
        private static void HandleGroups(byte groupNumber, LightFeature feature, Action<byte, ILightGroup> groupAction)
        {
            if(groupNumber != AllGroups)
            {
                //Get the group number for verification.
                var group = GetGroup(feature, groupNumber);
                groupAction(groupNumber, group);
            }
            else
            {
                foreach(var groupState in feature.GroupStates)
                {
                    var group = GetGroup(feature, groupState.Key);
                    groupAction(groupState.Key, group);
                }
            }
        }

        /// <summary>
        /// Get the LightFeature for the given feature ID.
        /// </summary>
        /// <param name="featureId">The ID of the feature to get.</param>
        /// <returns>The LightGroup for the feature ID.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the featureId is null.</exception>
        /// <exception cref="InvalidFeatureIdException">Thrown when the given feature id is not valid.</exception>
        private LightFeature GetFeature(string featureId)
        {
            if(featureId == null)
            {
                throw new ArgumentNullException(nameof(featureId));
            }

            if(!lightFeatures.ContainsKey(featureId))
            {
                throw new InvalidFeatureIdException(featureId);
            }

            return lightFeatures[featureId];
        }

        /// <summary>
        /// Get the light group for the given feature and group number.
        /// </summary>
        /// <param name="feature">The feature containing the group.</param>
        /// <param name="groupNumber">The number of the group.</param>
        /// <returns>The requested light group.</returns>
        /// <exception cref="InvalidLightGroupException">
        /// Thrown when the specified group does not exist in the given feature.
        /// </exception>
        private static ILightGroup GetGroup(LightFeature feature, byte groupNumber)
        {
            var group =
                (from lightGroup in feature.Description.Groups
                 where lightGroup.GroupNumber == groupNumber
                 select lightGroup).FirstOrDefault();

            if(group == null)
            {
                throw new InvalidLightGroupException(feature.FeatureId, groupNumber);
            }

            return group;
        }

        /// <summary>
        /// Enumerate the bits in a byte.
        /// </summary>
        /// <param name="value">The value to enumerate.</param>
        /// <returns>Enumerable of the bits in a byte.</returns>
        private static IEnumerable<bool> EnumerateBits(byte value)
        {
            for(var bitIndex = 0; bitIndex < 8; bitIndex++)
            {
                if((1 & value) == 1)
                {
                    yield return true;
                }
                else
                {
                    yield return false;
                }
                value >>= 1;
            }
        }

        /// <summary>
        /// Enumerate two bit values in the byte.
        /// </summary>
        /// <param name="value">The value to enumerate.</param>
        /// <returns>An enumerable of the two bit values in the byte.</returns>
        private static IEnumerable<byte> EnumerateBitPairs(byte value)
        {
            for(var pairIndex = 0; pairIndex < 4; pairIndex++)
            {
                yield return (byte)(value & 0x03);
                value >>= 2;
            }
        }

        /// <summary>
        /// Enumerate nibbles in the byte.
        /// </summary>
        /// <param name="value">The value to enumerate.</param>
        /// <returns>An enumerable of nibbles in the byte.</returns>
        private static IEnumerable<byte> EnumerateNibbles(byte value)
        {
            for(var nibbleIndex = 0; nibbleIndex < 2; nibbleIndex++)
            {
                yield return (byte)(value & 0x0F);
                value >>= 4;
            }
        }

        /// <summary>
        /// Enumerate six bit colors in the bytes.
        /// </summary>
        /// <param name="value">The value to enumerate.</param>
        /// <returns>An enumerable of the four bit colors in the bytes.</returns>
        private static IEnumerable<Rgb6> EnumerateSixBitColors(IEnumerable<byte> value)
        {
            var pairs = value.SelectMany(x => EnumerateBitPairs(x).Reverse()).ToList();
            const int pairsPerColor = 3;

            while(pairs.Any())
            {
                var color = pairs.Take(pairsPerColor).ToList();
                pairs = pairs.Skip(pairsPerColor).ToList();
                if(color.Count == pairsPerColor)
                {
                    yield return new Rgb6(color[0], color[1], color[2]);
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets a list of the current light feature states.
        /// </summary>
        public IEnumerable<ILightFeatureState> FeatureStates
        {
            get { return lightFeatures.Select(lightFeature => (ILightFeatureState)lightFeature.Value); }
        }

        #endregion
    }
}
