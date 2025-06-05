// -----------------------------------------------------------------------
// <copyright file = "ShellTiltSender.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System;
    using System.Collections.Generic;
    using Communication.Platform.ShellLib.Interfaces;
    using Game.Core.Tilts;
    using TransactionalOperation;
    using TransactionalOperation.Interfaces;

    /// <summary>
    /// An implementation of <see cref="IShellTiltSender"/>.
    /// </summary>
    internal class ShellTiltSender : IShellTiltSender
    {
        private readonly IShellTiltController shellTiltController;
        private readonly ITransactionalOperationManager transactionalOperationManager;

        internal ShellTiltSender(IShellTiltController shellTiltController, ITransactionalOperationManager transactionalOperationManager)
            => (this.shellTiltController, this.transactionalOperationManager) = (shellTiltController, transactionalOperationManager);

        /// <inheritdoc/>
        public bool PostTilt(ITilt tilt, string key, IEnumerable<object> titleFormat, IEnumerable<object> messageFormat, int coplayerId)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentException(nameof(key));

            if(tilt == null)
                throw new ArgumentNullException(nameof(tilt));

            var transactionalOperation = new TransactionalOperation(transactionalOperationManager,
                                                                    "ShellTiltSender.PostTilt");

            var result = false;
            var invoked = transactionalOperation.Invoke(
                () => result = shellTiltController.PostTilt(tilt, key, titleFormat, messageFormat, coplayerId));

            return invoked && result;
        }

        /// <inheritdoc/>
        public bool ClearAllTilts(int coplayerId)
        {
            var transactionalOperation = new TransactionalOperation(transactionalOperationManager,
                                                                    "ShellTiltSender.ClearAllTilts");

            var result = false;
            var invoked = transactionalOperation.Invoke(() => result = shellTiltController.ClearAllTilts(coplayerId));

            return invoked && result;
        }

        /// <inheritdoc/>
        public bool ClearTilt(string key, int coplayerId)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentException(nameof(key));

            var transactionalOperation = new TransactionalOperation(transactionalOperationManager,
                                                                    "ShellTiltSender.ClearTilt");

            var result = false;
            var invoked = transactionalOperation.Invoke(() => result = shellTiltController.ClearTilt(key, coplayerId));

            return invoked && result;
        }

        /// <inheritdoc/>
        public bool IsTilted(int coplayerId)
        {
            return shellTiltController.IsTilted(coplayerId);
        }

        /// <inheritdoc/>
        public bool TiltPresent(string key, int coplayerId)
        {
            if(string.IsNullOrEmpty(key))
                throw new ArgumentException(nameof(key));

            return shellTiltController.TiltPresent(key, coplayerId);
        }
    }
}