//-----------------------------------------------------------------------
// <copyright file = "F2XUgpPid.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.Pid
{
    using System;
    using Ascent.Restricted.EventManagement.Interfaces;
    using Interfaces;

    /// <summary>
    /// Implementation of the UgpPid extended interface that is backed by F2X.
    /// </summary>
    internal class F2XUgpPid : IUgpPid, IInterfaceExtension
    {
        #region Private Fields

        /// <summary>
        /// The UgpPid category handler.
        /// </summary>
        private readonly IUgpPidCategory ugpPidCategory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes an instance of <see cref="F2XUgpPid"/>.
        /// </summary>
        /// <param name="ugpPidCategory">
        /// The UgpPid category used to communicate with the foundation.
        /// </param>
        /// <param name="transactionalEventDispatcher">
        /// Interface for processing a transactional event.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when any of the arguments is null.
        /// </exception>
        public F2XUgpPid(IUgpPidCategory ugpPidCategory,
                         IEventDispatcher transactionalEventDispatcher)
        {
            if(transactionalEventDispatcher == null)
            {
                throw new ArgumentNullException(nameof(transactionalEventDispatcher));
            }

            this.ugpPidCategory = ugpPidCategory ?? throw new ArgumentNullException(nameof(ugpPidCategory));

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, PidActivated);

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, IsServiceRequestedChanged);

            transactionalEventDispatcher.EventDispatchedEvent +=
                            (sender, dispatchedEvent) => dispatchedEvent.RaiseWith(this, PidConfigurationChanged);
        }

        #endregion

        #region IUgpPid Implementation

        /// <inheritdoc/>
        public event EventHandler<PidActivationEventArgs> PidActivated;

        /// <inheritdoc/>
        public event EventHandler<PidServiceRequestedChangedEventArgs> IsServiceRequestedChanged;

        /// <inheritdoc/>
        public event EventHandler<PidConfigurationChangedEventArgs> PidConfigurationChanged;

        /// <inheritdoc/>
        public bool IsServiceRequested => ugpPidCategory.IsServiceRequested;

        /// <inheritdoc/>
        public void StartTracking()
        {
            ugpPidCategory.StartTracking();
        }

        /// <inheritdoc/>
        public void StopTracking()
        {
            ugpPidCategory.StopTracking();
        }

        /// <inheritdoc/>
        public PidSessionData GetSessionData()
        {
            return ugpPidCategory.GetSessionData().ToPublic();
        }

        /// <inheritdoc/>
        public PidConfiguration GetPidConfiguration()
        {
            return ugpPidCategory.GetPidConfiguration().ToPublic();
        }

        /// <inheritdoc/>
        public void ActivationStatusChanged(bool currentStatus)
        {
            ugpPidCategory.ActivationStatusChanged(currentStatus);
        }

        /// <inheritdoc/>
        public void GameInformationScreenEntered()
        {
            ugpPidCategory.GameInformationScreenEntered();
        }

        /// <inheritdoc/>
        public void SessionInformationScreenEntered()
        {
            ugpPidCategory.SessionInformationScreenEntered();
        }

        /// <inheritdoc/>
        public void AttendantServiceRequested()
        {
            ugpPidCategory.AttendantServiceRequested();
        }

        /// <inheritdoc/>
        public void RequestForcePayout()
        {
            ugpPidCategory.RequestForcePayout();
        }

        #endregion
    }
}
