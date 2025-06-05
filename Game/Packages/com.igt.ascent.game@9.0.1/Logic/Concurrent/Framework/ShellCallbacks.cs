// -----------------------------------------------------------------------
// <copyright file = "ShellCallbacks.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using IGT.Ascent.Restricted.EventManagement.Interfaces;

    /// <summary>
    /// This class holds a collection of objects to help a coplayer runner
    /// communicate with the shell runner.
    /// </summary>
    /// <remarks>
    /// These objects will be used by coplayers on coplayer threads, hence
    /// all the objects exposed here must support thread safety.
    /// </remarks>
    internal sealed class ShellCallbacks
    {
        #region Properties

        /// <summary>
        /// Object used to query the shell about configurations of the coplayer.
        /// </summary>
        public IShellConfigQuery ConfigQuery { get; set; }

        /// <summary>
        /// Object used to forward parcel calls to the shell which will in turn
        /// send the parcel calls out to Foundation on behalf of the coplayer.
        /// </summary>
        public IShellParcelCallSender ParcelCallSender { get; set; }

        /// <summary>
        /// Object used to post events to the shell.
        /// </summary>
        public IEventPoster EventPoster { get; set; }

        /// <summary>
        /// Object used to enqueue events to the shell.
        /// </summary>
        public IEventQueuer EventQueuer { get; set; }

        /// <summary>
        /// Object used to query history information from the shell.
        /// </summary>
        public IShellHistoryQuery HistoryQuery { get; set; }

        /// <summary>
        /// Object used to send tilt information to the shell.
        /// </summary>
        public IShellTiltSender ShellTiltSender { get; set; }

        #endregion
    }
}