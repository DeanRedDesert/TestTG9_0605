//-----------------------------------------------------------------------
// <copyright file = "UsbCrystalCoreLight.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;

    /// <summary>
    /// Represents a crystal core cabinet light.
    /// </summary>
    public class UsbCrystalCoreLight : UsbHaloLight
    {
        #region Hardware Sequence IDs

        private const ushort DualFadeSequenceId = 0xC00C;

        #endregion

        /// <summary>
        /// Construct a USB crystal core light.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbCrystalCoreLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {

        }

        #region Dual Fade Sequence

        /// <summary>
        /// A host defined number of segments are alternately cross-faded between two
        /// predefined colors.
        /// </summary>
        /// <param name="transitionTime">The number of milliseconds between color transitions.</param>
        /// <param name="holdTime">
        /// The number of milliseconds it takes to display fully transitioned colors.
        /// </param>
        /// <param name="numberOfSegments">The number of segments (2 to 16).</param>
        /// <param name="colors">The predefined colors for the fade.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="transitionTime"/> or <paramref name="holdTime"/> are 0.
        /// Thrown if <paramref name="numberOfSegments"/> is out of range.
        /// </exception>
        /// <example>
        /// <![CDATA[
        /// lightObject.RunDualFadeSequence(100, 250, 8, DualFadeSequenceColor.RedBlue);
        /// ]]>
        /// </example>
        public void RunDualFadeSequence(ushort transitionTime, ushort holdTime,
            byte numberOfSegments, DualFadeSequenceColor colors)
        {
            RunDualFadeSequence(transitionTime, holdTime, numberOfSegments, colors, TransitionMode.Immediate);
        }

        /// <summary>
        /// A host defined number of segments are alternately cross-faded between two
        /// predefined colors.
        /// </summary>
        /// <param name="transitionTime">The number of milliseconds between color transitions.</param>
        /// <param name="holdTime">
        /// The number of milliseconds it takes to display fully transitioned colors.
        /// </param>
        /// <param name="numberOfSegments">The number of segments (2 to 16).</param>
        /// <param name="colors">The predefined colors for the fade.</param>
        /// <param name="transitionMode">The sequence transition mode.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="transitionTime"/> or <paramref name="holdTime"/> are 0.
        /// Thrown if <paramref name="numberOfSegments"/> is out of range.
        /// </exception>
        /// <example>
        /// <![CDATA[
        /// lightObject.RunDualFadeSequence(100, 250, 8, DualFadeSequenceColor.RedBlue, TransitionMode.Immediate);
        /// ]]>
        /// </example>
        public void RunDualFadeSequence(ushort transitionTime, ushort holdTime,
            byte numberOfSegments, DualFadeSequenceColor colors, TransitionMode transitionMode)
        {
            if(transitionTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transitionTime),
                    "The transition time cannot be 0.");
            }

            if(holdTime == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(holdTime),
                    "The hold time cannot be 0.");
            }

            if(numberOfSegments < 2 || numberOfSegments > 16)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfSegments),
                    "The number of segments must be from 2 to 16.");
            }

            if(CanLightCommandBeSent)
            {
                // Do not use object or collection initializer because of the byte list extension method.
                // ReSharper disable once UseObjectOrCollectionInitializer
                var parameters = new List<byte>();
                parameters.Add(transitionTime);
                parameters.Add(holdTime);
                parameters.Add(numberOfSegments);
                parameters.Add((byte)colors);

                StartSequence(HaloLightGroupId, DualFadeSequenceId, transitionMode,
                    parameters);
            }
        }

        #endregion

    }
}
