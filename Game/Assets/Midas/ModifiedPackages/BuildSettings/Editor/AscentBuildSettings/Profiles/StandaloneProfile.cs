//-----------------------------------------------------------------------
// <copyright file = "StandaloneProfile.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor.Profiles
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;

    /// <summary>
    /// Standalone build profile which provides two build type options for safe storage support.
    /// </summary>
    public class StandaloneProfile : BaseBuildProfile
    {
        /// <summary>
        /// Types of standalone builds to correlate to GameType options.
        /// </summary>
        private enum StandaloneType
        {
            NoSafeStorage,
            FileBackedSafeStorage,
            BinaryFileBackedSafeStorage
        }

        /// <summary>
        /// Pair the GameType enum to appropriate StandaloneType enum value to use for a reduced-option build type gui.
        /// </summary>
        private readonly Dictionary<IgtGameParameters.GameType, StandaloneType> typePairs = new Dictionary
            <IgtGameParameters.GameType, StandaloneType>
        {
            { IgtGameParameters.GameType.StandaloneNoSafeStorage, StandaloneType.NoSafeStorage },
            { IgtGameParameters.GameType.StandaloneFileBackedSafeStorage, StandaloneType.FileBackedSafeStorage },
            { IgtGameParameters.GameType.StandaloneBinaryFileBackedSafeStorage, StandaloneType.BinaryFileBackedSafeStorage }
        };

        /// <inheritdoc />
        public override IgtGameParameters.GameType BuildTypeGui(IgtGameParameters.GameType current)
        {
            var currentConverted = typePairs.ContainsKey(current) ? typePairs[current] : StandaloneType.NoSafeStorage;

            var result = (StandaloneType)EditorGUILayout.EnumPopup("Safe Storage", currentConverted);
            return typePairs.FirstOrDefault(pair => pair.Value == result).Key;
        }
    }
}