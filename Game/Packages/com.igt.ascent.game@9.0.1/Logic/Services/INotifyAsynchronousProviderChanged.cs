//-----------------------------------------------------------------------
// <copyright file = "INotifyAsynchronousProviderChanged.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Services
{
    using System;

    ///<summary>
    ///   Notifies the <see cref = "ServiceController" /> that a service provider's asynchronous value has changed.
    ///</summary>
    ///<example>
    ///   The following example demonstrates how to implement the INotifyAsynchronousProviderChanged interface.
    ///   <code>
    ///      <![CDATA[
    /// // Service provider that is used to demonstrate how to implement and use the 
    /// // INotifyAsynchronousProviderChanged interface.
    /// public class ExampleServiceProvider : INotifyAsynchronousProviderChanged
    /// {
    ///     // initialize the values example provider values
    ///     public ExampleServiceProvider()
    ///     {
    ///         exampleProviderPropertyValue = string.Empty;
    ///         exampleProviderList = new string[5];
    ///     }
    ///
    ///     // Implement the interface here
    ///     #region INotifyAsynchronousProviderChanged Members
    ///
    ///     public event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;
    /// 
    ///     #endregion
    /// 
    ///     #region Properties
    /// 
    ///     // Get or Set the example service property value.
    ///     // When this value is set an event will be posted to notify the ServiceController
    ///     // that the value was updated.
    ///     [AsynchronousGameService]
    ///     public string ExampleProviderProperty
    ///     {
    ///         get
    ///         {
    ///             return exampleProviderPropertyValue;
    ///         }
    ///         set
    ///         {
    ///             exampleProviderPropertyValue = value;
    ///             OnServiceUpdated("ExampleProviderProperty", null);
    ///         }
    ///     }
    ///
    ///     #endregion
    ///
    ///     #region Public Methods
    ///
    ///     // Set an item in the example provider array.
    ///     // When this method is called with a valid index, an event will be posted to notify the
    ///     // ServiceController that the value was updated.
    ///     public void SetExampleProviderArrayValue(int index, string newValue)
    ///     {
    ///         if(index < exampleProviderList.Length)
    ///         {
    ///             exampleProviderList[index] = newValue;
    ///
    ///             // Notify that an Asynchronous value has changed.
    ///             // Build the argument list that will be passed along with the event.
    ///             Dictionary<string, object> arguments = new Dictionary<string, object>();
    ///             arguments.Add("index", index);
    ///             // Post the event, pass the getter method as the updated Service.
    ///             // this will allow the correct access for the ServiceAccessor.
    ///             OnServiceUpdated("GetExampleProviderValue", arguments);
    ///         }
    ///     }
    ///
    ///     // Get the value of an item in the example provider array.
    ///     // This method is tagged with the AsynchronousGameService attribute.
    ///     [AsynchronousGameService]
    ///     public string GetExampleProviderValue(int index)
    ///     {
    ///         if(index < exampleProviderList.Length)
    ///         {
    ///             return exampleProviderList[index];
    ///         }
    ///         return string.Empty;
    ///     }
    ///
    ///     #endregion
    ///
    ///     #region Protected Methods
    ///
    ///     // Post the AsynchronousProviderChanged event when an asynchronous member is changed.
    ///     protected void OnServiceUpdated(string serviceName, Dictionary<string, object> serviceArguments)
    ///     {
    ///         // Create a temporary copy of the event handler for thread safety
    ///         EventHandler<AsynchronousProviderChangedEventArgs> handler = AsynchronousProviderChanged;
    ///
    ///         // If there are any handlers registered with this event
    ///         if(handler != null)
    ///         {
    ///             // Post the event with this service provider as the sender and the parameter values passed in.
    ///             handler(this, new AsynchronousProviderChangedEventArgs(serviceName, serviceArguments));
    ///         }
    ///     }
    ///
    ///     #endregion
    ///
    ///     #region Private Fields
    ///
    ///     private string exampleProviderPropertyValue;
    ///     private string[] exampleProviderList;
    ///
    ///     #endregion
    /// }
    /// ]]>
    ///   </code>
    ///</example>
    public interface INotifyAsynchronousProviderChanged
    {
        /// <summary>
        ///    Represents the method that will handle the <see cref = "AsynchronousProviderChanged" /> event
        ///    when an asynchronous service provider value is changed.
        /// </summary>
        event EventHandler<AsynchronousProviderChangedEventArgs> AsynchronousProviderChanged;
    }
}
