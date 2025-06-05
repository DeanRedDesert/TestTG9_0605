//-----------------------------------------------------------------------
// <copyright file = "LightGroupState.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Class which represents the state of a light group.
    /// </summary>
    internal class LightGroupState : ILightGroupState
    {
        #region Private Fields

        /// <summary>
        /// The number of lights in the group.
        /// </summary>
        private readonly int lightCount;

        /// <summary>
        /// List of all the light states for this group.
        /// </summary>
        private readonly List<LightState> lightStates;

        /// <summary>
        /// Constant that represents all lights.
        /// </summary>
        private const ushort AllLights = 0xFFFF;

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a LightGroupState.
        /// </summary>
        /// <param name="lightCount">The number of lights in the group.</param>
        public LightGroupState(int lightCount)
        {
            this.lightCount = lightCount;
            lightStates = Enumerable.Range(0, lightCount).Select(index => new LightState()).ToList();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Disable all of the lights in the group.
        /// </summary>
        public void TurnOffLights()
        {
            foreach(var state in lightStates)
            {
                state.Enabled = false;
            }
            SequenceActive = false;
        }

        /// <summary>
        /// Sets the Boolean state of a light.
        /// </summary>
        /// <param name="lightNumber">The number of the light to set.</param>
        /// <param name="state">The new state of the light.</param>
        /// <returns>True if the light was valid.</returns>
        public bool SetLightState(int lightNumber, bool state)
        {
            if(lightNumber < lightCount)
            {
                lightStates[lightNumber].Enabled = state;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the two bit intensity of a light.
        /// </summary>
        /// <param name="lightNumber">The number of the light to set.</param>
        /// <param name="intensity">The new intensity of the light.</param>
        /// <returns>True if the light was valid.</returns>
        public bool SetLightState(int lightNumber, BitwiseLightIntensity intensity)
        {
            if(lightNumber < lightCount)
            {
                lightStates[lightNumber].TwoBitIntensity = intensity;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the monochrome state of a light.
        /// </summary>
        /// <param name="lightState">The light state.</param>
        /// <returns>True if the light was valid.</returns>
        public bool SetLightState(MonochromeLightState lightState)
        {
            return HandleLights(lightState.LightNumber,
                                currentLight =>
                                    {
                                        lightStates[(int)currentLight].EightBitIntensity =
                                            lightState.Brightness;
                                    });
        }

        /// <summary>
        /// Sets the four bit color of a light.
        /// </summary>
        /// <param name="lightNumber">The number of the light to set.</param>
        /// <param name="bitwiseLightColor">The four bit bitwise color.</param>
        /// <returns>True if the light was valid.</returns>
        public bool SetLightState(int lightNumber, BitwiseLightColor bitwiseLightColor)
        {
            if(lightNumber < lightCount)
            {
                lightStates[lightNumber].FourBitColor = bitwiseLightColor;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the Rgb6 state of a light.
        /// </summary>
        /// <param name="lightNumber">The number of the light to set.</param>
        /// <param name="lightState">The light state.</param>
        /// <returns>True if the light was valid.</returns>
        public bool SetLightState(int lightNumber, Rgb6 lightState)
        {
            if(lightNumber < lightCount)
            {
                lightStates[lightNumber].SixBitColor = lightState;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the RGB state of a light.
        /// </summary>
        /// <param name="lightState">The light state.</param>
        /// <returns>True if the light was valid.</returns>
        public bool SetLightState(RgbLightState lightState)
        {
            return HandleLights(lightState.LightNumber,
                                currentLight =>
                                    {
                                        lightStates[(int)currentLight].SixteenBitColor =
                                            lightState.Color;
                                    });
        }

        /// <summary>
        /// Activate the specified sequence.
        /// </summary>
        /// <param name="sequence">The sequence to activate.</param>
        public void SetActiveSequence(uint sequence)
        {
            CurrentSequence = sequence;
            SequenceActive = true;
        }

        /// <summary>
        /// Check if the specified sequence is active.
        /// </summary>
        /// <param name="sequence">True if the sequence is active.</param>
        public bool IsSequenceActive(uint sequence)
        {
            return SequenceActive && CurrentSequence == sequence;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method which abstracts control of a single or multiple lights.
        /// </summary>
        /// <param name="lightNumber">
        /// The light number. May either be a single light or a constants representing all lights.
        /// </param>
        /// <param name="lightAction">The action to perform on the light.</param>
        /// <returns>True if the operation was successful.</returns>
        private bool HandleLights(uint lightNumber, Action<uint> lightAction)
        {
            if(lightNumber < lightCount)
            {
                lightAction(lightNumber);
                return true;
            }

            if(lightNumber == AllLights)
            {
                for(ushort lightIndex = 0; lightIndex < lightStates.Count; lightIndex++)
                {
                    lightAction(lightIndex);
                }
                return true;
            }

            return false;
        }

        #endregion

        #region ILightGroupState Members

        /// <inheritdoc/>
        public IEnumerable<ILightState> LightStates => lightStates;

        /// <inheritdoc/>
        public bool SequenceActive { get; private set; }

        /// <inheritdoc/>
        public uint CurrentSequence { get; private set; }

        #endregion
    }
}