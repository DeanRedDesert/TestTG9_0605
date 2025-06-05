// -----------------------------------------------------------------------
// <copyright file = "RequestChooserActionParam.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// This class defines a parameter object used for <see cref="BaseShellIdlePresentationAction.RequestChooser"/>.
    /// </summary>
    public sealed class RequestChooserActionParam : PresentationActionParam
    {
        #region Constructors

        /// <inheritdoc/>
        public RequestChooserActionParam()
            : base(BaseShellIdlePresentationAction.RequestChooser.ToString())
        {
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override object DeepClone()
        {
            return new RequestChooserActionParam();
        }

        #endregion
    }
}