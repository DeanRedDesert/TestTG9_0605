//-----------------------------------------------------------------------
// <copyright file = "StandaloneDeviceTiltManager.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.StandaloneDeviceConfiguration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Core.Communication.Cabinet;
    using Core.Communication.Cabinet.Standalone;
    using Core.Communication.Cabinet.Standalone.Devices;
    using IGT.Game.Core.Communication.Cabinet.MechanicalReels;
    using UnityEngine;

    /// <summary>
    /// A very simple version of tilt manager that manages
    /// tilts related to standalone devices.
    /// 
    /// This manager does not hold up game play when there is a tilt.
    /// It simply prints warnings in the log/console when tilt is
    /// reported and cleared.
    /// 
    /// This class is implemented mainly for testing purpose.
    /// It should be replaced with the full-fledged standalone
    /// tilt manager in the future.
    /// </summary>
    public sealed class StandaloneDeviceTiltManager
    {
        #region Static Members

        /// <summary>
        /// The global instance of the type <see cref="StandaloneDeviceTiltManager"/>.
        /// </summary>
        private static StandaloneDeviceTiltManager instance;

        /// <summary>
        /// Starts a global instance of <see cref="StandaloneDeviceTiltManager"/>
        /// with a cabinet lib interface.
        /// </summary>
        /// <param name="cabinetLib">
        /// The cabinet lib from which the manager listens for
        /// the device events that indicate tilts being reported
        /// or cleared.
        /// </param>
        /// <param name="logDeviceInformation">
        /// A flag indicating whether the tilt manager should log
        /// the information updated by hardware devices.
        /// The information logged would include both normal and
        /// tilted statues.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="cabinetLib"/> is null.
        /// </exception>
        public static void Start(ICabinetLib cabinetLib, bool logDeviceInformation)
        {
            if(instance == null)
            {
                instance = new StandaloneDeviceTiltManager(cabinetLib, logDeviceInformation);
            }
        }

        #endregion

        #region Instance Members

        /// <summary>
        /// List of devices that have been tilted.
        /// </summary>
        private readonly HashSet<DeviceIdentifier> tiltedDevices = new HashSet<DeviceIdentifier>();

        /// <remarks>
        /// Private constructor.
        /// </remarks>
        private StandaloneDeviceTiltManager(ICabinetLib cabinetLib, bool logDeviceInformation)
        {
            if(cabinetLib == null)
            {
                throw new ArgumentNullException(nameof(cabinetLib));
            }

            cabinetLib.DeviceConnectedEvent += HandleDeviceConnected;
            cabinetLib.DeviceRemovedEvent += HandleDeviceRemoved;
            cabinetLib.DeviceReleasedEvent += HandleDeviceReleased;

            if(logDeviceInformation)
            {
                if(cabinetLib.GetInterface<IPeripheralLights>() is HardwarePeripheralLights hardwarePeripheralLights)
                {
                    hardwarePeripheralLights.DeviceInformationUpdateEvent += HandleDeviceInformationUpdate;
                }

                if(cabinetLib.GetInterface<IMechanicalReels>() is HardwareMechanicalReels hardwareMechanicalReels)
                {
                    hardwareMechanicalReels.DeviceInformationUpdateEvent += HandleDeviceInformationUpdate;
                }
            }
        }

        /// <summary>
        /// Handle Device Connected event from the cabinet lib.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceConnected(object sender, DeviceConnectedEventArgs eventArgs)
        {
            var identifier = new DeviceIdentifier(eventArgs.DeviceName, eventArgs.DeviceId);

            // Avoid duplicate tilt cleared messages.
            if(tiltedDevices.Contains(identifier))
            {
                tiltedDevices.Remove(identifier);

                Debug.LogWarning($"{identifier}'s tilt is cleared.");

                if(!tiltedDevices.Any())
                {
                    Debug.LogWarning("All device tilts have been cleared.");
                }
            }
        }

        /// <summary>
        /// Handle Device Removed event from the cabinet lib.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceRemoved(object sender, DeviceRemovedEventArgs eventArgs)
        {
            AddDeviceTilt(new DeviceIdentifier(eventArgs.DeviceName, eventArgs.DeviceId));
        }

        /// <summary>
        /// Handle Device Released event from the cabinet lib.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private void HandleDeviceReleased(object sender, DeviceReleasedEventArgs eventArgs)
        {
            AddDeviceTilt(new DeviceIdentifier(eventArgs.DeviceName, eventArgs.DeviceId));
        }

        /// <summary>
        /// Add a device tilt to the tilt list.
        /// </summary>
        /// <param name="identifier">Identifier of the device being tilted.</param>
        private void AddDeviceTilt(DeviceIdentifier identifier)
        {
            // Avoid duplicate tilt messages.
            if(!tiltedDevices.Contains(identifier))
            {
                tiltedDevices.Add(identifier);

                Debug.LogWarning($"{identifier} is tilted.");
            }
        }

        /// <summary>
        /// Handle Device Information Update event from the hardware devices.
        /// </summary>
        /// <remarks>
        /// This function logs all the information update, including both normal
        /// and tilted information.  A reel tilt manager should only handle the
        /// tilts.
        /// </remarks>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="eventArgs">The event arguments.</param>
        private static void HandleDeviceInformationUpdate(object sender, DeviceInformationUpdateEventArgs eventArgs)
        {
            Debug.Log(eventArgs.DeviceInformation);
        }

        #endregion
    }
}
