//-----------------------------------------------------------------------
// <copyright file = "CategoryXmlSerializationException.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Cabinet.Standard
{
    using System;
    using CSI.Schemas.Internal;

    /// <summary>
    /// Thrown when there is a problem serializing an object to XML.
    /// </summary>
    [Serializable]
    public class CategoryXmlSerializationException : Exception
    {
        private const string ErrorMessageFormat = "CSI category {0} encountered an error while serializing a request of type {1}.";

        /// <summary>
        /// Create a new exception given the category and request.
        /// </summary>
        /// <param name="category">The CSI category that had the error.</param>
        /// <param name="request">The request type that triggered the error.</param>
        public CategoryXmlSerializationException(Category category, Type request)
            : this(category, request, null)
        {

        }

        /// <summary>
        /// Create a new exception given the category, request, and inner exception.
        /// </summary>
        /// <param name="category">The CSI category that had the error.</param>
        /// <param name="request">The request type that triggered the error.</param>
        /// <param name="innerException">The inner exception for the error.</param>
        public CategoryXmlSerializationException(Category category, Type request, Exception innerException)
            : base(string.Format(ErrorMessageFormat, category, request.Name), innerException)
        {

        }
    }
}
