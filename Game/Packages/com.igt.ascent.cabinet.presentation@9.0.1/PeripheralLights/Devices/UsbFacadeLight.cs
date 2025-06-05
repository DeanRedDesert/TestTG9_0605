//-----------------------------------------------------------------------
// <copyright file = "UsbFacadeLight.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using Communication.Cabinet;

    /// <summary>
    /// Represents a USB facade light hardware.
    /// </summary>
    public class UsbFacadeLight : UsbPeripheralLight
    {
        private const uint AttractSequenceId = 0xFFFF;

        /// <summary>
        /// Construct a USB facade light.
        /// </summary>
        /// <param name="featureName">The hardware feature name of the peripheral.</param>
        /// <param name="featureDescription">The light feature description of the peripheral.</param>
        /// <param name="peripheralLights">The interface to use to communicate to the hardware.</param>
        internal UsbFacadeLight(string featureName, LightFeatureDescription featureDescription, IPeripheralLights peripheralLights)
            : base(featureName, featureDescription, peripheralLights)
        {

        }

        /// <summary>
        /// Starts a sequence on the facade light.
        /// </summary>
        /// <param name="groupNumber">The number of the group to apply the sequence to.</param>
        /// <param name="sequenceNumber">The number which identifies the desired sequence.</param>
        /// <param name="parameters">
        /// Byte list containing the parameters for the sequence. If the sequence does not require any parameters,
        /// then null may be passed.
        /// </param>
        public void StartSequence(byte groupNumber, uint sequenceNumber, IList<byte> parameters)
        {
            StartSequence(groupNumber, sequenceNumber, parameters, TransitionMode.Immediate);
        }

        /// <summary>
        /// Starts a sequence on the facade light.
        /// </summary>
        /// <param name="groupNumber">The number of the group to apply the sequence to.</param>
        /// <param name="sequenceNumber">The number which identifies the desired sequence.</param>
        /// <param name="parameters">
        /// Byte list containing the parameters for the sequence. If the sequence does not require any parameters,
        /// then null may be passed.
        /// </param>
        /// <param name="transitionMode">The transition mode to use when running the sequence.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown if <paramref name="groupNumber"/> is out of range when the device is acquired.
        /// </exception>
        public void StartSequence(byte groupNumber, uint sequenceNumber, IList<byte> parameters,
            TransitionMode transitionMode)
        {
            if(DeviceAcquired && groupNumber >= GroupCount && groupNumber != AllGroups)
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.InvariantCulture,
                    "The device only reports {0} groups, the specified group number {1} is invalid.",
                    GroupCount, groupNumber));
            }

            base.StartSequence(groupNumber, sequenceNumber, transitionMode, parameters);
        }

        /// <summary>
        /// Starts the attract sequence on all light groups.
        /// </summary>
        public void StartAttractSequence()
        {
            base.StartSequence(AllGroups, AttractSequenceId, TransitionMode.Immediate, null);
        }
    }
}
