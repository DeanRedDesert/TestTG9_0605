// -----------------------------------------------------------------------
// <copyright file = "FrameworkStubFactory.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework.Restricted
{
    using Interfaces;

    /// <summary>
    /// This factory creates the stub version of framework components to help tools like LDC.
    /// It is NOT to be used for any other purposes.
    /// </summary>
    public static class FrameworkStubFactory
    {
        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="IShellObservableCollection"/> object.
        /// </summary>
        /// <returns>
        /// A new <see cref="IShellObservableCollection"/> object for History mode.
        /// </returns>
        public static IShellObservableCollection CreateShellObservableCollection()
        {
            return new ShellObservableCollection();
        }

        /// <summary>
        /// Creates a new <see cref="ICoplayerObservableCollection"/> object.
        /// </summary>
        /// <returns>
        /// A new <see cref="ICoplayerObservableCollection"/> object for History mode.
        /// </returns>
        public static ICoplayerObservableCollection CreateCoplayerObservableCollection()
        {
            return new CoplayerObservableCollection();
        }

        /// <summary>
        /// Creates a new <see cref="IShellStateMachine"/> object for History mode.
        /// </summary>
        /// <returns>
        /// A new <see cref="IShellStateMachine"/> object for History mode.
        /// </returns>
        public static IShellStateMachine CreateShellHistoryStateMachine()
        {
            return new ShellHistoryStateMachine();
        }

        /// <summary>
        /// Creates a new <see cref="IGameStateMachine"/> object for History mode.
        /// </summary>
        /// <returns>
        /// A new <see cref="IGameStateMachine"/> object for History mode.
        /// </returns>
        public static IGameStateMachine CreateGameHistoryStateMachine()
        {
            return new CoplayerHistoryStateMachine();
        }

        /// <summary>
        /// Creates a new <see cref="IShellExposition"/> object.
        /// </summary>
        /// <returns>
        /// A new <see cref="IShellExposition"/> object with the default values.
        /// </returns>
        public static IShellExposition CreateShellExposition()
        {
            return new ShellExposition();
        }

        #endregion
    }
}