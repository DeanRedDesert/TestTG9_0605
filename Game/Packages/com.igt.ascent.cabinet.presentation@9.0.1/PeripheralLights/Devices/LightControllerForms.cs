//-----------------------------------------------------------------------
// <copyright file = "LightControllerForms.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// The main header for the light controller data. It is identified by the MHDR tag.
    /// Encapsulates information stored in the main header.
    /// </summary>
    /// <remarks>
    /// The main header is also referred to as the MHDR form in the light controller file document in AVP.
    /// </remarks>
    public class MainLightControllerData
    {
        #region Public Data Members

        /// <summary>
        /// Constructor for the main light controller data.
        /// </summary>
        /// <param name="animationSequenceName">The name of the light sequences.</param>
        public MainLightControllerData(string animationSequenceName)
        {
            AnimationSequenceName = animationSequenceName;
        }

        /// <summary>
        /// The name of the animation sequence.
        /// </summary>
        public string AnimationSequenceName { get; }

        /// <summary>
        /// A read only collection of <see cref="LightControllerData"/> for each light controller type.
        /// </summary>
        public IEnumerable<LightControllerData> LightControllerData => lightControllerDataList;

        #endregion

        #region Private Data Members

        /// <summary>
        /// A list of <see cref="LightControllerData"/> for each light controller type.
        /// </summary>
        private readonly List<LightControllerData> lightControllerDataList = new List<LightControllerData>();

        #endregion

        /// <summary>
        /// Add a <see cref="LightControllerData"/> into the list.
        /// </summary>
        /// <param name="lightControllerData"><see cref="LightControllerData"/></param>
        internal void AddLightControllerData(LightControllerData lightControllerData)
        {
            if(lightControllerData != null)
            {
                lightControllerDataList.Add(lightControllerData);
            }
        }
    }

    /// <summary>
    /// Data on a specific light controller type.
    /// </summary>
    /// <remarks>
    /// Examples of controller types are AVP Royale (Type 1), Trimline (Type 2),
    /// WideScreen and Light Bars (Type 3), and UltraWide (Type 4).
    /// The data structure containing this data is also referred to as the LBZT form
    /// in the light controller file document in AVP.
    /// </remarks>
    public class LightControllerData
    {
        #region Public Data Members

        /// <summary>
        /// Constructor for the light controller data.
        /// </summary>
        /// <param name="identifier">The light controller type.</param>
        public LightControllerData(uint identifier)
        {
            Identifier = identifier;
        }

        /// <summary>
        /// The light controller type.
        /// </summary>
        public uint Identifier { get; }

        /// <summary>
        /// A read only collection of <see cref="AnimationData"/> in the light controller.
        /// </summary>
        public IEnumerable<AnimationData> AnimationData => animationDataList;

        #endregion

        #region Private Data Members

        /// <summary>
        /// A list of <see cref="AnimationData"/> in the light controller.
        /// </summary>
        private readonly List<AnimationData> animationDataList = new List<AnimationData>();

        #endregion

        /// <summary>
        /// Add an <see cref="AnimationData"/> into the list.
        /// </summary>
        /// <param name="animationData"><see cref="AnimationData"/></param>
        internal void AddAnimationData(AnimationData animationData)
        {
            if(animationData != null)
            {
                animationDataList.Add(animationData);
            }
        }
    }

    /// <summary>
    /// Specific light animations/frames for a light controller type.
    /// </summary>
    /// <remarks>
    /// The data structure containing this data is also referred to as the ANMF form
    /// in the light controller file document in AVP.
    /// </remarks>
    public class AnimationData
    {
        #region Public Data Members

        /// <summary>
        /// Constructor for the light animation data.
        /// </summary>
        /// <param name="duration">The duration of this animation.</param>
        public AnimationData(uint duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// The duration to run this animation (in milliseconds). If duration is not specified, the default
        /// duration from the light controller containing this data must be used.
        /// </summary>
        public uint Duration { get; }

        /// <summary>
        /// A read only collection of pixel colors to display on the device.
        /// </summary>
        public IEnumerable<Rgb6> FrameData => frameDataList;

        #endregion

        #region Private Data Members

        /// <summary>
        /// A list of pixel colors to display on the device.
        /// Each pixel consists of four bytes. One byte represents the color intensity of red,
        /// one byte represents the color intensity of green, one byte represents the color intensity of blue,
        /// and one byte is reserved and is not used.
        /// </summary>
        /// <remarks>
        /// The light bars for instance contains 120 pixels in each frame.
        /// </remarks>
        private readonly List<Rgb6> frameDataList = new List<Rgb6>();

        #endregion

        /// <summary>
        /// Add a frame data into the list.
        /// </summary>
        /// <param name="rgb6"><see cref="Rgb6"/></param>
        internal void AddFrameData(Rgb6 rgb6)
        {
            frameDataList.Add(rgb6);
        }
    }
}
