// -----------------------------------------------------------------------
// <copyright file = "OutcomeResponseData.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.CoplayerLib.Interfaces
{
    using System;
    using System.Text;
    using OutcomeList.Interfaces;

    /// <summary>
    /// This class defines the data of evaluate outcome response retrieved from the Foundation.
    /// </summary>
    [Serializable]
    public class OutcomeResponseData
    {
        #region Properties

        /// <summary>
        /// Gets the boolean flag that indicates if the data is ready.
        /// </summary>
        public bool IsReady { get; private set; }

        /// <summary>
        /// Gets the boolean flag that indicates if this was the last outcome or not.
        /// </summary>
        public bool IsLastOutcome { get; private set; }

        /// <summary>
        /// Gets the list of outcomes/awards the Foundation has evaluated and potentially adjusted.
        /// Could be null when <see cref="IsReady"/> is false.
        /// </summary>
        public IOutcomeList OutcomeList { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiates an instance of <see cref="OutcomeResponseData"/>.
        /// </summary>
        /// <param name="isReady">
        /// The boolean flag that indicates if the data is ready.
        /// </param>
        /// <param name="isLastOutcome">
        /// The boolean flag that indicates if this was the last outcome or not.
        /// </param>
        /// <param name="outcomeList">
        /// The list of outcomes/awards the foundation has evaluated and potentially adjusted.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="isReady"/> is true but <paramref name="outcomeList"/> is null.
        /// </exception>
        public OutcomeResponseData(bool isReady, bool isLastOutcome, IOutcomeList outcomeList)
        {
            if(isReady && outcomeList == null || !isReady && outcomeList != null)
            {
                throw new ArgumentException(
                    $"isReady flag({isReady}) conflicts with the outcomeList({(outcomeList == null ? "Null" : "NotNull")}).");
            }

            IsReady = isReady;
            IsLastOutcome = isLastOutcome;
            OutcomeList = outcomeList;
        }

        #endregion

        #region ToString Overrides

        /// <inheritdoc/>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("OutcomeResponseData -");
            builder.AppendLine("\t IsReady: " + IsReady);
            builder.AppendLine("\t IsLastOutcome: " + IsLastOutcome);
            builder.Append("\t " + OutcomeList);

            return builder.ToString();
        }

        #endregion
    }
}