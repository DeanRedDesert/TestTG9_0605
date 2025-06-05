//-----------------------------------------------------------------------
// <copyright file = "DevEgmProfile.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.SDKAssets.AscentBuildSettings.Editor.Profiles
{
    /// <summary>
    /// The development EGM/AOW profile which defaults the game type.
    /// </summary>
    public class DevEgmProfile : BaseBuildProfile
    {
        /// <inheritdoc />
        public override IgtGameParameters.GameType BuildTypeGui(IgtGameParameters.GameType current)
        {
            return IgtGameParameters.GameType.Standard;
        }
    }
}