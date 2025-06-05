//-----------------------------------------------------------------------
// <copyright file = "IMonitor.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    using System;
    using CSI.Schemas;

    /// <summary>
    /// Interface for the monitor category of the CSI.
    /// </summary>
    public interface IMonitor
    {
        /// <summary>
        /// Get the current monitor composition.
        /// </summary>
        /// <returns>
        /// Configuration information including all present monitors and their relevant parameters.
        /// </returns>
        MonitorComposition GetComposition();

        /// <summary>
        /// Set the color profile for the specified monitor device.
        /// </summary>
        /// <param name="deviceId">
        /// The identifier for the monitor. The valid identifiers are determined by the monitor composition.
        /// </param>
        /// <param name="setting">The desired profile setting.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="setting"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="deviceId"/> is null or empty.</exception>
        /// <exception cref="MonitorCategoryException">
        /// Thrown if the <paramref name="deviceId"/> is not a valid device.
        /// </exception>
        void SetColorProfile(string deviceId, ColorProfileSetting setting);

        /// <summary>
        /// Get the active color profile of the specified monitor device.
        /// </summary>
        /// <param name="deviceId">The identifier for the device.</param>
        /// <returns>The color profile setting for the monitor.</returns>
        /// <exception cref="ArgumentException">Thrown if the <paramref name="deviceId"/> is null or empty.</exception>
        /// <exception cref="MonitorCategoryException">
        /// Thrown if the <paramref name="deviceId"/> is not a valid device.
        /// </exception>
        ColorProfileSetting GetActiveColorProfile(string deviceId);

        /// <summary>
        /// Enables Stereoscopic display.
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="deviceId"/> is null or empty.
        /// </exception>
        /// <exception cref="MonitorCategoryException">
        /// Thrown if the <paramref name="deviceId"/> is not a valid device.
        /// </exception>
        void EnableStereoscopyDisplay(string deviceId);

        /// <summary>
        /// Disables Stereoscopic display.
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="deviceId"/> is null or empty.
        /// </exception>
        /// <exception cref="MonitorCategoryException">
        /// Thrown if the <paramref name="deviceId"/> is not a valid device.
        /// </exception>
        void DisableStereoscopyDisplay(string deviceId);

        /// <summary>
        /// Requests the Stereoscopic display state.
        /// </summary>
        /// <param name="deviceId">Device identifier.</param>
        /// <returns>The state of the Stereoscopy display.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="deviceId"/> is null or empty.
        /// </exception>
        /// <exception cref="MonitorCategoryException">
        /// Thrown if the <paramref name="deviceId"/> is not a valid device.
        /// </exception>
        StereoscopyState GetStereoscopyDisplayState(string deviceId);

        /// <summary>
        /// Notifies CSI of the capabilities the client has to drive transmissive content on a specified display.
        /// </summary>
        /// <param name="deviceId">
        /// The identifier for the monitor. The valid identifiers are determined by the monitor composition.
        /// </param>
        /// <param name="transmissiveSupport">
        /// The transmissive content, if any, the client supports for the specified monitor.
        /// </param>
        /// <exception cref="MonitorCategoryException">
        /// Thrown if SetTransmissiveSupport without targeting CsiMonitor v1.3 or newer.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown if the <paramref name="deviceId"/> is null or empty.
        /// </exception>
        void SetTransmissiveSupport(string deviceId, TransmissiveSupport transmissiveSupport);

        /// <summary>
        /// Request to get the preferred display for user input.
        /// </summary>
        /// <returns>
        /// The monitor role mapping to the preferred display for user input.
        /// </returns>
        // ReSharper disable once InconsistentNaming
        MonitorRole GetPreferredUIDisplay();
    }
}
