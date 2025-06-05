//-----------------------------------------------------------------------
// <copyright file = "PickAdapter.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;

    /// <summary>
    /// This class represents a <seealso cref="Pick"/> object which implements the <seealso cref="IPick"/> interface.
    /// Instances of this type can be used to wrap a <see cref="Pick"/> and mutate the weight without changing the
    /// wrapped pick object.
    /// </summary>
    public class PickAdapter : IPick
    {
        /// <summary>
        /// The pick object which this adapter represents.
        /// </summary>
        private readonly Pick pick;

        /// <summary>
        /// Construct an instance with a <seealso cref="Pick"/> object.
        /// </summary>
        /// <param name="pick">The <seealso cref="Pick"/> object to wrap.</param>
        public PickAdapter(Pick pick)
        {
            if(pick == null)
            {
                throw new ArgumentNullException("pick", "Parameter may not be null.");
            }

            this.pick = pick;
            Weight = pick.weight;
        }

        /// <summary>
        /// Gets the original <see cref="Pick"/> object.
        /// </summary>
        public Pick PickObject
        {
            get { return pick; }
        }

        #region IPick members

        /// <inheritDoc />
        public string Name
        {
            get { return pick.name; }
        }

        /// <inheritDoc />
        public uint Weight { get; set; }

        #endregion

    }
}