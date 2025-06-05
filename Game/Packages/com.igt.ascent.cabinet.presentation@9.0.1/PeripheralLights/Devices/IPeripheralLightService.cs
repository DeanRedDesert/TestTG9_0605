// -----------------------------------------------------------------------
// <copyright file = "IPeripheralLightService.cs" company = "IGT">
//     Copyright (c) 2021 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using CabinetServices;

    /// <summary>
    /// The cabinet service that provides access to various USB peripheral lights.
    /// </summary>
    /// <devdoc>
    /// This interface derives from ICabinetService rather than IDeviceService because of
    /// the special implementation of the concrete service class.
    /// </devdoc>
    public interface IPeripheralLightService : ICabinetService
    {
        #region Events

        /// <summary>
        /// Occurs when peripheral lights are acquired.
        /// </summary>
        event Action<PeripheralLightDeviceEventArgs> PeripheralLightDeviceAcquired;

        /// <summary>
        /// Occurs when peripheral lights are released.
        /// </summary>
        event Action<PeripheralLightDeviceEventArgs> PeripheralLightDeviceReleased;

        /// <summary>
        /// Occurs when peripheral lights are removed.
        /// </summary>
        event Action<PeripheralLightDeviceEventArgs> PeripheralLightDeviceRemoved;

        #endregion

        #region Release Lights Functions

        /// <summary>
        /// Releases control over all acquired light devices.
        /// </summary>
        void ReleaseLights();

        #endregion

        #region Get Light Functions

        /// <summary>
        /// Gets a peripheral light instance for a specific topper light peripheral.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the topper hardware.</returns>
        UsbTopperLight GetPeripheralLight(TopperHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for a specific halo light peripheral.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the halo hardware.</returns>
        UsbHaloLight GetPeripheralLight(HaloHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for a specific button edge light peripheral.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the button edge hardware.</returns>
        UsbButtonEdgeLight GetPeripheralLight(ButtonHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for a specific light bars peripheral.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the light bars hardware.</returns>
        UsbLightBars GetPeripheralLight(LightBarHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for a specific facade light peripheral.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the facade hardware.</returns>
        UsbFacadeLight GetPeripheralLight(FacadeHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for legacy mechanical reel lights.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the reel backlight hardware.</returns>
        UsbIndividualLightControl GetPeripheralLight(LegacyReelLightHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for a specific crystal core light peripheral.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the crystal core hardware.</returns>
        UsbCrystalCoreLight GetPeripheralLight(CrystalCoreLightHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for a specific streaming light peripheral.
        /// </summary>
        /// <param name="hardware">
        /// The hardware to get control for.
        /// </param>
        /// <param name="allowCoupling">
        /// Specifies whether a game client can use two types of <see cref="StreamingLightHardware"/> to request a single coupled light device.
        /// Default value is false.
        /// </param>
        /// <remarks>
        /// A coupled light device is a single light device that can be obtained using two different types of <see cref="StreamingLightHardware"/>.
        /// Using two types of <see cref="StreamingLightHardware"/> to request a coupled light device can lead to unwanted performance glitches.
        /// Setting the colors with both types can result in colors flashing before setting to the correct color.
        /// Setting sequences with both types can result in both sequences trying to play on the device at once.
        /// Again, please only request a coupled light object with both types of <see cref="StreamingLightHardware"/> when ABSOLUTELY necessary.
        /// </remarks>
        /// <returns>
        /// Returns an instance to the streaming light hardware.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="hardware"/> is <see cref="StreamingLightHardware.Unknown"/>.
        /// </exception>
        /// <exception cref="CoupledLightException">
        /// Thrown when requesting a coupled light device with two types of <see cref="StreamingLightHardware"/> and
        /// not explicitly setting <paramref name="allowCoupling"/> to true.
        /// </exception>
        UsbStreamingLight GetPeripheralLight(StreamingLightHardware hardware, bool allowCoupling = false);

        /// <summary>
        /// Gets a peripheral light instance for a specific streaming light peripheral that
        /// supports symbol highlights.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the streaming light hardware.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="hardware"/> is <see cref="StreamingLightHardware.Unknown"/>.
        /// </exception>
        UsbSymbolHighlightSupportedStreamingLight GetSymbolHighlightSupportedStreamingLight(SymbolHighlightSupportedStreamingLightHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for a specific handle light peripheral.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the handle light hardware.</returns>
        UsbHandleLight GetPeripheralLight(HandleLightHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for the nested wheels light hardware.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the nested wheels light hardware.</returns>
        UsbNestedWheelLight GetPeripheralLight(NestedwheelsLightHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for the twilight zone 3D monitor bezel hardware.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the twilight zone 3D monitor bezel hardware.</returns>
        TwilightZone3DMonitorBezel GetPeripheralLight(TwilightZone3DMonitorBezelHardware hardware);

        /// <summary>
        /// Gets a peripheral light instance for the legacy backlight hardware.
        /// </summary>
        /// <param name="hardware">The hardware to get control for.</param>
        /// <returns>Returns an instance to the legacy backlight hardware</returns>
        UsbLegacyBacklight GetPeripheralLight(LegacyBacklightHardware hardware);

        #endregion

        #region Device Intensity Functions

        /// <summary>
        /// Sets the light intensity for all applicable connected devices.
        /// </summary>
        /// <param name="intensity">The intensity to set the lights to.</param>
        void SetLightIntensity(byte intensity);

        /// <summary>
        /// Gets the current light intensity for all applicable connected devices.
        /// </summary>
        /// <returns>The light intensity level for the first group on the first device.</returns>
        byte GetLightIntensity();

        #endregion

        #region Universal Color Functions

        /// <summary>
        /// Sets a universal color for all blank light devices.
        /// </summary>
        /// <remarks>
        /// A device is considered to be blank if:
        ///<list type="numbered">
        ///<item>It has not been requested by the client via a GetPeripheralLight call, OR</item>
        ///<item>It has been requested by the client via a GetPeripheralLight call, but does NOT have
        ///      any valid content set by calls like SetColor or PlaySequence etc.</item>
        ///</list>
        /// </remarks>
        /// <param name="color">The universal color to set.</param>
        void SetUniversalColor(Color color);

        #endregion

        #region Enable and Disable Functions

        /// <summary>
        /// Enables the device implementations for the specified USB streaming light.
        /// Precondition: The device implementation for the specified streaming light hardware must have been retrieved using GetPeripheralLight.
        /// </summary>
        /// <param name="deviceType">Type of streaming light to enable.</param>
        /// <returns>True if the device implementation was enabled; otherwise, false.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="deviceType"/> is StreamingLightHardware.Unknown.
        /// </exception>
        bool EnableStreamingLight(StreamingLightHardware deviceType);

        /// <summary>
        /// Disables the device implementation for the specified USB streaming light.
        /// Precondition: The device implementation for the specified streaming light hardware must have been retrieved using GetPeripheralLight.
        /// </summary>
        /// <param name="deviceType">Type of streaming light to disable.</param>
        /// <returns>True if the device implementation was disabled; otherwise, false.</returns>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="deviceType"/> is StreamingLightHardware.Unknown.
        /// </exception>
        bool DisableStreamingLight(StreamingLightHardware deviceType);

        /// <summary>
        /// Clears all the streaming light layers and updates them.
        /// </summary>
        void ClearStreamingLights();

        #endregion
    }
}
