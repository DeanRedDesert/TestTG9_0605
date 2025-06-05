//-----------------------------------------------------------------------
// <copyright file = "ReleaseEgmProfile.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor.Profiles
{
    /// <summary>
    /// Release EGM build profile which defaults build type, tool connections, and mouse cursor.
    /// </summary>
    public class ReleaseEgmProfile : BaseBuildProfile
    {
        /// <inheritdoc />
        public override IgtGameParameters.GameType BuildTypeGui(IgtGameParameters.GameType current)
        {
            return IgtGameParameters.GameType.Standard;
        }

        /// <inheritdoc />
        public override IgtGameParameters.ConnectionType ToolConnectionsGui(IgtGameParameters.ConnectionType current)
        {
            return IgtGameParameters.ConnectionType.LocalHost;
        }

        /// <inheritdoc />
        public override bool MouseCursorGui(bool current)
        {
            return false;
        }

        /// <inheritdoc />
        public override bool MonoAotCompileGui(bool current)
        {
            return true;
        }

    }
}