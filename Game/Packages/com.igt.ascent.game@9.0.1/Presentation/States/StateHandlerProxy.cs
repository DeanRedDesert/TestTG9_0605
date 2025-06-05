//-----------------------------------------------------------------------
// <copyright file = "StateHandlerProxy.cs" company = "IGT">
//     Copyright (c) 2012 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.States
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Communication.CommunicationLib;

    /// <summary>
    /// A proxy for <see cref="IStateHandler"/> objects which enables on-demand loading of scenes.
    /// </summary>
    public class StateHandlerProxy : IStateHandler
    {
        #region Nested Class: ServiceInfo

        /// <summary>
        /// This class stores the Service Info temporarily when the real state handler is not ready.
        /// </summary>
        private class ServiceInfo
        {
            /// <summary>
            /// The name of the service provider.
            /// </summary>
            public string ServiceProvider;

            /// <summary>
            /// Service to access within the provider.
            /// </summary>
            public ServiceAccessor ServiceLocation;
        }

        #endregion

        #region Nested Class: PropertyInfoObjectPair

        /// <summary>
        /// Store an object and property info pair.
        /// </summary>
        private class PropertyInfoObjectPair
        {
            /// <summary>
            /// PropertyInfo used to identify property.
            /// </summary>
            public PropertyInfo TargetProperty { get; }

            /// <summary>
            /// Object that contains the property identified by the property info.
            /// </summary>
            public object TargetObject { get; }

            /// <summary>
            /// Constructor to create a PropertyInfoObjectPair.
            /// </summary>
            /// <param name="newObject">object that owns the property.</param>
            /// <param name="newPropertyInfo">PropertyInfo for the property.</param>
            public PropertyInfoObjectPair(object newObject, PropertyInfo newPropertyInfo)
            {
                TargetObject = newObject ?? throw new ArgumentNullException(nameof(newObject));
                TargetProperty = newPropertyInfo ?? throw new ArgumentNullException(nameof(newPropertyInfo));
            }

            /// <summary>
            /// Compare equality of a property info.
            /// </summary>
            /// <param name="obj">object to compare against.</param>
            /// <returns>returns true if objects are equal.</returns>
            public override bool Equals(object obj)
            {
                if(!(obj is PropertyInfoObjectPair target))
                {
                    return false;
                }
                return this == target || CompareMembers(target);
            }

            /// <summary>
            /// Generate Hash for PropertyInfoObjectPair.
            /// </summary>
            /// <DevDoc>Implemented to remove warnings, currently not used as key in a hashed container.</DevDoc>
            /// <returns>Returns hash code for object.</returns>
            public override int GetHashCode()
            {
                var hash = TargetObject.GetHashCode();
                hash ^= TargetProperty.GetHashCode();
                return hash;
            }

            /// <summary>
            /// Type safe comparison of members.
            /// </summary>
            /// <param name="obj">The target object to compare with.</param>
            /// <returns>Returns true if members are equal.</returns>
            private bool CompareMembers(PropertyInfoObjectPair obj)
            {
                return TargetObject == obj.TargetObject && TargetProperty.Equals(obj.TargetProperty);
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// This dictionary caches the calls to AddConsumerProperty() and then
        /// re-applies them on the real state handler once it has been loaded.
        /// </summary>
        private readonly Dictionary<PropertyInfoObjectPair, ServiceInfo> consumerCache =
            new Dictionary<PropertyInfoObjectPair, ServiceInfo>();

        /// <summary>
        /// A <see cref="WeakReference"/> to the state handler.
        /// </summary>
        /// <remarks>
        /// A weak reference is required so that the proxy does not cause the state handler to remain in
        /// memory when it is destroyed, since the proxy must always remain in memory.
        /// </remarks>
        private WeakReference stateHandlerReference;

        /// <summary>
        ///  A cache of state manager object.  Used to initialize the real state handler when one is loaded.
        /// </summary>
        private IPresentationStateManager stateManager;

        /// <summary>
        /// A flag indicating whether OnEnter has been called.
        /// </summary>
        private bool isEntered;

        #endregion

        #region Create Method and Constructors

        /// <summary>
        /// Creates a new implementation of <see cref="StateHandlerProxy"/> whose type is based on the contents
        /// of the given registry.
        /// </summary>
        /// <param name="handlerRegistry">The <see cref="StateHandlerRegistry"/> to create the proxy with.</param>
        /// <returns>An object derived from <see cref="StateHandlerProxy"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="handlerRegistry"/> is <b>null</b>.
        /// </exception>
        public static StateHandlerProxy Create(StateHandlerRegistry handlerRegistry)
        {
            if(handlerRegistry == null)
            {
                throw new ArgumentNullException(nameof(handlerRegistry));
            }
            var proxy = handlerRegistry.IsHistory
                            ? new HistoryStateHandlerProxy(handlerRegistry)
                            : new StateHandlerProxy(handlerRegistry);
            return proxy;
        }

        /// <summary>
        /// Initializes an instance of the <see cref="StateHandlerProxy"/> class with the given state handler
        /// locator.
        /// </summary>
        /// <param name="handlerRegistry">An object containing registration info for the state handler.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="handlerRegistry"/> is <b>null</b>.
        /// </exception>
        protected StateHandlerProxy(StateHandlerRegistry handlerRegistry)
        {
            HandlerRegistry = handlerRegistry ?? throw new ArgumentNullException(nameof(handlerRegistry));
        }

        #endregion


        #region Public and Protected Members

        /// <summary>
        /// Gets the <see cref="StateHandlerRegistry"/> for the represented handler.
        /// </summary>
        public StateHandlerRegistry HandlerRegistry { get; }

        /// <summary>
        /// Indicates whether the handler is loaded or not.
        /// </summary>
        /// <devdoc>
        /// Note that Unity overloads the Equals method for UnityEngine.Object type, which is the base class
        /// for Component and MonoBehaviour etc.  When an UnityEngine.Object instance is destroyed, the overloaded
        /// operator will return true for null comparison.  Therefore, the code below calls Equals(null) to
        /// take care of the scenario where the Target is a destroyed UnityEngine.Object.
        /// </devdoc>
        public bool IsHandlerLoaded => stateHandlerReference != null &&
                                       stateHandlerReference.IsAlive &&
                                       !stateHandlerReference.Target.Equals(null);

        /// <summary>
        /// Uses the locator to load the state handler and returns either the loaded handler or an object that
        /// stores and forwards calls to the handler when it is loaded.
        /// </summary>
        protected IStateHandler StateHandler => IsHandlerLoaded ? stateHandlerReference.Target as IStateHandler : null;

        /// <summary>
        /// Load the state handlers for this proxy.
        /// </summary>
        /// <exception cref="StateHandlerNotLocatedException">
        /// Thrown if the state handler cannot be located.
        /// </exception>
        /// <remarks>
        /// This method should only be called once the base scene for the handler has been fully loaded.
        /// </remarks>
        public void LoadStateHandler()
        {
            var stateHandler = HandlerRegistry.GetStateHandler();
            stateHandler.StateManager = StateManager;
            stateHandler.BeingDestroyed += OnDestroy;

            stateHandlerReference = new WeakReference(stateHandler);
            foreach(var consumerPair in consumerCache)
            {
                stateHandler.AddConsumerProperty(consumerPair.Key.TargetProperty, consumerPair.Key.TargetObject,
                    consumerPair.Value.ServiceProvider, consumerPair.Value.ServiceLocation);
            }
            consumerCache.Clear();
        }

        /// <summary>
        /// Unloads the state handler.
        /// </summary>
        public void UnLoadStateHandler()
        {
            RemoveStateHandler();
        }

        #endregion

        #region IStateHandler Implementation

        /// <inheritdoc />
        public event EventHandler BeingDestroyed;

        /// <inheritdoc/>
        public IPresentationStateManager StateManager
        {
            get => stateManager;
            set
            {
                stateManager = value;
                if(IsHandlerLoaded)
                {
                    StateHandler.StateManager = value;
                }
            }
        }

        /// <inheritdoc/>
        public bool CanRunWithoutFullLoad
        {
            get => StateHandler != null && StateHandler.CanRunWithoutFullLoad;
            set => StateHandler.CanRunWithoutFullLoad = value;
        }

        /// <inheritdoc/>
        public virtual void OnEnter(DataItems stateData)
        {
            StateHandler.OnEnter(stateData);
            isEntered = true;
        }

        /// <inheritdoc/>
        public virtual void OnExit()
        {
            // Validate StateHandler first, because OnExit might be called after
            // the handler has been destroyed.
            StateHandler?.OnExit();
            PostExit();
            isEntered = false;
        }

        /// <inheritdoc/>
        public void PostExit()
        {
            // Validate StateHandler first, because it might be called after
            // the handler has been destroyed.
            StateHandler?.PostExit();
        }

        /// <inheritdoc/>
        public void OnDataUpdate(DataItems updatedData)
        {
            StateHandler.OnDataUpdate(updatedData);
        }

        ///<inheritdoc/>
        public void OnPresentationComplete()
        {
            StateHandler.OnPresentationComplete();
        }

        /// <inheritdoc/>
        public void AddConsumerProperty(PropertyInfo consumerProperty, object consumer, string serviceProvider, ServiceAccessor serviceLocation)
        {
            // We'll get another shot at registering the same data when the scene containing the state handler is loaded.
            // Until the handler is loaded just ignore any requests to register data with it.
            if (StateHandler != null)
            {
                StateHandler.AddConsumerProperty(consumerProperty, consumer, serviceProvider, serviceLocation);
            }
            else
            {
                var keyPair = new PropertyInfoObjectPair(consumer, consumerProperty);
                consumerCache[keyPair] = new ServiceInfo
                {
                    ServiceProvider = serviceProvider,
                    ServiceLocation = serviceLocation
                };
            }
        }

        /// <inheritdoc/>
        public void ClearConsumerProperties()
        {
            StateHandler?.ClearConsumerProperties();
            consumerCache.Clear();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles the event when the state handler target is being destroyed.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments</param>
        private void OnDestroy(object sender, EventArgs e)
        {
            BeingDestroyed?.Invoke(this, EventArgs.Empty);

            RemoveStateHandler();
        }

        /// <summary>
        /// Removes the state handler from the proxy, cleaning up the state handler as needed.
        /// After this call, IsHandlerLoaded will be false and StateHandler will be null.
        /// </summary>
        /// <devdoc>
        /// Currently there are two entry points for this call: UnLoadStateHandle (called by state handler locator
        /// when a scene is unloaded), and OnDestroy (event raised with the unity object is destroyed).
        /// Usually OnDestroy will be called before sceneUnloaded, but in rare cases (e.g. objects are moved out
        /// of the scene and manually destroyed later), OnDestroy could be called after sceneUnloaded.
        /// As a result, this method must not fail on re-entry.
        /// </devdoc>
        private void RemoveStateHandler()
        {
            if(!IsHandlerLoaded)
                return;

            // When the state handler is being removed, exit the state handler if it has been entered.
            // This is to make sure that any necessary clean up work is done, such as un-subscribing event handlers.
            // Checking "isEntered" flag can avoid any state handler implementation that assumes OnExit is only called
            // when OnEnter has been called.
            if(isEntered)
            {
                OnExit();
            }

            stateHandlerReference = null;
            consumerCache.Clear();
        }

        #endregion
    }
}
