// -----------------------------------------------------------------------
// <copyright file = "AddCreditsActionParam.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// This class defines a parameter object used for <see cref="BaseShellIdlePresentationAction.AddCredits"/>.
    /// </summary>
    public sealed class AddCreditsActionParam : PresentationActionParam
    {
        #region Public Properties

        /// <summary>
        /// The amount of credits to add.
        /// </summary>
        public long CreditAmountToAdd { get; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public AddCreditsActionParam(long addCreditAmount)
            : base(BaseShellIdlePresentationAction.AddCredits.ToString())
        {
            CreditAmountToAdd = addCreditAmount;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override object DeepClone()
        {
            return new AddCreditsActionParam(CreditAmountToAdd);
        }

        #endregion
    }
}