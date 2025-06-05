// -----------------------------------------------------------------------
// <copyright file = "IGameStateMachine.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using Communication.Platform.CoplayerLib.Interfaces;

    /// <inheritdoc/>
    public interface IGameStateMachine
        : IStateMachine<IGameState, IGameFrameworkInitialization, IGameFrameworkExecution, ICoplayerLib>
    {
    }
}