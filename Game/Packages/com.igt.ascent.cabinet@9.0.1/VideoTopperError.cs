// -----------------------------------------------------------------------
// <copyright file = "VideoTopperError.cs" company = "IGT">
//     Copyright (c) 2022 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet
{
    /// <summary>
    /// Video topper errors.
    /// </summary>
    public enum VideoTopperError
    {
        /// <summary>
        /// Error to notify client does not own the video topper resource.
        /// </summary>
        ClientDoesNotOwnResource,

        /// <summary>
        /// Error to notify content path of a media does not exist.
        /// </summary>
        ContentPathDoesNotExist,

        /// <summary>
        /// Error to notify a content's respective key does not exist.
        /// </summary>
        ContentKeyDoesNotExist
    }
}