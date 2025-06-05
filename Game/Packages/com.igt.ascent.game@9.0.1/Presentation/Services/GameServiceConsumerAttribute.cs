//-----------------------------------------------------------------------
// <copyright file = "GameServiceConsumerAttribute.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Services
{
    using System;
    using Communication.CommunicationLib;

    /// <summary>
    /// Used to identify properties that require data from the Game Logic.
    /// Logic Data Editor uses this attribute to find properties to populate.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class GameServiceConsumerAttribute : Attribute
    {
        /// <summary>
        /// Default Constructor creates attribute with blank default service provider or service
        /// and with a synchronous notification type.
        /// </summary>
        /// <example>
        /// <code><![CDATA[
        /// class CurrentStateConsumer : MonoBehaviour
        /// {
        ///     private string stringValue;
        ///     [GameServiceConsumer]
        ///     public object StringValue
        ///     {
        ///         set{ GetComponent<TextMesh>().text = value.ToString(); }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public GameServiceConsumerAttribute () : this ("", "", NotificationType.Synchronous) {}

        /// <summary>
        /// Constructor creates attribute with blank default service provider or service
        /// </summary>
        /// <param name="notification">Notification type for data request</param>
        ///     /// <example>
        /// <code><![CDATA[
        /// class CurrentStateConsumer : MonoBehaviour
        /// {
        ///     private string stringValue;
        ///     [GameServiceConsumer(NotificationType.Synchronous)]
        ///     public object StringValue
        ///     {
        ///         set{ GetComponent<TextMesh>().text = value.ToString(); }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public GameServiceConsumerAttribute(NotificationType notification) : this("", "", notification) { }

        /// <summary>
        /// Constructor to specify default service provider and service
        /// </summary>
        /// <param name="defaultServiceProvider">The Default Service Provider</param>
        /// <param name="defaultService">The Default Service</param>
        /// <param name="notification">Notification type for data request</param>
        /// <example>
        /// <code><![CDATA[
        /// class CurrentStateConsumer : MonoBehaviour
        /// {
        ///     private string stringValue;
        ///     [GameServiceConsumer("StateFramework","CurrentState", NotificationType.Synchronous)]
        ///     public object StringValue
        ///     {
        ///         set{ GetComponent<TextMesh>().text = value.ToString(); }
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public GameServiceConsumerAttribute(string defaultServiceProvider, string defaultService, NotificationType notification)
        {
            ServiceProvider = defaultServiceProvider;
            Service = defaultService;
            Notification = notification;
        }

        /// <summary>
        /// Get the default service provider.
        /// </summary>
        public string ServiceProvider { get; }

        /// <summary>
        /// Get the default service.
        /// </summary>
        public string Service { get; }

        /// <summary>
        /// Get the notification type for data request.
        /// </summary>
        public NotificationType Notification {get; }
    }
}
