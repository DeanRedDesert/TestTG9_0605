//-----------------------------------------------------------------------
// <copyright file = "FastPlayProfile.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor.Profiles
{
    /// <summary>
    /// Fast Play build profile which defaults the build type, tool connections, and mouse cursor settings.
    /// </summary>
    public class FastPlayProfile : BaseBuildProfile
    {
        /// <inheritdoc />
        public override IgtGameParameters.GameType BuildTypeGui(IgtGameParameters.GameType current)
        {
            return IgtGameParameters.GameType.FastPlay;
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