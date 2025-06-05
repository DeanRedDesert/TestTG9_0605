//-----------------------------------------------------------------------
// <copyright file = "MonitorController.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Monitor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Communication.Cabinet;
    using Communication.Cabinet.CSI.Schemas;
    using CsiColorProfileSetting = Communication.Cabinet.ColorProfileSetting;

    /// <summary>
    /// A class responsible for acquiring/releasing/controlling monitor devices.
    /// </summary>
    public static class MonitorController
    {
        #region Private Fields

        /// <summary>
        /// A reference to the acquired stereoscopic Monitor.
        /// </summary>
        private static Monitor stereoscopicMonitor;

        /// <summary>
        /// Color profile settings that have been requested by the user. When possible they are applied to the device.
        /// </summary>
        private static IColorProfileSettings colorProfileSettings;

        /// <summary>
        /// Reference to the cabinet library.
        /// </summary>
        private static ICabinetLib cabinet;

        /// <summary>
        /// The IMonitor interface.
        /// </summary>
        private static IMonitor monitorInterface;

        /// <summary>
        /// The mount point for the game.
        /// </summary>
        private static string mountPoint;

        /// <summary>
        /// The monitor composition.
        /// </summary>
        private static MonitorComposition monitorComposition;

        /// <summary>
        /// The list of acquired monitors.
        /// </summary>
        private static readonly IList<string> AcquiredMonitors = new List<string>();

        #endregion

        #region Public Fields

        /// <summary>
        /// An event raised when a device is acquired.
        /// </summary>
        public static event EventHandler<DeviceAcquiredEventArgs> DeviceAcquiredEvent;

        /// <summary>
        /// An event raised when a device is released.
        /// </summary>
        public static event EventHandler<DeviceReleasedEventArgs> DeviceReleasedEvent;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the flag indicating if <see cref="AsyncConnect"/> is complete.
        /// </summary>
        public static bool AsyncConnectComplete { get; private set; }

        /// <summary>
        /// Gets the flag indicating if <see cref="PostConnect"/> is complete.
        /// </summary>
        public static bool PostConnectComplete { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set the current monitor category and game mount point
        /// on a separate thread to speed up performance.
        /// </summary>
        /// <remarks>
        /// This method follows the pattern defined by <see cref="IAsyncConnect.AsyncConnect"/>
        /// </remarks>
        /// <param name="cabinetLib">
        /// Reference to the current cabinet library. This interface will be used for requesting control of monitors.
        /// </param>
        /// <param name="gameMountPoint">The mount point for the game package.</param>
        /// <param name="priority">The priority.</param>
        /// <exception cref="ArgumentNullException">Thrown if the cabinetLib is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the gameMountPoint is null or empty.</exception>
        public static void AsyncConnect(ICabinetLib cabinetLib, string gameMountPoint, Priority priority)
        {
            if(string.IsNullOrEmpty(gameMountPoint))
            {
                throw new ArgumentException("The game mount may not be null or empty.", nameof(gameMountPoint));
            }

            cabinet = cabinetLib ?? throw new ArgumentNullException(nameof(cabinetLib), "A valid cabinet interface is required.");
            mountPoint = gameMountPoint;

            if(cabinet != null)
            {
                cabinetLib.DeviceAcquiredEvent -= OnDeviceAcquired;
                cabinetLib.DeviceReleasedEvent -= OnDeviceReleased;

                monitorInterface = cabinet.GetInterface<IMonitor>();
                if(monitorInterface != null)
                {
                    monitorComposition = monitorInterface.GetComposition();

                    cabinet.DeviceAcquiredEvent += OnDeviceAcquired;
                    cabinet.DeviceReleasedEvent += OnDeviceReleased;

                    AcquireMonitors(priority);
                }
            }

            AsyncConnectComplete = true;
        }

        /// <summary>
        /// Initializes on the main thread.
        /// This will be called after <see cref="AsyncConnect"/>.
        /// </summary>
        /// <remarks>
        /// This method follows the pattern defined by <see cref="IAsyncConnect.PostConnect"/>
        /// </remarks>
        /// <exception cref="AsyncConnectException">Thrown if <see cref="PostConnect"/> failed.</exception>
        public static void PostConnect()
        {
            if(!AsyncConnectComplete)
            {
                throw new AsyncConnectException("Post Connect cannot be called before Async Connect completes.");
            }
            PostConnectComplete = true;
        }

        /// <summary>
        /// Get the monitor information for the specified monitor device.
        /// </summary>
        /// <param name="deviceId">The device to get information for.</param>
        /// <returns>Information about the monitor or null if there is no matching monitor.</returns>
        public static Monitor GetMonitorInfo(string deviceId)
        {
            return monitorComposition?.Monitors.FirstOrDefault(m => m.DeviceId == deviceId);
        }

        /// <summary>
        /// Returns whether a monitor is acquired given its style.
        /// </summary>
        /// <param name="monitorStyle">The monitor style.</param>
        /// <returns>True if monitor is acquired.</returns>
        public static bool IsMonitorAcquired(MonitorStyle monitorStyle)
        {
            // find the monitor
            if(AcquiredMonitors.Count > 0)
            {
                // find the device ID
                var monitor = monitorComposition?.Monitors?.FirstOrDefault(m => m.Style == monitorStyle);
                if(monitor != null)
                {
                    return AcquiredMonitors.Contains(monitor.DeviceId);
                }
            }

            return false;
        }

        /// <summary>
        /// Remove any event handlers or other references to the cabinet library.
        /// </summary>
        public static void RemoveCabinetLibrary()
        {
            if(cabinet != null)
            {
                var monitorList = new List<string>(AcquiredMonitors);

                // Release acquired devices
                foreach(var monitor in monitorList)
                {
                    cabinet.ReleaseDevice(DeviceType.Monitor, monitor);
                    OnDeviceReleased(null, new DeviceReleasedEventArgs(DeviceType.Monitor, monitor, DeviceAcquisitionFailureReason.RequestQueued));
                }

                cabinet.DeviceAcquiredEvent -= OnDeviceAcquired;
                cabinet.DeviceReleasedEvent -= OnDeviceReleased;

                cabinet = null;
                monitorInterface = null;
                monitorComposition = null;

                // colorProfileSettings shouldn't be set to null here as it is set from the SDK scripts.
            }
        }

        /// <summary>
        /// Set the desired color profile settings. These settings will be applied as possible if color profiles are
        /// supported.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Thrown if settings is null.
        /// </exception>
        /// <param name="settings">Desired color profile settings.</param>
        public static void SetProfileSettings(IColorProfileSettings settings)
        {
            colorProfileSettings = settings ?? throw new ArgumentNullException(nameof(settings), "The color profile settings may not be null.");

            foreach(var monitor in AcquiredMonitors)
            {
                var monitorInfo = GetMonitorInfo(monitor);
                if(monitorInfo != null)
                {
                    ApplyColorProfiles(monitorInfo);
                }
            }
        }

        /// <summary>
        /// Set the monitor's <see cref="StereoscopyState"/>.
        /// </summary>
        /// <param name="stereoscopyState">The <see cref="StereoscopyState"/>.</param>
        /// <exception cref="MonitorControllerException">
        /// Thrown if stereoscopy state cannot be set.
        /// </exception>
        public static void SetStereoscopyState(StereoscopyState stereoscopyState)
        {
            if(stereoscopicMonitor == null || monitorInterface == null)
            {
                throw new MonitorControllerException("Error setting monitor's stereoscopic state");
            }

            switch (stereoscopyState)
            {
                case StereoscopyState.DISABLED:
                    if (monitorInterface.GetStereoscopyDisplayState(stereoscopicMonitor.DeviceId) != StereoscopyState.DISABLED)
                    {
                        monitorInterface.DisableStereoscopyDisplay(stereoscopicMonitor.DeviceId);
                    }
                    break;

                case StereoscopyState.ENABLED:
                    if (monitorInterface.GetStereoscopyDisplayState(stereoscopicMonitor.DeviceId) != StereoscopyState.ENABLED)
                    {
                        monitorInterface.EnableStereoscopyDisplay(stereoscopicMonitor.DeviceId);
                    }
                    break;
            }
        }

        /// <summary>
        /// Return the current <see cref="StereoscopyState"/>.
        /// </summary>
        /// <exception cref="MonitorControllerException">
        /// Thrown if stereoscopy state cannot be acquired.
        /// </exception>
        /// <returns>The current <see cref="StereoscopyState"/>.</returns>
        public static StereoscopyState GetStereoscopyState()
        {
            if(monitorInterface != null && stereoscopicMonitor != null)
            {
                return monitorInterface.GetStereoscopyDisplayState(stereoscopicMonitor.DeviceId);
            }

            throw new MonitorControllerException("Error getting the monitor's stereoscopic state");
        }

        /// <summary>
        /// Gets whether or not there is a stereo monitor present.
        /// </summary>
        /// <returns>True if the stereo monitor is present. False if the stereo monitor is not present.</returns>
        public static bool IsStereoMonitorPresent()
        {
            return stereoscopicMonitor != null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Attempt to acquire all available monitor devices.
        /// </summary>
        /// <exception cref="MonitorControllerException">
        /// Thrown if Monitors cannot be acquired.
        /// </exception>
        private static void AcquireMonitors(Priority priority)
        {
            if(cabinet == null)
            {
                throw new MonitorControllerException("Cabinet cannot be null.");
            }

            if(monitorInterface == null)
            {
                throw new MonitorControllerException("monitorInterface cannot be null.");
            }

            // Clear cached monitors
            AcquiredMonitors.Clear();

            if(monitorComposition != null)
            {
                foreach(var monitorConfig in monitorComposition.Monitors)
                {
                    // Acquire the monitor.
                    var result = cabinet.RequestAcquireDevice(DeviceType.Monitor, monitorConfig.DeviceId,
                        priority);

                    // Check the result
                    if(result.Acquired)
                    {
                        // Fire the acquire event
                        OnDeviceAcquired(null, new DeviceAcquiredEventArgs(DeviceType.Monitor, monitorConfig.DeviceId));
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceAcquiredEvent"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        private static void OnDeviceAcquired(object sender, DeviceAcquiredEventArgs args)
        {
            // We are only interested in Monitors.
            if(args.DeviceName == DeviceType.Monitor)
            {
                var monitor = GetMonitorInfo(args.DeviceId);

                // Add device to the cache.
                if(monitor != null && !AcquiredMonitors.Contains(monitor.DeviceId))
                {
                    AcquiredMonitors.Add(monitor.DeviceId);

                    if(monitor.Style == MonitorStyle.Stereoscopic)
                    {
                        stereoscopicMonitor = monitor;
                    }
                }

                // Apply color profiles
                ApplyColorProfiles(GetMonitorInfo(args.DeviceId));

                // Fire the custom event
                DeviceAcquiredEvent?.Invoke(sender, args);
            }
        }

        /// <summary>
        /// Event handler for <see cref="ICabinetLib.DeviceReleasedEvent"/>.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        private static void OnDeviceReleased(object sender, DeviceReleasedEventArgs args)
        {
            // We are only interested in Monitors.
            if(args.DeviceName == DeviceType.Monitor)
            {
                var monitor = GetMonitorInfo(args.DeviceId);

                // Device released, remove it from the cache.
                if(monitor != null && AcquiredMonitors.Contains(monitor.DeviceId))
                {
                    AcquiredMonitors.Remove(monitor.DeviceId);

                    // Invalidated the stereoscopic monitor.
                    if(monitor.Style == MonitorStyle.Stereoscopic)
                    {
                        stereoscopicMonitor = null;
                    }
                }

                // Fire the custom event
                DeviceReleasedEvent?.Invoke(sender, args);
            }
        }

        /// <summary>
        /// Applies the color profiles to the given monitor.
        /// </summary>
        /// <param name="monitor">The monitor to apply the color profile to.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if monitor is null.
        /// </exception>
        private static void ApplyColorProfiles(Monitor monitor)
        {
            if(monitor == null)
            {
                throw new ArgumentNullException(nameof(monitor), "A valid monitor is required.");
            }

            if(colorProfileSettings == null)
            {
                return;
            }

            try
            {
                if(colorProfileSettings.ProfileStrategy == ColorProfileStrategy.Custom)
                {
                    if(colorProfileSettings.CustomSettings != null)
                    {
                        var monitorSetting = colorProfileSettings.CustomSettings.FirstOrDefault(
                            setting => setting.Id == monitor.ColorProfileId);

                        if(monitorSetting != null)
                        {
                            if(monitorSetting.Strategy == ColorProfileStrategy.Custom)
                            {
                                //Need to get the profile path elsewhere.
                                monitorInterface?.SetColorProfile(monitor.DeviceId,
                                    new CsiColorProfileSetting(Path.Combine(mountPoint,
                                        monitorSetting.CustomFile)));
                            }
                            else
                            {
                                ApplyBasicColorProfileStrategy(monitorSetting.Strategy, monitor);
                            }
                        }
                        else
                        {
                            //Default to the calibrated profile if no settings were provided.
                            ApplyBasicColorProfileStrategy(ColorProfileStrategy.Calibrated, monitor);
                        }
                    }
                }
                else
                {
                    ApplyBasicColorProfileStrategy(colorProfileSettings.ProfileStrategy, monitor);
                }
            }
            catch (Exception e)
            {
                //If a color profile cannot be set, then it is not a critical error. Log the issue and continue.
                Logging.Log.WriteWarning("Unable to apply color profile: " + e);
            }
        }

        /// <summary>
        /// Apply a non-custom color profile strategy to the given monitor.
        /// </summary>
        /// <param name="strategy">The profile strategy to use.</param>
        /// <param name="monitorInfo">The monitor to apply the strategy.</param>
        private static void ApplyBasicColorProfileStrategy(ColorProfileStrategy strategy, Monitor monitorInfo)
        {
            switch(strategy)
            {
                case ColorProfileStrategy.Matched:
                    SetColorMatchedProfile(monitorInfo);
                    break;
                case ColorProfileStrategy.Calibrated:
                    SetCalibratedProfile(monitorInfo);
                    break;
                case ColorProfileStrategy.Uncalibrated:
                    monitorInterface?.SetColorProfile(monitorInfo.DeviceId,
                        new CsiColorProfileSetting(ColorProfile.None));
                    break;
            }
        }

        /// <summary>
        /// Attempt to set the monitor to a calibrated profile. If a calibrated profile is not available, then the
        /// profile will be reset.
        /// </summary>
        /// <param name="monitorInfo">Monitor to apply the profile to.</param>
        private static void SetCalibratedProfile(Monitor monitorInfo)
        {
            monitorInterface?.SetColorProfile(monitorInfo.DeviceId,
                monitorInfo.AvailableColorProfiles.Contains(ColorProfile.Calibrated)
                    ? new CsiColorProfileSetting(ColorProfile.Calibrated)
                    : new CsiColorProfileSetting(ColorProfile.None));
        }

        /// <summary>
        /// Attempt to set the monitor to a color matched profile. If a color matched profile is not available, then an
        /// attempt will be made to apply a calibrated profile.
        /// </summary>
        /// <param name="monitorInfo">Monitor to apply the profile to.</param>
        private static void SetColorMatchedProfile(Monitor monitorInfo)
        {
            if(monitorInfo.AvailableColorProfiles.Contains(ColorProfile.ColorMatch))
            {
                monitorInterface?.SetColorProfile(monitorInfo.DeviceId,
                    new CsiColorProfileSetting(ColorProfile.ColorMatch));
            }
            else
            {
                SetCalibratedProfile(monitorInfo);
            }
        }

        #endregion
    }
}
