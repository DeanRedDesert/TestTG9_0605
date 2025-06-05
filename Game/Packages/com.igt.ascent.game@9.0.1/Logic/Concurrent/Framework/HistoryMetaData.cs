// -----------------------------------------------------------------------
// <copyright file = "HistoryMetaData.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.Framework
{
    using System.Collections.Generic;
    using Game.Core.Communication.CommunicationLib;

    /// <summary>
    /// History Meta Data is a Dictionary added to the history data (DataItems) read from the critical data in history mode.
    /// The Meta Data can be accessed by the <see cref="DataItems.HistoryData"/> property, which basically is an entry
    /// in the DataItems by the key of "History".
    /// The presence of the Meta Data instructs the LDC how to apply the service data to consumers.
    /// The Meta Data also contains information needed by the history presentation state to display a history step.
    /// </summary>
    internal static class HistoryMetaData
    {
        #region HistoryData Accessors

        private static readonly ServiceAccessor HistoryModeAccessor = new ServiceAccessor("HistoryMode");
        private static readonly ServiceAccessor RecoveryModeAccessor = new ServiceAccessor("RecoveryMode");
        private static readonly ServiceAccessor TotalHistoryStepsAccessor = new ServiceAccessor("TotalHistorySteps");
        private static readonly ServiceAccessor CurrentHistoryStepAccessor = new ServiceAccessor("CurrentHistoryStep");

        #endregion

        /// <summary>
        /// Creates the history metadata without information on steps.
        /// </summary>
        /// <returns>
        /// A <see cref="Dictionary{TKey,TValue}"/> with service accessor keys and values that are set by this method.
        /// </returns>
        /// <remarks>
        /// See the HistoryData Accessors region for the definitions of the <see cref="ServiceAccessor"/> objects.
        /// </remarks>
        public static Dictionary<int, object> Create()
        {
            var metadata = new Dictionary<int, object>(2)
                               {
                                   [HistoryModeAccessor.Identifier] = true,
                                   [RecoveryModeAccessor.Identifier] = false,
                               };
            return metadata;
        }

        /// <summary>
        /// Creates the history metadata with information on steps.
        /// </summary>
        /// <param name="totalSteps">The total number of history steps for this game cycle.</param>
        /// <param name="currentStep">The current history step number, 0-based!!!</param>
        /// <returns>
        /// A <see cref="Dictionary{TKey, TValue}"/> with service accessor keys and values that are set by this method.
        /// </returns>
        /// <remarks>
        /// See the HistoryData Accessors region for the definitions of the <see cref="ServiceAccessor"/> objects.
        /// </remarks>
        public static Dictionary<int, object> Create(int totalSteps, int currentStep)
        {
            var metadata = new Dictionary<int, object>(4)
                               {
                                   [HistoryModeAccessor.Identifier] = true,
                                   [RecoveryModeAccessor.Identifier] = false,
                                   [TotalHistoryStepsAccessor.Identifier] = totalSteps,
                                   [CurrentHistoryStepAccessor.Identifier] = currentStep
                               };
            return metadata;
        }
    }
}