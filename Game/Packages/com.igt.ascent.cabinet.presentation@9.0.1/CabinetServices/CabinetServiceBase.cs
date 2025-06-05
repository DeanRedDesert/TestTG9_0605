// -----------------------------------------------------------------------
// <copyright file = "CabinetServiceBase.cs" company = "IGT">
//     Copyright (c) 2016 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.CabinetServices
{
    using System;
    using Communication.Cabinet;

    /// <summary>
    /// Basic cabinet service that connects to, and disconnects from, the Cabinet API.
    /// </summary>
    public abstract class CabinetServiceBase : ICabinetService, ICabinetServiceRestricted
    {
        #region IAsyncConnect

        /// <inheritdoc/>
        public bool AsyncConnectComplete { get; private set; }

        /// <inheritdoc/>
        public bool PostConnectComplete { get; private set; }

        /// <inheritdoc/>
        public void AsyncConnect(ICabinetLib cabinetLib)
        {
            CabinetLib = cabinetLib ?? throw new ArgumentNullException(nameof(cabinetLib));
            OnAsyncConnect();
            AsyncConnectComplete = true;
        }

        /// <inheritdoc/>
        public void PostConnect()
        {
            if(!AsyncConnectComplete)
            {
                throw new AsyncConnectException("Post Connect cannot be called before Async Connect completes.");
            }
            OnPostConnect();
            PostConnectComplete = true;
        }

        #endregion

        #region ICabinetServiceRestricted

        /// <inheritdoc/>
        public virtual void Disconnect()
        {
            CabinetLib = null;
        }

        #endregion

        #region Protected Members

        /// <summary>
        /// Instance of the cabinet library this service is using.
        /// </summary>
        protected ICabinetLib CabinetLib { get; private set; }

        /// <summary>
        /// Verify the cabinet is set and connected. Throws <see cref="CabinetDisconnectedException"/> 
        /// </summary>
        /// <exception cref="CabinetDisconnectedException">
        /// If <see cref="CabinetLib"/> is null, or <see cref="ICabinetLib.IsConnected"/> is false.
        /// </exception>
        // ReSharper disable once MemberCanBePrivate.Global
        protected void VerifyCabinetIsConnected()
        {
            if(CabinetLib?.IsConnected != true)
            {
                throw new CabinetDisconnectedException();
            }
        }

        /// <summary>
        /// Custom handler for sub classes to override during <see cref="AsyncConnect"/>.
        /// </summary>
        protected virtual void OnAsyncConnect()
        { }

        /// <summary>
        /// Custom handler for sub classes to override during <see cref="PostConnect"/>.
        /// </summary>
        protected virtual void OnPostConnect()
        { }

        #endregion
    }
}