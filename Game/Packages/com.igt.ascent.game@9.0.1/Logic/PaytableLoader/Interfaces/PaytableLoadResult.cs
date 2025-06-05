// -----------------------------------------------------------------------
// <copyright file = "PaytableLoadResult.cs" company = "IGT">
//     Copyright (c) 2018 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.PaytableLoader.Interfaces
{
    using System;
    using System.Text;

    /// <summary>
    /// A class encapsulating the result of a paytable load attempt.
    /// </summary>
    public class PaytableLoadResult
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public PaytableLoadResult()
        {
            IsLoaded = false;
            FullPathToPaytableFile = "";
            PaytableType = PaytableType.Unknown;
            LoadResultsDescription = "";
            PaytableId = "";
            GenericPaytableData = null;
        }

        /// <summary>
        /// A class encapsulating the results and any data retrieved from a paytable load operation.
        /// </summary>
        /// <param name="isLoaded">A bool flag indicating if the load was successful.</param>
        /// <param name="fullPathToPaytableFile">The full path to the paytable file.</param>
        /// <param name="paytableType">The determined <see cref="PaytableType"/> paytable type.</param>
        /// <param name="loadResultsDescription">A message detailing the results of the load attempt.</param>
        /// <param name="paytableId">The requested paytable ID.</param>
        /// <param name="genericPaytableData">An object that implements <see cref="IGenericPaytableData"/>, containing generic data common
        /// to all supported paytable types.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if both <paramref name="isLoaded"/> and <paramref name="genericPaytableData"/> are null.
        /// </exception>
        public PaytableLoadResult(bool isLoaded, string fullPathToPaytableFile, PaytableType paytableType, string loadResultsDescription, string paytableId,
                                  IGenericPaytableData genericPaytableData)
        {
            if(isLoaded && genericPaytableData == null)
            {
                throw new ArgumentNullException("genericPaytableData", "GenericPaytableData cannot be null if isLoaded is true.");
            }

            IsLoaded = isLoaded;
            FullPathToPaytableFile = fullPathToPaytableFile;
            PaytableType = paytableType;
            LoadResultsDescription = loadResultsDescription;
            PaytableId = paytableId;
            GenericPaytableData = genericPaytableData;
        }

        /// <summary>
        /// Gets the flag indicating whether the load attempt was successful.
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Retrieves the full path to the paytable file.
        /// </summary>
        public string FullPathToPaytableFile { get; private set; }

        /// <summary>
        /// Gets descriptive text describing the success or failure to load attempt with more detail.
        /// </summary>
        public string LoadResultsDescription { get; private set; }

        /// <summary>
        /// Returns the <see cref="PaytableType"/> of the loaded paytable.
        /// </summary>
        /// <returns></returns>
        public PaytableType PaytableType { get; private set; }

        /// <summary>
        /// Returns the paytableId of the loaded paytable or variant.
        /// </summary>
        /// <returns>
        /// The paytable id. This must be specified for MPT paytables, but XPaytable loads will set this value after the paytable 
        /// is loaded successfully.
        /// </returns>
        public string PaytableId { get; private set; }

        /// <summary>
        /// Retrieves the <see cref="IGenericPaytableData"/> data common between the various supported paytables.
        /// </summary>
        public IGenericPaytableData GenericPaytableData { get; private set; }

        #region Overrides

        /// <summary>
        /// Overridden ToString for more helpful results.
        /// </summary>
        /// <returns>Diagnostic information to assist with paytable loading problems.</returns>
        public override string ToString()
        {
            var loadResultSb = new StringBuilder();
            loadResultSb.AppendFormat("IsLoaded: {0}, \n", IsLoaded);
            loadResultSb.AppendFormat("Full Path: {0}, \n", FullPathToPaytableFile);
            loadResultSb.AppendFormat("Results Description: {0}, \n", LoadResultsDescription);
            loadResultSb.AppendFormat("Type: {0}, \n", PaytableType.ToString());
            loadResultSb.AppendFormat("Foundation Requested Paytable ID: {0}, \n", PaytableId);
            loadResultSb.AppendFormat("Generic Data: {0} \n", GenericPaytableData); 

            return loadResultSb.ToString();
        }

        #endregion
    }
}
