//-----------------------------------------------------------------------
// <copyright file = "Step.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming.Choreography
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using CompactSerialization;

    /// <summary>
    /// Represents a choreography step.
    /// </summary>
    public sealed class Step : IEquatable<Step>, ICompactSerializable, IEnumerable<StreamingAction>
    {
        /// <summary>
        /// Construct a new instance.
        /// </summary>
        public Step()
        {
            Actions = new List<StreamingAction>();
        }

        /// <summary>
        /// The time in seconds for this step to last.
        /// </summary>
        public float Time
        {
            get;
            set;
        }

        /// <summary>
        /// The list of actions to take during this step.
        /// </summary>
        public List<StreamingAction> Actions
        {
            get;
            private set;
        }

        /// <inheritdoc />
        void ICompactSerializable.Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, Time);
            CompactSerializer.WriteList(stream, Actions);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(System.IO.Stream stream)
        {
            Time = CompactSerializer.ReadFloat(stream);
            Actions = CompactSerializer.ReadListSerializable<StreamingAction>(stream);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var equals = false;

            if(obj != null)
            {
                if(ReferenceEquals(this, obj))
                {
                    equals = true;
                }
                else
                {
                    equals = Equals(obj as Step);
                }
            }

            return equals;
        }

        /// <inheritdoc />
        public bool Equals(Step other)
        {
            var equals = false;

            if(other != null)
            {
                if(ReferenceEquals(this, other))
                {
                    equals = true;
                }
                else
                {
                    equals = Time.Equals(other.Time)
                        && Actions.SequenceEqual(other.Actions);
                }
            }

            return equals;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
            return base.GetHashCode();
        }

        #region IEnumerable Implementation

        /// <inheritdoc />
        public IEnumerator<StreamingAction> GetEnumerator()
        {
            return Actions.GetEnumerator();
        }

        /// <inheritdoc />
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Actions.GetEnumerator();
        }

        #endregion
    }
}
