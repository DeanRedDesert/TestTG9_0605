// -----------------------------------------------------------------------
// <copyright file = "RequestCashoutActionParam.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// This class defines a parameter object used for <see cref="BaseShellIdlePresentationAction.RequestCashout"/>.
    /// </summary>
    public sealed class RequestCashoutActionParam: PresentationActionParam
    {
        #region Constructors

        /// <inheritdoc/>
        public RequestCashoutActionParam()
            : base(BaseShellIdlePresentationAction.RequestCashout.ToString())
        {
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override object DeepClone()
        {
            return new RequestCashoutActionParam();
        }

        #endregion
    }
}