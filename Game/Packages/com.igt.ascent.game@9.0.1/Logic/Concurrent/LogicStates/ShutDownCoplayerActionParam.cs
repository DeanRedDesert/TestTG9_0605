// -----------------------------------------------------------------------
// <copyright file = "ShutDownCoplayerActionParam.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// This class defines a parameter object used for <see cref="BaseShellIdlePresentationAction.ShutDownCoplayer"/>.
    /// </summary>
    public sealed class ShutDownCoplayerActionParam : PresentationActionParam
    {
        #region Properties

        /// <summary>
        /// Gets the coplayer id whose theme is to be changed.
        /// </summary>
        public int CoplayerId { get; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public ShutDownCoplayerActionParam(int coplayerId)
            : base(BaseShellIdlePresentationAction.ShutDownCoplayer.ToString())
        {
            CoplayerId = coplayerId;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override object DeepClone()
        {
            return new ShutDownCoplayerActionParam(CoplayerId);
        }

        #endregion
    }
}