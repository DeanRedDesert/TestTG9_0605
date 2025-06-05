//-----------------------------------------------------------------------
// <copyright file = "UniversalControllerBuildProfile.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor.Profiles
{
    using System;
    using Core.Communication;

    /// <summary>
    /// Universal Controller build profile which defaults the build type, tool connections, mouse cursor, and foundation target.
    /// </summary>
    public class UniversalControllerBuildProfile : BaseBuildProfile
    {
        /// <summary>
        /// List of UC foundation target names based on the foundation target enum.
        /// </summary>
        private readonly string[] ucFoundationTargetNames =
        {
            Enum.GetName(typeof(FoundationTarget), FoundationTarget.UniversalController),
            Enum.GetName(typeof(FoundationTarget), FoundationTarget.UniversalController2)
        };

        /// <inheritdoc />
        public override IgtGameParameters.GameType BuildTypeGui(IgtGameParameters.GameType current)
        {
            return IgtGameParameters.GameType.UniversalController;
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
            return false;
        }

        /// <inheritdoc />
        public override FoundationTarget FoundationTargetGui(FoundationTarget current)
        {
            return DoFoundationTargetGui(ucFoundationTargetNames, current);
        }
    }
}
