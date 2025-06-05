//-----------------------------------------------------------------------
// <copyright file = "FinalizeOutcomeEventArgs.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.GameLib.Interfaces
{
    using System;
    using System.IO;
    using Game.Core.CompactSerialization;

    /// <summary>
    /// EventArgs for a FinalizeOutcomeEvent.
    /// </summary>
    [Serializable]
    public class FinalizeOutcomeEventArgs : EventArgs, ICompactSerializable
    {
        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            
        }

        #endregion
    }
}