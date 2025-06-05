// -----------------------------------------------------------------------
// <copyright file = "IPaytableLoader.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.PaytableLoader.Interfaces
{
    /// <summary>
    /// This interface defines a common load method that is used by the different paytable file formats.
    /// </summary>
    public interface IPaytableLoader
    {
        /// <summary>
        /// Loads the paytable specified.
        /// </summary>
        /// <param name="paytablePath">
        /// The full path to the paytable to load and use.
        /// </param>
        /// <param name="paytableId">
        /// The paytable specifier, which is context dependent on the type of
        /// paytable being used. For legacy xpaytables, this value is ignored and returned with the
        /// paytable ID found inside of the paytable. For MPT paytables, it specifies the paytable variant found
        /// within the MPT binary paytable file. 
        /// </param>
        /// <returns>The <see cref="PaytableLoadResult"/> </returns>
        PaytableLoadResult LoadPaytable(string paytablePath, string paytableId);
    }
}
