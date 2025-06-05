// -----------------------------------------------------------------------
// <copyright file = "LocalizedResource.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation
{
    using System;
    using System.IO;
    using System.Text;
    using CompactSerialization;

    ///<summary>
    /// This class contains the information data for localized resource, such as resource type and resource string or asset path.
    ///</summary>
    [Serializable]
    public class LocalizedResource : ICompactSerializable
    {
        ///<summary>
        /// Gets the localized resource type.
        ///</summary>
        public LocalizedResourceType Type { get; private set; }

        ///<summary>
        /// Gets the value of the localized resource.
        /// If the resource is a string, this is the string value; If the resource is an asset,
        /// such as a file or a movie, this is the path to the asset.
        ///</summary>
        public string Value { get; private set; }

        /// <summary>
        /// Constructs an instance of <see cref="LocalizedResource"/> by the given type and value path of the resource.
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="value"/> is null or empty.
        /// </exception>
        public LocalizedResource(LocalizedResourceType type, string value)
        {
            if(string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("The value of localized resource can not be null or empty.");
            }
            Type = type;
            Value = value;
        }

        /// <summary>
        /// Instantiates an instance of <see cref="LocalizedResource"/>.
        /// This parameterless constructor is used for serialization purposes.
        /// </summary>
        public LocalizedResource()
        {
        }

        #region ToString

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("LocalizedResource -");
            builder.AppendLine($"\t Type({Type})");
            builder.AppendLine($"\t Value({Value})");

            return builder.ToString();

        }

        #endregion

        #region ICompactSerializable Implementation

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Serialize(stream, Type);
            CompactSerializer.Serialize(stream, Value);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            Type = CompactSerializer.ReadEnum<LocalizedResourceType>(stream);
            Value = CompactSerializer.ReadString(stream);
        }

        #endregion
    }
}