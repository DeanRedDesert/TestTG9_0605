// -----------------------------------------------------------------------
// <copyright file = "PresentationActionParam.cs" company = "IGT">
//     Copyright (c) 2019 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Ascent.Logic.Concurrent.LogicStates
{
    using System.Collections.Generic;
    using Game.Core.Cloneable;

    /// <summary>
    /// Base class for all presentation action parameter types.
    /// </summary>
    public abstract class PresentationActionParam : IDeepCloneable
    {
        #region Properties

        /// <summary>
        /// Gets the name of the presentation action.
        /// </summary>
        public string ActionName { get; protected set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the parameter of a specific presentation action.
        /// </summary>
        /// <param name="actionName"></param>
        protected PresentationActionParam(string actionName)
        {
            ActionName = actionName ?? string.Empty;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Provides a data structure that can be used as the GenericData field of a presentation state complete message.
        /// </summary>
        /// <returns>
        /// A data structure that can be used as the GenericData field of a presentation state complete message.
        /// </returns>
        /// <devdoc>
        /// The return type has to be consistent with the signature of PresentationState.PresentationComplete method.
        /// </devdoc>
        public Dictionary<string, object> ToGenericData()
        {
            // Since there is only one entry in the dictionary, simply use the empty string as the entry key.
            return new Dictionary<string, object> { { string.Empty, this } };
        }

        #endregion

        #region IDeepCloneable Implementation

        /// <inheritdoc/>
        public abstract object DeepClone();

        #endregion
    }
}