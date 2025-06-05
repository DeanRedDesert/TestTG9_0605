//-----------------------------------------------------------------------
// <copyright file = "IDeviceControl.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone.Devices.IgtUsbDevice
{
    using System;

    /// <summary>
    /// Interface for controlling the physical devices.
    /// </summary>
    public interface IDeviceControl
    {
        #region Events

        /// <summary>
        /// Event posted when there is an update of the information on a device.
        /// </summary>
        event EventHandler<DeviceInformationUpdateEventArgs> DeviceInformationUpdateEvent;

        #endregion

        #region Methods

        /// <summary>
        /// Reset the target device given its sub feature name.
        /// </summary>
        /// <param name="featureId">Sub feature name of the device.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown when <paramref name="featureId"/> is not valid.
        /// </exception>
        bool Reset(string featureId);

        /// <summary>
        /// Run self test on the target device given its sub feature name.
        /// </summary>
        /// <param name="featureId">Sub feature name of the device.</param>
        /// <returns>True if succeed, false otherwise.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown when <paramref name="featureId"/> is not valid.
        /// </exception>
        bool SelfTest(string featureId);

        /// <summary>
        /// Get the status on the target device given its sub feature name,
        /// and process it.
        /// </summary>
        /// <param name="featureId">Sub feature name of the device.</param>
        /// <returns>A string describing the status of the device.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown when <paramref name="featureId"/> is not valid.
        /// </exception>
        string PollDevice(string featureId);

        /// <summary>
        /// Get a string describing the descriptors that are common to all devices.
        /// </summary>
        /// <param name="featureId">Sub feature name of the device.</param>
        /// <returns>The string describing the descriptors.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown when <paramref name="featureId"/> is not valid.
        /// </exception>
        string GetCommonDescriptors(string featureId);

        /// <summary>
        /// Get a string describing the descriptors that are specific to a device.
        /// </summary>
        /// <param name="featureId">Sub feature name of the device.</param>
        /// <returns>The string describing the descriptors.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="featureId"/> is null.
        /// </exception>
        /// <exception cref="InvalidFeatureIdException">
        /// Thrown when <paramref name="featureId"/> is not valid.
        /// </exception>
        string GetFeatureDescriptors(string featureId);

        #endregion
    }
}
