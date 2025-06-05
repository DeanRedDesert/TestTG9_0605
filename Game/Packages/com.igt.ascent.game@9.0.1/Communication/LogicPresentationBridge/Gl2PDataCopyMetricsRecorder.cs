// -----------------------------------------------------------------------
// <copyright file = "Gl2PDataCopyMetricsRecorder.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.LogicPresentationBridge
{
    /// <summary>
    /// This class is used to record the GL2P data metrics.
    /// </summary>
    /// <remarks>
    /// This class is not thread safe. Please synchronize the access to this class instance if it's accessed from multiple threads.
    /// </remarks>
    internal class Gl2PDataCopyMetricsRecorder
    {
        #region Fields

        /// <summary>
        /// The data size that has been processed by serialization.
        /// </summary>
        private long serializationSize;

        #endregion

        #region Public Methods

        /// <summary>
        /// Increase the data serialization size by the given amount.
        /// </summary>
        /// <param name="increment">The delta amount to increase by.</param>
        public void IncreaseSerializationSize(long increment)
        {
            serializationSize += increment;
        }

        /// <summary>
        /// Reset the data serialization size to 0.
        /// </summary>
        public void ResetSerializationSize()
        {
            serializationSize = 0;
        }

        /// <summary>
        /// Gets the size of the data that has been serialized.
        /// </summary>
        /// <returns>The size of the data that has been serialized.</returns>
        public long GetSerializationSize()
        {
            return serializationSize;
        }

        #endregion
    }
}