//-----------------------------------------------------------------------
// <copyright file = "SetTournamentSessionConfiguration.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using IGT.Game.Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// This class stores the tournament session configuration in Standalone mode.
    /// </summary>
    internal class SetTournamentSessionConfiguration
    {
        /// <summary>
        /// Flag that indicates if the tournament session configuration should be shown.
        /// </summary>
        private bool showTournamentSessionConfig;

        /// <summary>
        /// Gets the tournament session configuration data that stores the current configuration.
        /// </summary>
        public TournamentSessionConfiguration Data
        {
            get
            {
                var initialCredits = (EditorData.SessionType == TournamentSessionType.Credits ||
                                      EditorData.SessionType == TournamentSessionType.TimedCredits)
                    ? EditorData.InitialCredits
                    : 0;

                var playDuration = (EditorData.SessionType == TournamentSessionType.Timer ||
                                    EditorData.SessionType == TournamentSessionType.TimedCredits)
                    ? EditorData.PlayDuration
                    : 0;

                return new TournamentSessionConfiguration
                {
                    CountdownDuration = EditorData.CountdownDuration,
                    PlayDuration = playDuration,
                    InitialCredits = initialCredits,
                    SessionType = EditorData.SessionType
                };
            }
        }

        /// <summary>
        /// Gets the tournament session configuration data that stores the editor changes.
        /// For example, changes made to the timer session type for will be saved if the user
        /// selects the credit session type as the current configuration so that changes made to
        /// the timer session type will be recovered if the user decides to switch back to the timer
        /// session type.
        /// </summary>
        public TournamentSessionConfiguration EditorData { get; private set; }

        /// <summary>
        /// The class constructor.
        /// </summary>
        public SetTournamentSessionConfiguration()
        {
            EditorData = new TournamentSessionConfiguration();
        }

        /// <summary>
        /// Displays the tournament session configuration
        /// </summary>
        /// <remarks>
        /// Countdown is not available in Standalone tournament.
        /// </remarks>
        public void DisplayTournamentSessionConfiguration()
        {
            showTournamentSessionConfig = EditorGUILayout.Foldout(showTournamentSessionConfig,
                "Tournament Session Configuration");

            if(showTournamentSessionConfig)
            {
                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(SystemConfigEditor.FoldoutSize));
                EditorGUILayout.BeginVertical();

                EditorGUI.indentLevel++;
                EditorGUI.indentLevel++;

                EditorData.SessionType = (TournamentSessionType)EditorGUILayout.EnumPopup(new GUIContent("Session type",
                    "The tournament session type."), EditorData.SessionType);

                var showInitialCredits = false;
                var showPlayDuration = false;

                switch(EditorData.SessionType)
                {
                    case TournamentSessionType.Credits:
                        showInitialCredits = true;
                        break;

                    case TournamentSessionType.TimedCredits:
                        showInitialCredits = true;
                        showPlayDuration = true;
                        break;

                    case TournamentSessionType.Timer:
                        showPlayDuration = true;
                        break;
                }

                if(showInitialCredits)
                {
                    EditorData.InitialCredits = EditorGUILayout.IntField(new GUIContent("Initial Credits",
                        "The starting credits of the tournament session. This is only applicable for Credits or " +
                        "TimedCredits session types."), (int)EditorData.InitialCredits);
                }

                if(showPlayDuration)
                {
                    EditorData.PlayDuration = EditorGUILayout.IntField(new GUIContent("Play Duration",
                        "The duration (in seconds) of the tournament session. This is only applicable for Timed or " +
                        "TimedCredits session types."), EditorData.PlayDuration);
                }

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Updates the tournament session configuration editor data with the
        /// <paramref name="tournamentSessionConfiguration"/> provided.
        /// </summary>
        /// <param name="tournamentSessionConfiguration">
        /// The tournament session configuration read from the system configuration editor data file.
        /// </param>
        public void UpdateTournamentSessionConfigurationEditorData(
            TournamentSessionConfiguration tournamentSessionConfiguration)
        {
            if(tournamentSessionConfiguration != null)
            {
                EditorData = tournamentSessionConfiguration;
            }
        }
    }
}