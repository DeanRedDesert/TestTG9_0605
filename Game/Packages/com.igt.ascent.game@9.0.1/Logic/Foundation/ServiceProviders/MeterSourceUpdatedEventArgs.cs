//-----------------------------------------------------------------------
// <copyright file = "MeterSourceUpdatedEventArgs.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Foundation.ServiceProviders
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Event arguments for the meter source item collection are updated.
    /// </summary>
    [Serializable]
    public class MeterSourceUpdatedEventArgs :EventArgs
    {
        /// <summary>
        /// Initialize an instance with collection of meter source item<see cref="MeterSourceItem"/>.
        /// </summary>
        /// <param name="meterSourceItems">The collection of meter source item.</param>
        /// <exception cref="ArgumentNullException">
        /// If the <paramref name="meterSourceItems"/> is null, this exception will be thrown.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// If the <paramref name="meterSourceItems"/> do not contain any 
        /// meter source item <see cref="MeterSourceItem"/>,this exception will be thrown.
        /// </exception>
        public MeterSourceUpdatedEventArgs(ICollection<MeterSourceItem> meterSourceItems)
        {
            if(meterSourceItems == null)
            {
                throw new ArgumentNullException("meterSourceItems", "Argument should not be null.");
            }
            if(meterSourceItems.Count == 0)
            {
                throw new ArgumentException("The meterSourceItems does not contain any MeterSourceItem",
                    "meterSourceItems");
            }
            MeterSourceItems = meterSourceItems;
        }

        /// <summary>
        /// Get the collection of meter source item.
        /// </summary>
        public ICollection<MeterSourceItem> MeterSourceItems { get; private set; }

    }
}
