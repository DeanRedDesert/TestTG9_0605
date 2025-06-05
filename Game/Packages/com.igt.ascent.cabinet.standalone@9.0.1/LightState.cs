//-----------------------------------------------------------------------
// <copyright file = "LightState.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    /// <summary>
    /// Class which represents the state of a light.
    /// </summary>
    internal class LightState : ILightState
    {
        #region ILightState Implementation

        /// <inheritdoc/>
        public bool Enabled { set; get; }

        /// <inheritdoc/>
        public LightStateType LastSetType { private set; get; }

        private BitwiseLightIntensity twoBitIntensity;

        /// <summary>
        /// Get/Set the two bit intensity of the light.
        /// </summary>
        public BitwiseLightIntensity TwoBitIntensity
        {
            set
            {
                twoBitIntensity = value;
                LastSetType = LightStateType.TwoBitIntensity;
                Enabled = true;
            }
            get => twoBitIntensity;
        }

        private byte eightBitIntensity;

        /// <summary>
        /// Get/Set the eight bit intensity of the light.
        /// </summary>
        public byte EightBitIntensity
        {
            set
            {
                eightBitIntensity = value;
                LastSetType = LightStateType.EightBitIntensity;
                Enabled = true;
            }
            get => eightBitIntensity;
        }

        private BitwiseLightColor fourBitColor1;

        /// <summary>
        /// Get/Set the four bit color of the light.
        /// </summary>
        public BitwiseLightColor FourBitColor
        {
            set
            {
                fourBitColor1 = value;
                LastSetType = LightStateType.FourBitColor;
                Enabled = true;
            }
            get => fourBitColor1;
        }

        private Rgb6 sixBitColor1;

        /// <summary>
        /// Get/Set the six bit color of the light.
        /// </summary>
        public Rgb6 SixBitColor
        {
            set
            {
                sixBitColor1 = value;
                LastSetType = LightStateType.SixBitColor;
                Enabled = true;
            }
            get => sixBitColor1;
        }

        private Rgb16 sixteenBitColor1;

        /// <summary>
        /// Get/Set the sixteen bit color of the light.
        /// </summary>
        public Rgb16 SixteenBitColor
        {
            set
            {
                sixteenBitColor1 = value;
                LastSetType = LightStateType.SixteenBitColor;
                Enabled = true;
            }
            get => sixteenBitColor1;
        }

        #endregion
    }
}