//-----------------------------------------------------------------------
// <copyright file = "CategorySubscription.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.F2XCallbacks
{
    using F2XTransport;

    /// <summary>
    /// This class is used to subscribe to a F2X message category by a communication link.
    /// </summary>
    public struct CategorySubscription
    {
        /// <summary>
        /// The category to subscribe.
        /// </summary>
        public MessageCategory Category { get; private set; }

        /// <summary>
        /// Flag indicating whether the category is required.
        /// </summary>
        public bool Required { get; private set; }

        /// <summary>
        /// Initializes an instance of <see cref="CategorySubscription"/>.
        /// </summary>
        /// <param name="category">The category to subscribe.</param>
        /// <param name="required">Flag indicating whether the category is required.</param>
        public CategorySubscription(MessageCategory category, bool required) : this()
        {
            Category = category;
            Required = required;
        }
    }
}
