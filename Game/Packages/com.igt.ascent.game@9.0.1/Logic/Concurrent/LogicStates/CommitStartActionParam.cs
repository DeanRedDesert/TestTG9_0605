// -----------------------------------------------------------------------
// <copyright file = "CommitStartActionParam.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// This class defines a parameter object used for <see cref="BaseIdlePresentationAction.CommitStart"/>.
    /// </summary>
    public sealed class CommitStartActionParam  : PresentationActionParam
    {
        #region Properties

        /// <summary>
        /// Gets the number of credits, in current game denomination, to bet.
        /// </summary>
        public long BetCredits { get; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public CommitStartActionParam(long betCredits)
            : base(BaseIdlePresentationAction.CommitStart.ToString())
        {
            BetCredits = betCredits;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override object DeepClone()
        {
            return new CommitStartActionParam(BetCredits);
        }

        #endregion
    }
}