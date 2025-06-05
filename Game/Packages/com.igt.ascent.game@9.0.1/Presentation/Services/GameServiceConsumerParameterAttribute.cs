//-----------------------------------------------------------------------
// <copyright file = "GameServiceConsumerParameterAttribute.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Services
{
    using System;

    /// <summary>
    /// Used to add parameters to data requests from the logic.
    /// Logic Data Editor uses this attribute to add parameters to services.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class GameServiceConsumerParameterAttribute : Attribute
    {
        /// <summary>
        /// Default Constructor creates attribute with blank default Parameter Name and null value.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// class CurrentStateConsumer : MonoBehaviour
        /// {
        ///     private string stringValue;
        ///     [GameServiceConsumer]
        ///     [GameServiceConsumerParameter]
        ///     public object StringValue
        ///     {
        ///         set{ GetComponent<TextMesh>().text = value.ToString(); }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public GameServiceConsumerParameterAttribute () : this ("", null) {}

        /// <summary>
        ///  Constructor creates attribute with default parameter name and null value.
        /// </summary>
        /// <param name="defaultParameterName">The Default Parameter Name</param>
        /// <example>
        /// <code><![CDATA[
        /// class ReelNameConsumer : MonoBehaviour
        /// {
        ///     private string stringValue;
        ///     [GameServiceConsumer("ReelsProvider", "ReelName")]
        ///     [GameServiceConsumerParameter("ReelIndex")]
        ///     public object StringValue
        ///     {
        ///         set{ GetComponent<TextMesh>().text = value.ToString(); }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public GameServiceConsumerParameterAttribute(string defaultParameterName) : this(defaultParameterName, null) { }

        /// <summary>
        ///  Constructor creates attribute with default parameter name and value.
        /// </summary>
        /// <param name="defaultParameterName">The Default Parameter Name</param>
        /// <param name="defaultParameterValue">The Default Parameter Value</param>
        /// <example>
        /// <code><![CDATA[
        /// class ReelNameConsumer : MonoBehaviour
        /// {
        ///     private string stringValue;
        ///     [GameServiceConsumer("ReelsProvider", "ReelName")]
        ///     [GameServiceConsumerParameter("ReelIndex", 3)]
        ///     public object StringValue
        ///     {
        ///         set{ GetComponent<TextMesh>().text = value.ToString(); }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public GameServiceConsumerParameterAttribute(string defaultParameterName, object defaultParameterValue)
        {
            ParameterName = defaultParameterName;
            ParameterValue = defaultParameterValue;
        }

        /// <summary>
        /// Name of the parameter.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Value of the Parameter.
        /// </summary>
        public object ParameterValue { get; }
    }
}
