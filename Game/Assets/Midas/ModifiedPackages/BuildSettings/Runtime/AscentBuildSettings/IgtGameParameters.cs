//-----------------------------------------------------------------------
// <copyright file = "IgtGameParameters.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Core.Communication;
    using UnityEngine;

    /// <summary>
    /// Class for representing game parameter information.
    /// </summary>
    /// <remarks>
    /// This class uses a custom file format for saving the game parameters. The custom format is used to reduce the
    /// number of dependencies this class has to only items available from the system assembly. Formats such as XML
    /// or JSON would introduce additional assembly dependencies.
    /// </remarks>
    [Serializable]
    public class IgtGameParameters : IgtParameters
    {
        #region Public Nested Types

        /// <summary>
        /// Enumeration listing the different types of game configurations.
        /// </summary>
        public enum GameType
        {
            /// <summary>
            /// Game configured for fast play. Does not use safe storage and the presentation is replaced with a fast
            /// play script.
            /// </summary>
            FastPlay,

            /// <summary>
            /// Game is configured to run with the foundation.
            /// </summary>
            Standard,

            /// <summary>
            /// Standalone game build which uses memory in place of safe storage. Such a build has no persistence.
            /// </summary>
            StandaloneNoSafeStorage,

            /// <summary>
            /// Standalone game build which uses xml files for safe storage. Builds like this have basic power-hit
            /// tolerance.
            /// </summary>
            /// <remarks>
            /// This standalone game type uses a readable safe storage file, but at the cost of lower performance.
            /// </remarks>
            StandaloneFileBackedSafeStorage,

            /// <summary>
            /// Web and mobile game type.
            /// </summary>
            WebAndMobile,

            /// <summary>
            /// Game type for running on the UC.
            /// </summary>
            UniversalController,

            /// <summary>
            /// Standalone game build which uses binary files for safe storage. Builds like this have basic power-hit
            /// tolerance.
            /// </summary>
            /// <remarks>
            /// This standalone game type uses a binary format for safe storage files to increase runtime performance,
            /// but the generated file will not be readable. 
            /// </remarks>
            StandaloneBinaryFileBackedSafeStorage
        }

        /// <summary>
        /// Enumeration which specifies the types of connections allowed.
        /// </summary>
        public enum ConnectionType
        {
            /// <summary>
            /// Allow any IP address to connect to services.
            /// </summary>
            AnyIp,

            /// <summary>
            /// Allow only the local host to connect to services.
            /// </summary>
            LocalHost
        }

        #endregion

        #region Private Fields and Properties

        /// <summary>
        /// The default port that fast play listens to.
        /// </summary>
        private const int FastPlayDefaultPort = 6003;

        /// <summary>
        /// Default target to use for new parameters files or during upgrades.
        /// </summary>
        private static readonly FoundationTarget DefaultTarget = FoundationTargetExtensions.FirstAscent();

        /// <summary>
        /// The full path for the game parameters file.
        /// </summary>
        private static string GameParametersPath
        {
            get
            {
                // The game parameters file needs to be loaded prior to the creation of the game lib so it cannot use the
                // mount point specified by the game lib.
                var gameParametersPath = Application.dataPath;

                if(Application.platform == RuntimePlatform.WindowsPlayer ||
                   Application.platform == RuntimePlatform.WindowsEditor)
                {
                    gameParametersPath += "/..";
                }

                gameParametersPath += "/" + GameParametersFileName;
                return gameParametersPath;
            }
        }

        /// <summary>
        /// String representing <see cref="FoundationTarget"/> to prevent deserialization issues when
        /// targets are removed or their backing values change.
        /// </summary>
        [SerializeField]
        private string targetedFoundationString = Enum.GetName(typeof(FoundationTarget), DefaultTarget);

        #endregion

        #region Public Inspector Fields

        /// <summary>
        /// The type of the game.
        /// </summary>
        public GameType Type = GameType.StandaloneBinaryFileBackedSafeStorage;

        /// <summary>
        /// The type of tool connections the game is to allow.
        /// </summary>
        public ConnectionType ToolConnections;

        /// <summary>
        /// Flag which indicates if the game should show a mouse cursor or not.
        /// </summary>
        public bool ShowMouseCursor;

        /// <summary>
        /// The port that fast play listens to.
        /// </summary>
        public int FastPlayPort = FastPlayDefaultPort;

        /// <summary>
        /// The Foundation build being targeted.
        /// </summary>
        public FoundationTarget TargetedFoundation
        {
            get
            {
                var target = DefaultTarget;
                try
                {
                    target = (FoundationTarget)Enum.Parse(typeof(FoundationTarget), targetedFoundationString);
                }
                catch
                {
                    // Use DefaultTarget if target string doesn't exist as a valid target.
                }
                return target;
            }
            set { targetedFoundationString = Enum.GetName(typeof(FoundationTarget), value); }
        }

        /// <summary>
        /// Whether to fit all mointors on a single display.
        /// </summary>
        public bool FitToScreen;

        /// <summary>
        /// Flag indicating if all monitors should be force enabled on build.
        /// </summary>
        public bool ForceEnableAllMonitors;

        #endregion

        #region Public Constants

        /// <summary>
        /// File name for the game parameters file.
        /// </summary>
        public const string GameParametersFileName = "GameParameters.config";

        #endregion

        #region Constructor

        /// <summary>
        /// Construct a default instance of the class.
        /// </summary>
        public IgtGameParameters() : base(DefaultTarget)
        {
            FileContent = new Dictionary<string, ITypeHandler>
            {
                {
                    "Type",
                    new TypeHandler(
                        stringValue => Type = (GameType)Enum.Parse(typeof(GameType), stringValue),
                        () => Enum.GetName(typeof(GameType), Type),
                        "Valid values: " + string.Join(", ", Enum.GetNames(typeof(GameType))))
                },
                {
                    "ToolConnections",
                    new TypeHandler(
                        stringValue =>
                            ToolConnections = (ConnectionType)Enum.Parse(typeof(ConnectionType), stringValue),
                        () => Enum.GetName(typeof(ConnectionType), ToolConnections),
                        "Valid values: " + string.Join(", ", Enum.GetNames(typeof(ConnectionType))))
                },
                {
                    "ShowMouseCursor",
                    new TypeHandler(stringValue => ShowMouseCursor = bool.Parse(stringValue),
                        () => ShowMouseCursor.ToString(CultureInfo.InvariantCulture),
                        "Valid values: True, False")
                },
                {
                    "FastPlayPort",
                    new TypeHandler(stringValue => FastPlayPort = int.Parse(stringValue),
                        () => FastPlayPort.ToString(CultureInfo.InvariantCulture),
                        "The port that the Fast play listens to. Example: 6003")
                },
                {
                    "TargetedFoundation",
                    new TypeHandler(stringValue => TargetedFoundation =
                        ParseTarget(stringValue),
                        () => Enum.GetName(typeof(FoundationTarget), TargetedFoundation),
                        "Valid targets: " + string.Join(", ", Enum.GetNames(typeof(FoundationTarget)).Where(
                            value => value != "All" && value != "AllAscent").ToArray()))
                },
                {
                    "FitToScreen",
                    new TypeHandler(stringValue => FitToScreen = bool.Parse(stringValue),
                        () => FitToScreen.ToString(CultureInfo.InvariantCulture),
                        "Fit all windows on the main monitor. Valid values: True, False")
                },
                {
                    "ForceEnableAllMonitors",
                    new TypeHandler(stringValue => ForceEnableAllMonitors = bool.Parse(stringValue),
                        () => ForceEnableAllMonitors.ToString(CultureInfo.InvariantCulture),
                        "Force enable all monitors on game build. Valid values: True, False")
                }
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Load the game parameters. The game parameters control the games configuration including the game type.
        /// </summary>
        public static IgtGameParameters Load()
        {
            var parameters = new IgtGameParameters();
            parameters.Load(GameParametersPath);

            return parameters;
        }

        /// <summary>
        /// Saves the game parameters to <see cref="GameParametersPath"/>.
        /// </summary>
        public void Save()
        {
            Save(GameParametersPath);
        }

        /// <summary>
        /// Configure the game parameters object for a fast play game.
        /// </summary>
        public void ConfigureFastPlay()
        {
            ToolConnections = ConnectionType.LocalHost;
            ShowMouseCursor = false;
            FastPlayPort = FastPlayDefaultPort;
        }

        #endregion

        #region Public Static Methods

        /// <summary>
        /// Create a parameters object configured for release.
        /// </summary>
        /// <param name="targetFoundation">The foundation to target.</param>
        /// <returns>Release configured parameters.</returns>
        public static IgtGameParameters CreateReleaseParameters(FoundationTarget targetFoundation)
        {
            var parameters = new IgtGameParameters();

            // Do not use an initializer list here, the serialization and constructor will override.
            parameters.Type = GameType.Standard;
            parameters.ToolConnections = ConnectionType.LocalHost;
            parameters.ShowMouseCursor = false;
            parameters.FastPlayPort = FastPlayDefaultPort;
            parameters.TargetedFoundation = targetFoundation;
            parameters.ForceEnableAllMonitors = true;

            return parameters;
        }

        /// <summary>
        /// Create a parameters object configured for a Universal Controller.
        /// </summary>
        /// <param name="foundationTarget">The foundation to target.</param>
        /// <returns>UC-configured parameters.</returns>
        public static IgtGameParameters CreateUcParameters(FoundationTarget foundationTarget)
        {
            var parameters = new IgtGameParameters();

            parameters.Type = GameType.UniversalController;
            parameters.ToolConnections = ConnectionType.LocalHost;
            parameters.ShowMouseCursor = false;
            parameters.FastPlayPort = FastPlayDefaultPort;
            parameters.TargetedFoundation = foundationTarget;

            return parameters;
        }

        #endregion
    }
}