//-----------------------------------------------------------------------
// <copyright file = "PaytableSection.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    /// <summary>
    /// Extensions to the schema generated PaytableSection class.
    /// </summary>
    public partial class PaytableSection
    {
        /// <summary>
        /// Get user data with the specified name as the specified type.
        /// </summary>
        /// <typeparam name="TSchemaClass">The type of the user data to get.</typeparam>
        /// <param name="userDataName">The name of the user data to get.</param>
        /// <returns>The specified user data as the specified type.</returns>
        /// <exception cref="KeyNotFoundException">
        ///  Thrown when user data with the specified name cannot be found.
        /// </exception>
        /// <exception cref="UserDataTypeException">
        /// Thrown when the read user data cannot be deserialized into the specified type
        /// </exception>
        /// <remarks>
        /// Any user data can be read as a string by using string as TSchemaClass. This is not recommended, and it will
        /// only return the inner xml, not the complete element.
        /// </remarks>
        public TSchemaClass GetUserData<TSchemaClass>(string userDataName) where TSchemaClass : class
        {
            var item = (from userData in UserData where userData.name == userDataName select userData).FirstOrDefault();

            if (item == null)
            {
                throw new KeyNotFoundException("User data  with the name: " + userDataName + " could not be found.");
            }

            var xmlItem = item.Any;

            var serializer = new XmlSerializer(typeof (TSchemaClass));
            var reader = new XmlNodeReader(xmlItem);
            TSchemaClass readItem = null;

            //If requested as string return the inner xml. Any xml type can be requested as a string.
            if (typeof(TSchemaClass) == typeof(string))
            {
                readItem = xmlItem.InnerXml as TSchemaClass;
            }
            else
            {
                try
                {
                    readItem = serializer.Deserialize(reader) as TSchemaClass;
                }
                catch (InvalidOperationException invalidOperationException)
                {
                    throw new UserDataTypeException(userDataName, typeof(TSchemaClass).ToString(), invalidOperationException);
                }
            }

            return readItem;
        }
    }
}
