//-----------------------------------------------------------------------
// <copyright file = "ILogicDataRequestController.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.Services
{
    using System;
    using System.Reflection;
    using Communication.CommunicationLib;

    /// <summary>
    /// This interface is used to cache the relationship between
    /// the services and consumers and apply data to them.
    /// </summary>
    public interface ILogicDataRequestController
    {
        /// <summary>
        /// Register a property of an object with the request controller.
        /// This allows the request controller to populate the property with
        /// data received from the game logic.
        /// </summary>
        /// <param name="consumerProperty">
        /// Property Info identifying the property to let the request controller update
        /// </param>
        /// <param name="consumer">Object the property belongs to</param>
        /// <param name="serviceProvider">Service Provider to get data from</param>
        /// <param name="serviceLocation">Service and arguments to access the data</param>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null</exception>
        /// <exception cref="ArgumentException">Thrown if serviceProvider is empty or null</exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public void RegisterWithRequestController(string propertyName, object consumerObject,
        ///    string serviceProvider, string service,
        ///    ILogicDataRequestController requestController)
        /// {
        ///     var propertyInfo = consumerObject.GetType().GetProperty(propertyName);
        ///     var serviceAccessor = new ServiceAccessor(service);
        ///     try
        ///     {
        ///         requestController.AddConsumerProperty(propertyInfo, consumerObject, serviceProvider, serviceAccessor);
        ///     }
        ///     catch (Exception)
        ///     {
        ///         //handle exception
        ///         throw;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        void AddConsumerProperty(PropertyInfo consumerProperty, object consumer, string serviceProvider,
            ServiceAccessor serviceLocation);

        /// <summary>
        /// Try to register a property of an object with the request controller.
        /// This allows the request controller to populate the property with
        /// data received from the game logic.
        /// </summary>
        /// <param name="consumerProperty">
        /// Property Info identifying the property to let the request controller update
        /// </param>
        /// <param name="consumer">Object the property belongs to</param>
        /// <param name="serviceProvider">Service Provider to get data from</param>
        /// <param name="serviceLocation">Service and arguments to access the data</param>
        /// <returns>True if the property is successfully registered; false if already registered before.</returns>
        /// <exception cref="ArgumentNullException">Thrown if any parameter is null</exception>
        /// <exception cref="ArgumentException">Thrown if serviceProvider is empty or null</exception>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// public void RegisterWithRequestController(string propertyName, object consumerObject,
        ///    string serviceProvider, string service,
        ///    ILogicDataRequestController requestController)
        /// {
        ///     var propertyInfo = consumerObject.GetType().GetProperty(propertyName);
        ///     var serviceAccessor = new ServiceAccessor(service);
        ///     try
        ///     {
        ///         requestController.TryAddConsumerProperty(propertyInfo, consumerObject, serviceProvider, serviceAccessor);
        ///     }
        ///     catch (Exception)
        ///     {
        ///         //handle exception
        ///         throw;
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        bool TryAddConsumerProperty(PropertyInfo consumerProperty, object consumer, string serviceProvider,
            ServiceAccessor serviceLocation);

        /// <summary>
        /// Update properties that have been registered for asynchronous updates.
        /// </summary>
        /// <param name="filledRequest">
        /// Updated data to update properties with.
        /// </param>
        /// <exception cref="ApplyFilledRequestException">
        /// Exception thrown if the request cannot be applied.
        /// </exception>
        /// <example>
        /// <code><![CDATA[
        /// public void ApplyAsynchonousDataToRequestController(ILogicDataRequestController requestController,
        ///                                                     DataItems filledRequest)
        /// {
        ///     try
        ///     {
        ///         requestController.ApplyAsynchronouslyFilledRequest(filledRequest);
        ///     }
        ///     catch (ApplyFilledRequestException)
        ///     {
        ///         //handle exception.
        ///         throw;
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        void ApplyAsynchronouslyFilledRequest(DataItems filledRequest);

        /// <summary>
        /// Update properties that have been registered for synchronous updates.
        /// </summary>
        /// <param name="filledRequest">
        /// Updated data to update properties with.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Exception thrown if filledRequest is null.
        /// </exception>
        /// <exception cref="ApplyFilledRequestException">
        /// Exception thrown if the request cannot be applied or data is missing.
        /// </exception>
        /// <example>
        /// <code><![CDATA[
        /// public void ApplySynchonousDataToRequestController(ILogicDataRequestController requestController,
        ///                                                    DataItems filledRequest)
        /// {
        ///     try
        ///     {
        ///         requestController.ApplySynchronouslyFilledRequest(filledRequest);
        ///     }
        ///     catch (ApplyFilledRequestException)
        ///     {
        ///         //handle exception.
        ///         throw;
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        void ApplySynchronouslyFilledRequest(DataItems filledRequest);

        /// <summary>
        /// Unregister property from request controller
        /// </summary>
        /// <param name="property">
        /// Property Info for the property to unregister.
        /// </param>
        /// <param name="consumer">
        /// Consumer object for the property to unregister.
        /// </param>
        /// <returns>Returns true if the property was removed.</returns>
        /// <example>
        /// <code><![CDATA[
        /// public void RemovePropertyFromRequestController(string propertyName, object consumerObject,
        ///                                                 ILogicDataRequestController requestController)
        /// {
        ///     var propertyInfo = consumerObject.GetType().GetProperty(propertyName);
        ///     if(requestController.RemoveConsumerProperty(propertyInfo, consumerObject))
        ///     {
        ///         //...
        ///     }
        ///     else
        ///     {
        ///         //...
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        bool RemoveConsumerProperty(PropertyInfo property, object consumer);

        /// <summary>
        /// Clear all consumer properties.
        /// </summary>
        void ClearConsumerProperties();

        /// <summary>
        /// Finds an existing request
        /// </summary>
        /// <param name="property">Property to use for lookup.</param>
        /// <param name="consumer">Object to use for lookup.</param>
        /// <param name="serviceProvider">Outputs provider of request.</param>
        /// <param name="identifier">Outputs service identifier of request.</param>
        /// <exception cref="ArgumentNullException">Thrown if input parameters are null.</exception>
        /// <returns>True if found</returns>
        bool FindRequest(PropertyInfo property, object consumer, out string serviceProvider,
            out int identifier);

        /// <summary>
        /// Finds an existing request.
        /// </summary>
        /// <param name="property">Property to use for lookup.</param>
        /// <param name="consumer">Object to use for lookup.</param>
        /// <param name="isHistory">If the consumer is for history mode.</param>
        /// <param name="serviceProvider">Outputs provider of request.</param>
        /// <param name="identifier">Outputs service identifier of request.</param>
        /// <exception cref="ArgumentNullException">Thrown if input parameters are null.</exception>
        /// <returns>True if found</returns>
        bool FindRequest(PropertyInfo property, object consumer, bool isHistory, out string serviceProvider,
            out int identifier);
    }
}
