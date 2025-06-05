// -----------------------------------------------------------------------
// <copyright file = "SwitchCoplayerThemeActionParam.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// This class defines a parameter object used for <see cref="BaseShellIdlePresentationAction.SwitchCoplayerTheme"/>.
    /// </summary>
    public class SwitchCoplayerThemeActionParam : PresentationActionParam
    {
        #region Properties

        /// <summary>
        /// Gets the coplayer id whose theme is to be changed.
        /// </summary>
        public int CoplayerId { get; }

        /// <summary>
        /// Gets the G2S Theme Id of the theme to switch to.
        /// </summary>
        public string G2SThemeId { get; }

        /// <summary>
        /// Gets the denomination of the theme to switch to.
        /// </summary>
        public long Denomination { get; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public SwitchCoplayerThemeActionParam(int coplayerId, string g2SThemeId, long denomination)
            : base(BaseShellIdlePresentationAction.SwitchCoplayerTheme.ToString())
        {
            CoplayerId = coplayerId;
            G2SThemeId = g2SThemeId ?? string.Empty;
            Denomination = denomination;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override object DeepClone()
        {
            return new SwitchCoplayerThemeActionParam(CoplayerId, G2SThemeId, Denomination);
        }

        #endregion
    }
}