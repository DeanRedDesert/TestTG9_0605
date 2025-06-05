// -----------------------------------------------------------------------
// <copyright file = "UsbSymbolHighlightSupportedStreamingLight.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Devices
{
    using System;
    using System.Collections.Generic;
    using Communication.Cabinet;
    using Communication.Cabinet.SymbolHighlights;
    using Streaming;

    /// <summary>
    /// Streaming light class that implements symbol highlight control. 
    /// Commands will not be sent if the device does not support symbol highlights.
    /// </summary>
    public class UsbSymbolHighlightSupportedStreamingLight : UsbStreamingLight
    {
        #region Properties

        /// <summary>
        /// Flag indicating if the Symbol Tracking/Hot Position feature is enabled.
        /// Symbol highlight commands can be sent to the device while the feature is
        /// not enabled, and the device commands will be displayed once the feature
        /// is enabled again. This flag will be reset when the device becomes unacquired,
        /// due to the Foundation resetting the state of the light device after client change.
        /// </summary>
        public bool IsFeatureEnabled { get; private set; }

        /// <inheritdoc/>
        public override bool DeviceAcquired
        {
            get => base.DeviceAcquired;
            internal set
            {
                var valueChanged = DeviceAcquired != value;
                if(valueChanged && !value)
                {
                    // The feature gets disabled by the Foundation when the device is unacquired from a client.
                    IsFeatureEnabled = false;
                }

                base.DeviceAcquired = value;
            }
        }

        #endregion

        #region Members

        /// <summary>
        /// Cached list of symbol tracking commands. 
        /// </summary>
        /// <remarks>
        /// Member made internal for unit testing purposes.
        /// </remarks>
        internal readonly List<SymbolTrackingData> CachedTrackingCommands = new List<SymbolTrackingData>();

        /// <summary>
        /// Cached list of hot position commands. 
        /// </summary>
        /// <remarks>
        /// Member made internal for unit testing purposes.
        /// </remarks>
        internal readonly List<SymbolHotPositionData> CachedHotPositionCommands = new List<SymbolHotPositionData>();

        /// <summary>
        /// The last reel index.
        /// Mechanical reel shelves can have up to 5 reels.
        /// </summary>
        private readonly int maxReelIndex;

        /// <summary>
        /// the last reel stop index. 
        /// Mechanical reels have a total of 22 stop positions. 
        /// </summary>
        private const int MaxReelStopIndex = 21;

        /// <summary>
        /// The last window stop index.
        /// The mechanical reel shelf window has a total of 5 visible stop positions.
        /// </summary>
        private const int MaxWindowStopIndex = 4;


        #endregion

        /// <summary>
        /// Create an instance of <see cref="UsbSymbolHighlightSupportedStreamingLight"/>.
        /// </summary>
        /// <param name="featureName">Name of the feature.</param>
        /// <param name="featureDescription">Description for the feature.</param>
        /// <param name="streamingLights">Streaming lights interface.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when the hardware for the instance is not supported by <see cref="UsbSymbolHighlightSupportedStreamingLight"/>.
        /// </exception>
        public UsbSymbolHighlightSupportedStreamingLight(string featureName, LightFeatureDescription featureDescription, IStreamingLights streamingLights) 
            : base(featureName, featureDescription, streamingLights)
        {
            // Check compatible hardware type.
            if(featureName != HardwareSpecs.GetStreamingLightFeatureNames()[Hardware.CatalinaReelBackLights])
            {
                throw new ArgumentException(
                    $"Feature name '{featureName}' corresponds to a Hardware type that is not supported by {nameof(UsbSymbolHighlightSupportedStreamingLight)}. " +
                    $"Currently the only Hardware type being supported is {Hardware.CatalinaReelBackLights}.",
                    nameof(featureName));
            }

            // With the current firmware implementation, only group zero has control of symbol highlights.
            var groupInfo = GetGroupInformation(0);

            if(groupInfo == null)
            {
                // Assume 5 reel configuration if no physical device is connected.
                maxReelIndex = 4;
            }
            else
            {
                // 5 reel cabinets have 50 back light LEDs. 
                maxReelIndex = groupInfo.Count == 50 ? 4 : 2;
            }
        }

        #region Symbol Tracking

        /// <summary>
        /// Update tracked symbol on the mechanical reels.
        /// </summary>
        /// <param name="reelNumber">reel that the symbol is located, 0 being the leftmost reel.</param>
        /// <param name="stopNumber">Stop position of the symbol, 0 being the first reel stop.</param>
        /// <param name="trackingColor">New tracking color for the symbol. Setting this to black will remove tracking.</param>
        /// <param name="trackingRow">Describes what row to track for the given symbol.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="reelNumber"/> or <paramref name="stopNumber"/> is out of range.
        /// </exception>
        public void SetSymbolTracking(int reelNumber, int stopNumber, Color trackingColor, SymbolTrackingRow trackingRow)
        {
            if(reelNumber < 0 || reelNumber > maxReelIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {maxReelIndex}.", "reelNumber");
            }

            if(stopNumber < 0 || stopNumber > MaxReelStopIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {MaxReelStopIndex}.", "stopNumber");
            }

            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            CachedTrackingCommands.Add(
                new SymbolTrackingData(reelNumber,
                                       stopNumber,
                                       new Rgb15(ColorSpaceConverter.ConvertColorToPackedColor(trackingColor)),
                                       trackingRow));
        }

        /// <summary>
        /// Clear all reel symbol tracking data.
        /// </summary>
        public void ClearAllTrackedSymbols()
        {
            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            StreamingLightsInterface.ClearSymbolHighlights(FeatureName, 0,
                new List<SymbolHighlightFeature> { SymbolHighlightFeature.SymbolTracking });
        }

        /// <summary>
        /// Clear all symbol tracking data from a given reel. 
        /// </summary>
        /// <param name="reelNumber">Reel to clear symbol tracking data from.</param>
        public void ClearAllTrackedSymbols(int reelNumber)
        {
            if(reelNumber < 0 || reelNumber > maxReelIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {maxReelIndex}.", "reelNumber");
            }

            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            StreamingLightsInterface.ClearSymbolHighlightReel(FeatureName, 0, reelNumber,
                new List<SymbolHighlightFeature> { SymbolHighlightFeature.SymbolTracking });
        }

        /// <summary>
        /// Clears all cached symbol tracking commands.
        /// </summary>
        public void ClearCachedTrackingData()
        {
            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            CachedTrackingCommands.Clear();
        }

        #endregion

        #region Hot Positions

        /// <summary>
        /// Add new hot position for a given symbol.
        /// </summary>
        /// <param name="reelNumber">
        /// The symbol's reel index, zero being the leftmost reel. 
        /// When a 3 reel cabinet is connected any data found after the 3rd reel is ignored.
        /// </param>
        /// <param name="stopNumber">The Symbols stop number, zero being the first stop.</param>
        /// <param name="hotPositionColor">Hot Position color. Setting this to black will remove the hot position.</param>
        /// <param name="windowStopIndex">Window location for the hot position. 0 is the top of the window, four is the bottom of the window.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when either <paramref name="reelNumber"/>, <paramref name="stopNumber"/>, <paramref name="hotPositionColor"/>, or 
        /// <paramref name="windowStopIndex"/> is out of range.
        /// </exception>
        /// <remarks>
        /// Hot position colors are limited to being either 0 or 255 because of hardware limitations. RGB values that are not set to max (255)
        /// do not consistently flash the correct color.
        /// </remarks>
        public void SetHotPosition(int reelNumber, int stopNumber, Color hotPositionColor, int windowStopIndex)
        {
            if(reelNumber < 0 || reelNumber > maxReelIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {maxReelIndex}.", "reelNumber");
            }

            if(stopNumber < 0 || stopNumber > MaxReelStopIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {MaxReelStopIndex}.", "stopNumber");
            }

            if(windowStopIndex < 0 || windowStopIndex > MaxWindowStopIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {MaxWindowStopIndex}.", "windowStopIndex");
            }

            if(hotPositionColor.R != 255 && hotPositionColor.R != 0 
                || hotPositionColor.G != 255 && hotPositionColor.G != 0
                || hotPositionColor.B != 255 && hotPositionColor.B != 0)
            {
                throw new ArgumentOutOfRangeException(
                    "hotPositionColor's RGB values must be either 0 or 255. " +
                    $"Given RGB color:\nR: {hotPositionColor.R} G: {hotPositionColor.G} B: {hotPositionColor.B}", 
                                                                    "hotPositionColor");
            }

            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            CachedHotPositionCommands.Add(
                new SymbolHotPositionData(reelNumber,
                                          stopNumber,
                                          new Rgb15(ColorSpaceConverter.ConvertColorToPackedColor(hotPositionColor)),
                                          windowStopIndex));
        }

        /// <summary>
        /// Clear all hot position tracking data. 
        /// </summary>
        public void ClearAllHotPositions()
        {
            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            StreamingLightsInterface.ClearSymbolHighlights(FeatureName, 0,
                new List<SymbolHighlightFeature> { SymbolHighlightFeature.SymbolHotPosition });
        }

        /// <summary>
        /// Clear all hot position data from a given reel. 
        /// </summary>
        /// <param name="reelNumber">Reel to clear hot position data from.</param>
        public void ClearAllHotPositions(int reelNumber)
        {
            if(reelNumber < 0 || reelNumber > maxReelIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {maxReelIndex}.", "reelNumber");
            }

            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            StreamingLightsInterface.ClearSymbolHighlightReel(FeatureName, 0, reelNumber,
                new List<SymbolHighlightFeature> { SymbolHighlightFeature.SymbolHotPosition });
        }

        /// <summary>
        /// Clears cached hot position commands.
        /// </summary>
        public void ClearCachedHotPositionData()
        {
            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            CachedHotPositionCommands.Clear();
        }

        #endregion

        #region Clear All

        /// <summary>
        /// Clear all highlight data from all reels. 
        /// </summary>
        public void ClearAllHighlights()
        {
            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            // Sending a single streaming light command will limit the potential for messages getting overridden in the buffer.
            StreamingLightsInterface.ClearSymbolHighlights(FeatureName, 0,
                new List<SymbolHighlightFeature> { SymbolHighlightFeature.SymbolTracking, SymbolHighlightFeature.SymbolHotPosition });
        }

        /// <summary>
        /// Clear all highlight data from a given reel. 
        /// </summary>
        /// <param name="reelNumber">Reel to clear highlight data from.</param>
        public void ClearAllHighlights(int reelNumber)
        {
            if(reelNumber < 0 || reelNumber > maxReelIndex)
            {
                throw new ArgumentOutOfRangeException(
                    $"Parameter must be a value inclusively between 0 and {maxReelIndex}.", "reelNumber");
            }

            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            StreamingLightsInterface.ClearSymbolHighlightReel(FeatureName, 0, reelNumber, 
                new List<SymbolHighlightFeature> { SymbolHighlightFeature.SymbolTracking, SymbolHighlightFeature.SymbolHotPosition });
        }

        /// <summary>
        /// Clear all cached highlight data.
        /// </summary>
        public void ClearAllCachedData()
        {
            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            ClearCachedTrackingData();
            ClearCachedHotPositionData();
        }

        #endregion

        #region Feature Control

        /// <summary>
        /// Enable or disable the symbol tracking/hot position feature.
        /// </summary>
        /// <param name="enable">Enable state to set the feature to.</param>
        public void EnableSymbolHighlightsFeature(bool enable)
        {
            if(!SymbolHighlightValidAndReady())
            {
                return;
            }

            if(enable)
            {
                StreamingLightsInterface.EnableGivenSymbolHighlights(FeatureName, 0,
                    new List<SymbolHighlightFeature>
                    { SymbolHighlightFeature.SymbolTracking, SymbolHighlightFeature.SymbolHotPosition });
            }
            else
            {
                StreamingLightsInterface.DisableSymbolHighlights(FeatureName, 0);
            }

            IsFeatureEnabled = enable;
        }

        /// <summary>
        /// Update the device with new symbol highlight commands.
        /// This method will not send a command if there is no cached highlight data.
        /// </summary>
        public void UpdateFeatureControl()
        {
            if(!SymbolHighlightValidAndReady()
                || CachedTrackingCommands.Count ==  0 && CachedHotPositionCommands.Count == 0)
            {
                return;
            }

            StreamingLightsInterface.SetSymbolHighlights(FeatureName, 0,
                CachedTrackingCommands.ToArray(), CachedHotPositionCommands.ToArray());

            ClearAllCachedData();
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Check if the device is valid and ready for symbol highlight commands.
        /// </summary>
        /// <returns>True if the device is valid and ready for symbol highlight commands.</returns>
        // ReSharper disable once MemberCanBePrivate.Global
        protected bool SymbolHighlightValidAndReady()
        {
            return SupportsSymbolHighlightsControl && IsValidandReady(0);
        }

        #endregion
    }
}