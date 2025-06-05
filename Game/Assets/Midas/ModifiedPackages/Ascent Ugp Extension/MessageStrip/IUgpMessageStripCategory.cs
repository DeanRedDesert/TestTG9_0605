//-----------------------------------------------------------------------
// <copyright file = "IUgpMessageStripCategory.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.Ugp.MessageStrip
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface of UgpMessageStrip category messages.
    /// </summary>
    public interface IUgpMessageStripCategory
    {
        /// <summary>
        /// Retrieve all the messages.
        /// </summary>
        /// <returns>A collection of message strings.</returns>
        IEnumerable<string> GetMessages();
    }
}
