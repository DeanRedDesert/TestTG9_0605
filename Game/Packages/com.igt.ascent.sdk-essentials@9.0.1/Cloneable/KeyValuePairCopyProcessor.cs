// -----------------------------------------------------------------------
// <copyright file = "KeyValuePairCopyProcessor.cs" company = "IGT">
//     Copyright (c) 2017 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Cloneable
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// This class is used to deep copy generic <see cref="KeyValuePair{TKey,TValue}"/> data.
    /// </summary>
    public sealed class KeyValuePairCopyProcessor : IDataCopyProcessor
    {
        #region Fields

        /// <summary>
        /// This copy processor used to copy the key elements in the dictionary.
        /// </summary>
        private readonly IDataCopyProcessor keyCopyProcessor;

        /// <summary>
        /// This copy processor used to copy the value elements in the dictionary.
        /// </summary>
        private readonly IDataCopyProcessor valueCopyProcessor;

        /// <summary>
        /// The cached generic copy methods indexed by the real dictionary type.
        /// </summary>
        private readonly Dictionary<Type, MethodInfo> copyMethodImpl = new Dictionary<Type, MethodInfo>();

        /// <summary>
        /// The cached copy method generic definition.
        /// </summary>
        private static MethodInfo genericCloneMethodDefinition;

        #endregion

        #region Constructors

        /// <summary>
        /// The static constructor.
        /// </summary>
        static KeyValuePairCopyProcessor()
        {
            ReflectGenericMethodDefinition();
        }

        /// <summary>
        /// Construct the instance with the given element copy processors.
        /// </summary>
        /// <param name="keyCopyProcessor">This copy processor is used to copy the keys.</param>
        /// <param name="valueCopyProcessor">This copy processor is used to copy the values.</param>
        /// <exception cref="ArgumentNullException">Thrown when either of the parameters is null.</exception>
        public KeyValuePairCopyProcessor(IDataCopyProcessor keyCopyProcessor, IDataCopyProcessor valueCopyProcessor)
        {
            this.keyCopyProcessor = keyCopyProcessor ?? throw new ArgumentNullException(nameof(keyCopyProcessor));
            this.valueCopyProcessor = valueCopyProcessor ?? throw new ArgumentNullException(nameof(valueCopyProcessor));
        }

        #endregion

        #region Implementation of IDataCopyProcessor

        /// <inheritdoc />
        public object DeepCopy(object source)
        {
            if(source == null)
            {
                return null;
            }

            var implType = source.GetType();
            if(implType.IsGenericType)
            {
                var genericDefinition = implType.GetGenericTypeDefinition();
                if(genericDefinition == typeof(KeyValuePair<,>))
                {
                    if(!copyMethodImpl.ContainsKey(implType))
                    {
                        copyMethodImpl[implType] =
                            genericCloneMethodDefinition.MakeGenericMethod(implType.GetGenericArguments());
                    }
                    return copyMethodImpl[implType].Invoke(this, new[] {source});
                }
            }
            throw new ArgumentException("The source type is not supported.", nameof(source));
        }

        #endregion

        /// <summary>
        /// Reflect the generic copy method definition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the generic copy method cannot be found.
        /// </exception>
        private static void ReflectGenericMethodDefinition()
        {
            if(genericCloneMethodDefinition == null)
            {
                genericCloneMethodDefinition
                    = typeof(KeyValuePairCopyProcessor).GetMethod("DeepCopyKeyValuePair",
                        BindingFlags.Instance | BindingFlags.NonPublic);
                if(genericCloneMethodDefinition == null)
                {
                    throw new InvalidOperationException(
                        "Cannot find method KeyValuePairCopyProcessor.DeepCopyKeyValuePair");
                }
            }
        }

        /// <summary>
        /// Deep copy the key value pair struct.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="source">The source key value pair to copy.</param>
        /// <returns>The copied key value pair.</returns>
        /// <remarks>
        /// Fixing PFR-898.
        /// Using instance method instead of static method to avoid mono crash when unloading the runtime.
        /// </remarks>
        // ReSharper disable once UnusedMember.Local
        private KeyValuePair<TKey, TValue> DeepCopyKeyValuePair<TKey, TValue>(KeyValuePair<TKey, TValue> source)
        {
            return new KeyValuePair<TKey, TValue>(
                (TKey)keyCopyProcessor.DeepCopy(source.Key),
                (TValue)valueCopyProcessor.DeepCopy(source.Value));
        }
    }
}