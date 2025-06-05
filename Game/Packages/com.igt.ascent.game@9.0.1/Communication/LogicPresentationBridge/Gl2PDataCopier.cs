// -----------------------------------------------------------------------
// <copyright file = "Gl2PDataCopier.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    using System;

    /// <summary>
    /// This class is used to deep copy the GL2P data in a compact way.
    /// </summary>
    internal class Gl2PDataCopier
    {
        #region Fields

        /// <summary>
        /// The object used to deep copy the GL2P data.
        /// </summary>
        private readonly Gl2PDataCopyProcessor dataCopyProcessor = new Gl2PDataCopyProcessor
        {
            MetricsRecorder = new Gl2PDataCopyMetricsRecorder()
        };

        #endregion

        #region Public Methods

        /// <summary>
        /// Make a deep copy of a object and update the DataSize of the info.
        /// </summary>
        /// <typeparam name="T">Type being copied.</typeparam>
        /// <param name="data">Data to copy.</param>
        /// <param name="info">The data size of the copied message.</param>
        /// <returns>Copy of the passed data. If null is passed in, then null will be returned.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is null.</exception>
        public T DeepCopy<T>(T data, MessageInfo info)
        {
            if(info == null)
            {
                throw new ArgumentNullException("info");
            }

            dataCopyProcessor.MetricsRecorder.ResetSerializationSize();
            var copy = (T)dataCopyProcessor.DeepCopy(data);
            info.DataSize = dataCopyProcessor.MetricsRecorder.GetSerializationSize();

            return copy;
        }

        #endregion
    }
}