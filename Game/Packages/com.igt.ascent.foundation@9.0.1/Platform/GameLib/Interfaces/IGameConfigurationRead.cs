// -----------------------------------------------------------------------
// <copyright file = "IGameConfigurationRead.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using Platform.Interfaces;

    /// <summary>
    /// This interface defines the methods to request information about custom configuration items by the game client.
    /// </summary>
    public interface IGameConfigurationRead :
        IGenericConfigurationRead<GameConfigurationKey, GameConfigurationScopeKey>
    {
    }
}