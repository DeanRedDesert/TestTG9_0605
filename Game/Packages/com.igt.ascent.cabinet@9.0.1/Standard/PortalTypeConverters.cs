//-----------------------------------------------------------------------
// <copyright file = "PortalTypeConverters.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System.Collections.Generic;
    using CSIGroupName = CSI.Schemas.GroupName;
    using CSIPortalBounds = CSI.Schemas.Internal.PortalBounds;
    using CSIPortalExtents = CSI.Schemas.Internal.PortalExtents;
    using CSIPortalInformation = CSI.Schemas.Internal.PortalInfo;
    using CSIPortalLocation = CSI.Schemas.Internal.PortalLocation;
    using CSIPortalPosition = CSI.Schemas.Internal.PortalPosition;
    using CSIPortalScreen = CSI.Schemas.Internal.PortalScreen;
    using CSIPortalSize = CSI.Schemas.Internal.PortalSize;
    using CSIPortalType = CSI.Schemas.Internal.PortalType;
    using CSIVideoTopperPortalSupport = CSI.Schemas.Internal.VideoTopperPortalSupport;

    /// <summary>
    /// Class for converting between interface types and CSI Schema types. 
    /// </summary>
    internal static class PortalTypeConverters
    {
        #region Private Conversion Dictionaries

        /// <summary>
        /// Mappings from <see cref="CSIPortalScreen"/> (CSI) to <see cref="PortalScreen"/> (SDK).
        /// </summary>
        private static readonly Dictionary<CSIPortalScreen, PortalScreen> MapToPublicPortalScreen
            = new Dictionary<CSIPortalScreen, PortalScreen>
            {
                { CSIPortalScreen.INVALID_SCREEN, PortalScreen.InvalidScreen },
                { CSIPortalScreen.PRIMARY,        PortalScreen.Primary },
                { CSIPortalScreen.SECONDARY,      PortalScreen.Secondary },
                { CSIPortalScreen.TOPPER,         PortalScreen.Topper },
                { CSIPortalScreen.DPP,            PortalScreen.Dpp },
            };

        /// <summary>
        /// Mappings from <see cref="PortalScreen"/> (SDK) to <see cref="CSIPortalScreen"/> (CSI).
        /// </summary>
        private static readonly Dictionary<PortalScreen, CSIPortalScreen> MapToCsiPortalScreen
            = new Dictionary<PortalScreen, CSIPortalScreen>
            {
                { PortalScreen.InvalidScreen, CSIPortalScreen.INVALID_SCREEN },
                { PortalScreen.Primary,       CSIPortalScreen.PRIMARY },
                { PortalScreen.Secondary,     CSIPortalScreen.SECONDARY },
                { PortalScreen.Topper,        CSIPortalScreen.TOPPER },
                { PortalScreen.Dpp,           CSIPortalScreen.DPP },
            };

        /// <summary>
        /// Mappings from <see cref="CSIPortalType"/> (CSI) to <see cref="PortalType"/> (SDK).
        /// </summary>
        private static readonly Dictionary<CSIPortalType, PortalType> MapToPublicPortalType
            = new Dictionary<CSIPortalType, PortalType>
            {
                { CSIPortalType.INVALID_TYPE, PortalType.InvalidType },
                { CSIPortalType.SCALE,        PortalType.Scale },
                { CSIPortalType.OVERLAY,      PortalType.Overlay },
                { CSIPortalType.MODAL,        PortalType.Modal }
            };

        /// <summary>
        /// Mappings from <see cref="PortalType"/> (SDK) to <see cref="CSIPortalType"/> (CSI).
        /// </summary>
        private static readonly Dictionary<PortalType, CSIPortalType> MapToCsiPortalType
            = new Dictionary<PortalType, CSIPortalType>
            {
                { PortalType.InvalidType, CSIPortalType.INVALID_TYPE },
                { PortalType.Scale,       CSIPortalType.SCALE },
                { PortalType.Overlay,     CSIPortalType.OVERLAY },
                { PortalType.Modal,       CSIPortalType.MODAL }
            };

        /// <summary>
        /// Mappings from <see cref="CSIPortalPosition"/> (CSI) to <see cref="PortalPosition"/> (SDK).
        /// </summary>
        private static readonly Dictionary<CSIPortalPosition, PortalPosition> MapToPublicPortalPosition
            = new Dictionary<CSIPortalPosition, PortalPosition>
            {
                { CSIPortalPosition.INVALID_PORTAL_POSITION, PortalPosition.InvalidPortalPosition },
                { CSIPortalPosition.LEFT,                    PortalPosition.Left },
                { CSIPortalPosition.RIGHT,                   PortalPosition.Right },
                { CSIPortalPosition.BOTTOM,                  PortalPosition.Bottom },
                { CSIPortalPosition.TOP,                     PortalPosition.Top },
                { CSIPortalPosition.FLOAT,                   PortalPosition.Float },
                { CSIPortalPosition.FULL,                    PortalPosition.Full },
                { CSIPortalPosition.CENTER,                  PortalPosition.Center },
            };

        /// <summary>
        /// Mappings from <see cref="PortalPosition"/> (SDK) to <see cref="CSIPortalPosition"/> (CSI).
        /// </summary>
        private static readonly Dictionary<PortalPosition, CSIPortalPosition> MapToCsiPortalPosition
            = new Dictionary<PortalPosition, CSIPortalPosition>
            {
                { PortalPosition.InvalidPortalPosition, CSIPortalPosition.INVALID_PORTAL_POSITION },
                { PortalPosition.Left,                  CSIPortalPosition.LEFT },
                { PortalPosition.Right,                 CSIPortalPosition.RIGHT },
                { PortalPosition.Bottom,                CSIPortalPosition.BOTTOM },
                { PortalPosition.Top,                   CSIPortalPosition.TOP },
                { PortalPosition.Float,                 CSIPortalPosition.FLOAT },
                { PortalPosition.Full,                  CSIPortalPosition.FULL },
                { PortalPosition.Center,                CSIPortalPosition.CENTER },
            };

        /// <summary>
        /// Mappings from <see cref="CSIGroupName"/> (CSI) to <see cref="SoundGroupName"/> (SDK).
        /// </summary>
        private static readonly Dictionary<CSIGroupName, SoundGroupName> MapToPublicGroupName
            = new Dictionary<CSIGroupName, SoundGroupName>
            {
                { CSIGroupName.ALARMS,                    SoundGroupName.Alarms },
                { CSIGroupName.TILTS,                     SoundGroupName.Tilts },
                { CSIGroupName.SYSTEM,                    SoundGroupName.System },
                { CSIGroupName.VOCAL,                     SoundGroupName.Vocal },
                { CSIGroupName.FEEDBACK,                  SoundGroupName.Feedback },
                { CSIGroupName.GAME_SPECIAL,              SoundGroupName.GameSpecial },
                { CSIGroupName.GAME_SOUNDS,               SoundGroupName.GameSounds },
                { CSIGroupName.ATTRACTS,                  SoundGroupName.Attracts },
                { CSIGroupName.GAME_PRESET_VOLUME_SOUNDS, SoundGroupName.GamePresetVolumeSounds },
            };

        /// <summary>
        /// Mappings from <see cref="SoundGroupName"/> (SDK) to <see cref="CSIGroupName"/> (CSI).
        /// </summary>
        private static readonly Dictionary<SoundGroupName, CSIGroupName> MapToCsiGroupName
            = new Dictionary<SoundGroupName, CSIGroupName>
            {
                { SoundGroupName.Alarms,                 CSIGroupName.ALARMS },
                { SoundGroupName.Tilts,                  CSIGroupName.TILTS },
                { SoundGroupName.System,                 CSIGroupName.SYSTEM },
                { SoundGroupName.Vocal,                  CSIGroupName.VOCAL },
                { SoundGroupName.Feedback,               CSIGroupName.FEEDBACK },
                { SoundGroupName.GameSpecial,            CSIGroupName.GAME_SPECIAL },
                { SoundGroupName.GameSounds,             CSIGroupName.GAME_SOUNDS },
                { SoundGroupName.Attracts,               CSIGroupName.ATTRACTS },
                { SoundGroupName.GamePresetVolumeSounds, CSIGroupName.GAME_PRESET_VOLUME_SOUNDS },
            };

        #endregion

        #region Public Conversion Methods

        /// <summary>
        /// Extension method to convert a <see cref="CSIPortalScreen"/> (CSI) to
        /// a <see cref="PortalScreen"/> (SDK).
        /// </summary>
        /// <param name="portalScreen">The <see cref="CSIPortalScreen"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static PortalScreen ToPublic(this CSIPortalScreen portalScreen)
        {
            return MapToPublicPortalScreen[portalScreen];
        }

        /// <summary>
        /// Extension method to convert a <see cref="PortalScreen"/> (SDK) to a
        /// <see cref="CSIPortalScreen"/> (CSI).
        /// </summary>
        /// <param name="portalScreen">The <see cref="PortalScreen"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CSIPortalScreen ToCsi(this PortalScreen portalScreen)
        {
            return MapToCsiPortalScreen[portalScreen];
        }

        /// <summary>
        /// Extension method to convert a <see cref="CSIPortalType"/> (CSI) to
        /// a <see cref="PortalType"/> (SDK).
        /// </summary>
        /// <param name="portalType">The <see cref="CSIPortalType"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static PortalType ToPublic(this CSIPortalType portalType)
        {
            return MapToPublicPortalType[portalType];
        }

        /// <summary>
        /// Extension method to convert a <see cref="PortalType"/> (SDK) to a
        /// <see cref="CSIPortalType"/> (CSI).
        /// </summary>
        /// <param name="portalType">The <see cref="PortalType"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CSIPortalType ToCsi(this PortalType portalType)
        {
            return MapToCsiPortalType[portalType];
        }

        /// <summary>
        /// Extension method to convert a <see cref="CSIPortalPosition"/> (CSI) to
        /// a <see cref="PortalPosition"/> (SDK).
        /// </summary>
        /// <param name="portalPosition">The <see cref="CSIPortalPosition"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static PortalPosition ToPublic(this CSIPortalPosition portalPosition)
        {
            return MapToPublicPortalPosition[portalPosition];
        }

        /// <summary>
        /// Extension method to convert a <see cref="PortalPosition"/> (SDK) to a
        /// <see cref="CSIPortalPosition"/> (CSI).
        /// </summary>
        /// <param name="portalPosition">The <see cref="PortalPosition"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CSIPortalPosition ToCsi(this PortalPosition portalPosition)
        {
            return MapToCsiPortalPosition[portalPosition];
        }

        /// <summary>
        /// Extension method to convert a <see cref="CSIGroupName"/> (CSI) to
        /// a <see cref="SoundGroupName"/> (SDK).
        /// </summary>
        /// <param name="groupName">The <see cref="CSIGroupName"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static SoundGroupName ToPublic(this CSIGroupName groupName)
        {
            return MapToPublicGroupName[groupName];
        }

        /// <summary>
        /// Extension method to convert a <see cref="SoundGroupName"/> (SDK) to a
        /// <see cref="CSIGroupName"/> (CSI).
        /// </summary>
        /// <param name="groupName">The <see cref="SoundGroupName"/> to convert.</param>
        /// <returns>The conversion result.</returns>
        public static CSIGroupName ToCsi(this SoundGroupName groupName)
        {
            return MapToCsiGroupName[groupName];
        }

        /// <summary>
        /// Extension method to convert a <see cref="PortalInformation"/> (SDK) object to a
        /// <see cref="CSIPortalInformation"/> (CSI) object.
        /// </summary> 
        /// <param name="portalInfo">The <see cref="PortalInformation"/> to convert.</param>
        /// <returns>Null if input is null, the converted object otherwise.</returns>
        public static CSIPortalInformation ToCsi(this PortalInformation portalInfo)
        {
            return portalInfo == null
                ? null
                : new CSIPortalInformation
                {
                    PortalId = portalInfo.PortalId,
                    Name = portalInfo.Name,
                    Priority = portalInfo.Priority,
                    Screen = portalInfo.Screen.ToCsi(),
                    Type = portalInfo.Type.ToCsi(),
                    Position = portalInfo.Position.ToCsi(),
                    Bounds = new CSIPortalBounds
                    {
                        Location = new CSIPortalLocation
                        {
                            X = portalInfo.Bounds.Location.X,
                            Y = portalInfo.Bounds.Location.Y
                        },
                        Size = new CSIPortalSize
                        {
                            Width = portalInfo.Bounds.Size.Width,
                            Height = portalInfo.Bounds.Size.Height
                        }
                    },
                    TextureSize = new CSIPortalSize
                    {
                        Width = portalInfo.TextureSize.Width,
                        Height = portalInfo.TextureSize.Height
                    },
                    Extents = new CSIPortalExtents
                    {
                        Min = new CSIPortalSize
                        {
                            Width = portalInfo.MinExtents.Width,
                            Height = portalInfo.MinExtents.Height
                        },
                        Max = new CSIPortalSize
                        {
                            Width = portalInfo.MaxExtents.Width,
                            Height = portalInfo.MaxExtents.Height
                        }
                    },
                    DefaultEMDIAccessToken = portalInfo.DefaultEmdiAccessToken,
                    SoundGroup = portalInfo.SoundGroup.ToCsi(),
                    TouchSupported = portalInfo.TouchSupported,
                    AudioSupported = portalInfo.AudioSupported,
                    ConfigGroup = portalInfo.ConfigGroup
                };
        }

        /// <summary>
        /// Extension method to convert a <see cref="CSIPortalInformation"/> (CSI) object to a
        /// <see cref="PortalInformation"/> (SDK) object.
        /// </summary> 
        /// <param name="portalInfo">The <see cref="CSIPortalInformation"/> to convert.</param>
        /// <returns>Null if input is null, the converted object otherwise.</returns>
        public static PortalInformation ToPublic(this CSIPortalInformation portalInfo)
        {
            return portalInfo == null 
                ? null
                : new PortalInformation(
                    portalInfo.PortalId,
                    portalInfo.Name,
                    portalInfo.Priority,
                    portalInfo.Screen.ToPublic(),
                    portalInfo.Type.ToPublic(),
                    portalInfo.Position.ToPublic(),
                    new Rect(new Point(portalInfo.Bounds.Location.X, portalInfo.Bounds.Location.Y),
                        new SizeRect(portalInfo.Bounds.Size.Width, portalInfo.Bounds.Size.Height)),
                    new SizeRect(portalInfo.TextureSize.Width, portalInfo.TextureSize.Height),
                    new SizeRect(portalInfo.Extents.Min.Width, portalInfo.Extents.Min.Height),
                    new SizeRect(portalInfo.Extents.Max.Width, portalInfo.Extents.Max.Height),
                    portalInfo.DefaultEMDIAccessToken,
                    portalInfo.SoundGroup.ToPublic(),
                    portalInfo.TouchSupported,
                    portalInfo.AudioSupported,
                    portalInfo.ConfigGroup);
        }

        /// <summary>
        /// Extension method to convert a <see cref="CSIVideoTopperPortalSupport"/> (CSI) value to a
        /// <see cref="VideoTopperPortalSupport"/> (SDK) value.
        /// </summary> 
        /// <param name="portalSupport">The <see cref="CSIVideoTopperPortalSupport"/> to convert.</param>
        /// <returns>The converted result.</returns>
        public static VideoTopperPortalSupport ToPublic(this CSIVideoTopperPortalSupport portalSupport)
        {
            switch(portalSupport)
            {
                case CSIVideoTopperPortalSupport.VIDEO_TOPPER_PORTALS_SUPPORTED:
                    return VideoTopperPortalSupport.PortalSupported;
                case CSIVideoTopperPortalSupport.VIDEO_TOPPER_PORTALS_UNSUPPORTED:
                    return VideoTopperPortalSupport.PortalUnsupported;
                default:
                    return VideoTopperPortalSupport.Unknown;
            }
        }

        #endregion
    }
}
