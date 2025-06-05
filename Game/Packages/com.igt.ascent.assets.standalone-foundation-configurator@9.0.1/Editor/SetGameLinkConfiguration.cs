//-----------------------------------------------------------------------
// <copyright file = "SetGameLinkConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which sets the GameLink configuration.  GameLink is an extension class in the G2S Protocol that allows a G2S
    /// host to exchange data with a game that is installed on the EGM.  The GameLink class extension is proprietary to IGT.
    /// The Ascent foundation uses an implementation of STOMP to allow a game to communicate with the G2S GameLink class.
    /// </summary>
    internal class SetGameLinkConfiguration
    {
        #region Private Fields

        /// <summary>
        /// A boolean flag that indicates if the GameLink configuration shoud be shown or not.
        /// </summary>
        private bool showGameLinkConfiguration;

        /// <summary>
        /// A boolean flag that indicates if the GameLink configuration is to be written the system config file or not.
        /// </summary>
        private bool supported;

        /// <summary>
        /// The default hostname of the MiniMOM STOMP broker on the Foundation.
        /// </summary>
        private const string DefaultHostname = "127.0.0.1";

        /// <summary>
        /// The default port of the MiniMOM STOMP broker on the Foundation.
        /// </summary>
        private const int DefaultPort = 61613;

        /// <summary>
        /// The default major version of the GameLink implementation.
        /// </summary>
        private const uint DefaultMajorVersion = 1;

        /// <summary>
        /// The default minor version of the GameLink implementation.
        /// </summary>
        private const uint DefaultMinorVersion = 0;

        #endregion

        #region Public Fields

        /// <summary>
        /// The STOMP broker configuration data that is being written to the system config file in
        /// <see cref="SystemConfigFileHelper"/>.
        /// </summary>
        /// <returns>
        /// The STOMP broker configuration data.
        /// </returns>
        public StompBrokerConfiguration Data
        {
            get { return supported ? EditorData : null; }
        }

        /// <summary>
        /// The STOMP broker configuration Editor data that is being written to the system config editor in
        /// <see cref="SystemConfigFileHelper"/>.
        /// </summary>
        /// <returns>
        /// The STOMP broker configuration Editor data.
        /// </returns>
        public StompBrokerConfiguration EditorData { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs an instance of the <see cref="SetGameLinkConfiguration"/>.
        /// </summary>
        public SetGameLinkConfiguration()
        {
            EditorData = new StompBrokerConfiguration
            {
                Hostname = DefaultHostname,
                Port = DefaultPort,
                Version = new Version
                {
                    Major = DefaultMajorVersion,
                    Minor = DefaultMinorVersion
                }
            };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Sets the supported flag to true if the STOMP broker configuration exists in the system config file.
        /// </summary>
        public void SetSupportedFlag()
        {
            supported = true;
        }

        /// <summary>
        /// Sets the values for the GameLink configuration from the system config editor file.
        /// </summary>
        /// <param name="configuration">
        /// The STOMP broker configuration data.
        /// </param>
        public void UpdateGameLinkConfiguration(StompBrokerConfiguration configuration)
        {
            if(configuration != null)
            {
                EditorData.Hostname = configuration.Hostname;
                EditorData.Port = configuration.Port;
                EditorData.Version = configuration.Version;
            }
        }

        #endregion

        #region GameLink Configuration Foldout

        /// <summary>
        /// Foldout for the GameLink configuration.
        /// </summary>
        public void DisplayGameLinkConfiguration()
        {
            showGameLinkConfiguration = EditorGUILayout.Foldout(showGameLinkConfiguration, "GameLink Configurations");
            if(showGameLinkConfiguration)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                EditorGUILayout.BeginVertical();

                supported = EditorGUILayout.BeginToggleGroup("Supported", supported);

                EditorData.Hostname = EditorGUILayout.TextField(
                    new GUIContent("Hostname", "The hostname used to connect to the STOMP broker."),
                    CheckHostname(EditorData.Hostname));

                EditorData.Port = EditorGUILayout.IntField(
                    new GUIContent("Port", "The port used to connect to the STOMP broker."), CheckPort(EditorData.Port));

                EditorGUILayout.LabelField("Version");

                EditorGUI.indentLevel++;

                EditorData.Version.Major = (uint)EditorGUILayout.IntField(
                    new GUIContent("Major", "The major version will change to indicate breaking behavior."),
                    (int)EditorData.Version.Major);

                EditorData.Version.Minor = (uint)EditorGUILayout.IntField(
                    new GUIContent("Minor", "The minor version will change to indicate new non-breaking behavior."),
                    (int)EditorData.Version.Minor);

                EditorGUI.indentLevel--;

                EditorGUILayout.EndToggleGroup();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks if the hostname is null or empty. If it is null or empty, returns the default STOMP broker hostname.
        /// </summary>
        /// <param name="value">
        /// The hostname to verify.
        /// </param>
        /// <returns>
        /// The hostname if it is not null or empty, otherwise the default STOMP broker hostname.
        /// </returns>
        private static string CheckHostname(string value)
        {
            return string.IsNullOrEmpty(value) ? DefaultHostname : value;
        }

        /// <summary>
        /// Checks if the port is negative. If it is negative, returns the default STOMP broker port.
        /// </summary>
        /// <param name="value">
        /// The port to verify.
        /// </param>
        /// <returns>
        /// The port if it is not negative, otherwise the default STOMP broker port.
        /// </returns>
        private static int CheckPort(int value)
        {
            return value < 0 ? DefaultPort : value;
        }

        #endregion
    }
}
