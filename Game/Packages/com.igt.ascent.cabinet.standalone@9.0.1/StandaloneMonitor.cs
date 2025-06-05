//-----------------------------------------------------------------------
// <copyright file = "StandaloneMonitor.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standalone
{
    using System;
    using System.Collections.Generic;
    using CSI.Schemas;

    /// <summary>
    /// Provide a virtual implementation of the monitor category.
    /// </summary>
    internal class StandaloneMonitor : IMonitor
    {
        /// <summary>
        /// Default device id for the main monitor.
        /// </summary>
        private const string MainMonitor = "Main";

        /// <summary>
        /// Default device id for the stereoscopic monitor.
        /// </summary>
        private const string StereoscopicMonitor = "Main_Stereoscopic";

        /// <summary>
        /// The current color profile settings.
        /// </summary>
        private readonly Dictionary<string, ColorProfileSetting> profileSettings =
            new Dictionary<string, ColorProfileSetting>();

        /// <summary>
        /// Constant for an invalid device ID error code.
        /// </summary>
        private const string InvalidDeviceIdErrorCode = "INVALID_MONITOR_ID";

        /// <summary>
        /// Constant for the description of an invalid device ID.
        /// </summary>
        private const string InvalidDeviceIdDescription = "Monitor device ID is not valid.";

        /// <summary>
        /// The current stereoscopic state.
        /// </summary>
        private StereoscopyState stereoscopyState;

        /// <summary>
        /// List of monitors provided for standalone.
        /// </summary>
        private readonly List<Monitor> currentMonitors = new List<Monitor>
        {
            new Monitor
            {
                //Matches the device ID for AoW.
                DeviceId = MainMonitor,
                Role = MonitorRole.Main,
                Style = MonitorStyle.Normal,
                Aspect = MonitorAspect.Widescreen,
                DesktopCoordinates = new DesktopRectangle
                {
                    x = 100,
                    y = 100,
                    w = 683,
                    h = 384
                },
                VirtualX = 0,
                VirtualY = 0,
                ColorProfileId = 72,
                //Claim support for the built-in profiles.
                AvailableColorProfiles = new List<ColorProfile>
                {
                    ColorProfile.None,
                    ColorProfile.Calibrated,
                    ColorProfile.ColorMatch
                }
            },

            new Monitor
            {
                //Matches the device ID for AoW.
                DeviceId = StereoscopicMonitor,
                Role = MonitorRole.Main,
                Style = MonitorStyle.Stereoscopic,
                Aspect = MonitorAspect.Widescreen,
                DesktopCoordinates = new DesktopRectangle
                {
                    x = 100,
                    y = 100,
                    w = 683,
                    h = 384
                },
                VirtualX = 0,
                VirtualY = 0,
                ColorProfileId = 72,

                //Claim support for the built-in profiles.
                AvailableColorProfiles = new List<ColorProfile>
                {
                    ColorProfile.None,
                    ColorProfile.Calibrated,
                    ColorProfile.ColorMatch
                },

                StereoscopicSettings = new StereoscopicSettings
                {
                                               Description = "SeeFront",
                                               Format = StereoscopicFormat.FRAME_PACKING,
                                               Frames = new List<StereoscopicFrame>
                                               {
                                                                new StereoscopicFrame
                                                                {
                                                                        Height = 720,
                                                                        Type = StereoscopicFrameType.LEFT,
                                                                        Width = 1280,
                                                                        X = 0,
                                                                        Y = 0
                                                                    },
                                                                new StereoscopicFrame
                                                                {
                                                                        Height = 720,
                                                                        Type = StereoscopicFrameType.RIGHT,
                                                                        Width = 1280,
                                                                        X = 0,
                                                                        Y = 750
                                                                    }
                                                            },
                                               Technology = StereoscopicTechnology.AUTOSTEREOSCOPY
                                            }
            }
        };

        /// <summary>
        /// Basic desktop rectangle.
        /// </summary>
        private readonly DesktopRectangle rectangle = new DesktopRectangle
        {
            h = 1,
            w = 1,
            x = 0,
            y = 0
        };

        /// <summary>
        /// Construct a default instance with basic monitor support.
        /// </summary>
        public StandaloneMonitor()
        {
            foreach(var monitor in currentMonitors)
            {
                profileSettings[monitor.DeviceId] = new ColorProfileSetting(ColorProfile.None);
            }

            stereoscopyState = StereoscopyState.DISABLED;
        }

        #region IMonitor Implementation

        /// <ineritdoc/>
        public MonitorComposition GetComposition()
        {
            return new MonitorComposition(currentMonitors, rectangle);
        }

        /// <ineritdoc/>
        public void SetColorProfile(string deviceId, ColorProfileSetting setting)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            if(!profileSettings.ContainsKey(deviceId))
            {
                throw new MonitorCategoryException(InvalidDeviceIdErrorCode, InvalidDeviceIdDescription);
            }

            profileSettings[deviceId] = setting ?? throw new ArgumentNullException(nameof(setting));
        }

        /// <ineritdoc/>
        public ColorProfileSetting GetActiveColorProfile(string deviceId)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }
            if(!profileSettings.ContainsKey(deviceId))
            {
                throw new MonitorCategoryException(InvalidDeviceIdErrorCode, InvalidDeviceIdDescription);
            }
            return profileSettings[deviceId];
        }

        /// <inheritdoc/>
        public void EnableStereoscopyDisplay(string deviceId)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            stereoscopyState = StereoscopyState.ENABLED;
        }

        /// <inheritdoc/>
        public void DisableStereoscopyDisplay(string deviceId)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            stereoscopyState = StereoscopyState.DISABLED;
        }

        /// <inheritdoc/>
        public StereoscopyState GetStereoscopyDisplayState(string deviceId)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }

            return stereoscopyState;
        }

        /// <inheritdoc/>
        public void SetTransmissiveSupport(string deviceId, TransmissiveSupport transmissiveSupport)
        {
            if(string.IsNullOrEmpty(deviceId))
            {
                throw new ArgumentException("Argument may not be null or empty.", nameof(deviceId));
            }
        }

        /// <inheritdoc/>
        public MonitorRole GetPreferredUIDisplay()
        {
            return MonitorRole.Main;
        }

        #endregion IMonitor Implementation
    }
}
