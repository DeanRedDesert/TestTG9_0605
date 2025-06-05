//-----------------------------------------------------------------------
// <copyright file = "SetGameSubMode.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.SystemConfiguration.Editor
{
    using Core.Communication.Foundation.Standalone.Registries;
    using Core.Communication.Standalone.Schemas;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Class which stores the game sub-mode selection for the Standalone game mode.
    /// </summary>
    internal class SetGameSubMode
    {
        /// <summary>
        /// The game sub-mode type selected.
        /// </summary>
        public GameSubModeType Data { get; private set; }

        /// <summary>
        /// The class constructor.
        /// </summary>
        /// <remarks>
        /// The game sub-mode type is set to "Standard" by default.
        /// </remarks>
        public SetGameSubMode()
        {
            Data = new GameSubModeType();
        }

        /// <summary>
        /// Updates the data with the <paramref name="gameSubModeType" /> passed in.
        /// </summary>
        /// <param name="gameSubModeType">The game sub-mode type to update to.</param>
        public void UpdateGameSubModeWithEditorData(GameSubModeType gameSubModeType)
        {
            Data = gameSubModeType;
        }

        /// <summary>
        /// Updates the data with the <paramref name="payvarType" /> passed in.
        /// </summary>
        /// <param name="payvarType">
        /// The payvar type of the default paytable.
        /// </param>
        public void UpdateGameSubMode(PayvarType payvarType)
        {
            Data.Type = (payvarType == PayvarType.Tournament)
                ? GameSubModeString.Tournament
                : GameSubModeString.Standard;
        }

        /// <summary>
        /// Displays the game sub-mode type in the editor window.
        /// </summary>
        public void DisplayGameSubMode()
        {
            EditorGUILayout.LabelField(
                new GUIContent("Game sub-mode",
                    "This readonly game sub-mode field is set based on the payvar registry type defined in the default paytable registry."),
                new GUIContent(Data.Type.ToString()));
        }
    }
}