// -----------------------------------------------------------------------
// <copyright file = "ShellRunnerFactory.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework.Restricted
{
    using System;
    using Communication.Platform;
    using Game.Core.Threading;
    using Interfaces;

    /// <summary>
    /// This factory provides APIs for creating a new <see cref="IShellRunner"/> object.
    /// It is to be used by SDK Entry scripts only.  Other SDK or game specific code should NOT use it.
    /// </summary>
    public static class ShellRunnerFactory
    {
        #region Public Methods

        /// <summary>
        /// Creates a new <see cref="IShellRunner"/> object
        /// </summary>
        /// <param name="shellConfigurator">
        /// The object providing configurations of a shell application.
        /// </param>
        /// <param name="platformFactory">
        /// The factory for creating platform objects.
        /// </param>
        /// <param name="threadHealthChecker">
        /// The object monitoring the health of a collection of worker threads.
        /// </param>
        /// <returns>
        /// A new <see cref="IShellRunner"/> object.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="shellConfigurator"/> is null.
        /// </exception>
        public static IShellRunner CreateShellRunner(IShellConfigurator shellConfigurator,
                                                     IPlatformFactory platformFactory,
                                                     IThreadHealthChecker threadHealthChecker)
        {
            return new ShellRunner(shellConfigurator, platformFactory, threadHealthChecker);
        }

        #endregion
    }
}