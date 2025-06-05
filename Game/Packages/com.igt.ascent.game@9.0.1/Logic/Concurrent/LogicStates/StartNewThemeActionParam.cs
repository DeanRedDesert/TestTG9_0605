// -----------------------------------------------------------------------
// <copyright file = "StartNewThemeActionParam.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    /// <summary>
    /// This class defines a parameter object used for <see cref="BaseShellIdlePresentationAction.StartNewTheme"/>.
    /// </summary>
    public class StartNewThemeActionParam : PresentationActionParam
    {
        #region Properties

        /// <summary>
        /// Gets the G2S Theme Id of the new theme to start.
        /// </summary>
        public string G2SThemeId { get; }

        /// <summary>
        /// Gets the denomination of the new theme to start.
        /// </summary>
        public long Denomination { get; }

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public StartNewThemeActionParam(string g2SThemeId, long denomination)
            : base(BaseShellIdlePresentationAction.StartNewTheme.ToString())
        {
            G2SThemeId = g2SThemeId ?? string.Empty;
            Denomination = denomination;
        }

        #endregion

        #region Base Overrides

        /// <inheritdoc/>
        public override object DeepClone()
        {
            return new StartNewThemeActionParam(G2SThemeId, Denomination);
        }

        #endregion
    }
}