// -----------------------------------------------------------------------
// <copyright file = "IShellStateMachine.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Interfaces
{
    using Communication.Platform.ShellLib.Interfaces;

    /// <inheritdoc/>
    public interface IShellStateMachine
        : IStateMachine<IShellState, IShellFrameworkInitialization, IShellFrameworkExecution, IShellLib>
    {
    }
}