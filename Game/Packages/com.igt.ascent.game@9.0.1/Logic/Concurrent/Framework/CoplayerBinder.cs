// -----------------------------------------------------------------------
// <copyright file = "CoplayerBinder.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using Communication.Platform.CoplayerLib.Interfaces;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Threading;

    /// <summary>
    /// This class holds all components related to a coplayer.
    /// </summary>
    internal sealed class CoplayerBinder
    {
        #region Properties

        /// <summary>
        /// The id of the F2X session for this coplayer.
        /// </summary>
        public int SessionId { get; }

        /// <summary>
        /// <see cref="ShellThemeInfo"/> for this coplayer.
        /// </summary>
        public ShellThemeInfo ThemeInfo { get; private set; }

        /// <summary>
        /// Active thread for controlling this coplayer.
        /// </summary>
        public WorkerThread CoplayerThread { get; }

        /// <summary>
        /// Coplayer runner for this coplayer.
        /// </summary>
        public CoplayerRunner CoplayerRunner { get; }

        /// <summary>
        /// Reference to the coplayer lib for this coplayer.
        /// </summary>
        public ICoplayerLibRestricted CoplayerLibRestricted { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a coplayer binder.
        /// </summary>
        /// <param name="sessionId">The session id.</param>
        /// <param name="themeInfo">The theme info.</param>
        /// <param name="coplayerThread">The coplayer thread.</param>
        /// <param name="coplayerRunner">The coplayer runner.</param>
        /// <param name="coplayerLibRestricted">The coplayer lib.</param>
        public CoplayerBinder(int sessionId,
                              ShellThemeInfo themeInfo,
                              WorkerThread coplayerThread,
                              CoplayerRunner coplayerRunner,
                              ICoplayerLibRestricted coplayerLibRestricted)
        {
            SessionId = sessionId;
            ThemeInfo = themeInfo;
            CoplayerThread = coplayerThread;
            CoplayerRunner = coplayerRunner;
            CoplayerLibRestricted = coplayerLibRestricted;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set a new <see cref="ShellThemeInfo"/> on this binder.
        /// </summary>
        /// <param name="themeInfo">The new theme info.</param>
        public void UpdateThemeInfo(ShellThemeInfo themeInfo)
        {
            ThemeInfo = themeInfo;
        }

        #endregion
    }
}