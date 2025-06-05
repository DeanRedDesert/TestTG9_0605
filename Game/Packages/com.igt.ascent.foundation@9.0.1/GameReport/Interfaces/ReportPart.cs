//-----------------------------------------------------------------------
// <copyright file = "ReportPart.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.GameReport.Interfaces
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a part of the game inspection report that contains
    /// lists of Foundation-defined and custom report items.
    /// </summary>
    /// <typeparam name="TReportItem">The type of the Foundation-defined report items.</typeparam>
    /// <typeparam name="TLabel">The type of the label component.</typeparam>
    public class ReportPart<TReportItem, TLabel>
        where TReportItem : ReportItem<TLabel>
    {
        private readonly List<TReportItem> definedReportItems;
        private readonly List<CustomReportItem> customReportItems;

        /// <summary>
        /// Instantiates a new <see cref="ReportPart{TReportItem,TLabel}"/>.
        /// </summary>
        protected ReportPart()
        {
            definedReportItems = new List<TReportItem>();
            customReportItems = new List<CustomReportItem>();
        }

        /// <summary>
        /// Gets a collection of Foundation-defined report items.
        /// </summary>
        public IEnumerable<TReportItem> DefinedReportItems => definedReportItems;

        /// <summary>
        /// Gets a collection of custom report items.
        /// </summary>
        public IEnumerable<CustomReportItem> CustomReportItems => customReportItems;

        /// <summary>
        /// Adds a Foundation-defined report item to the report part.
        /// </summary>
        /// <param name="reportItem">Foundation-defined report item to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reportItem"/> is null.
        /// </exception>
        public void AddReportItem(TReportItem reportItem)
        {
            if(reportItem == null)
            {
                throw new ArgumentNullException(nameof(reportItem), "Argument may not be null");
            }

            definedReportItems.Add(reportItem);
        }

        /// <summary>
        /// Adds a custom report item to the report part.
        /// </summary>
        /// <param name="reportItem">Custom report item to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reportItem"/> is null.
        /// </exception>
        public void AddReportItem(CustomReportItem reportItem)
        {
            if(reportItem == null)
            {
                throw new ArgumentNullException(nameof(reportItem), "Argument may not be null");
            }

            customReportItems.Add(reportItem);
        }

        /// <summary>
        /// Adds a collection of Foundation-defined report items to the report part.
        /// </summary>
        /// <param name="reportItems">Collection of Foundation-defined report items to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reportItems"/> is null.
        /// </exception>
        public void AddReportItems(IEnumerable<TReportItem> reportItems)
        {
            if(reportItems == null)
            {
                throw new ArgumentNullException(nameof(reportItems), "Argument may not be null");
            }

            definedReportItems.AddRange(reportItems);
        }

        /// <summary>
        /// Adds a collection of custom report items to the report part. 
        /// </summary>
        /// <param name="reportItems">Collection of custom report items to add.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="reportItems"/> is null.
        /// </exception>
        public void AddReportItems(IEnumerable<CustomReportItem> reportItems)
        {
            if(reportItems == null)
            {
                throw new ArgumentNullException(nameof(reportItems), "Argument may not be null");
            }

            customReportItems.AddRange(reportItems);
        }
    }
}