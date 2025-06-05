//-----------------------------------------------------------------------
// <copyright file = "SynchronizedAttractEntry.Extension.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.BankSynchronization
{
    using System.Linq;

    public partial class SynchronizedAttractEntry
    {
        /// <inheritdoc />
        public override long DisplayTime
        {
            get
            {
                return File?.Max(entry => entry.DisplayTime) ?? 0;
            }
        }

        /// <inheritdoc />
        internal override void Initialize(string gameMountPoint)
        {
            if(File != null)
            {
                foreach(var entry in File)
                {
                    entry.Initialize(gameMountPoint);
                }
            }
        }
    }
}
