//-----------------------------------------------------------------------
// <copyright file = "StreamingAction.cs" company = "IGT">
//     Copyright (c) 2013 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Presentation.PeripheralLights.Streaming.Choreography
{
    using System;
    using CompactSerialization;

    /// <summary>
    /// Represents a choreography action.
    /// </summary>
    public sealed class StreamingAction : IEquatable<StreamingAction>, ICompactSerializable
    {
        /// <summary>
        /// Create new instance.
        /// </summary>
        public StreamingAction()
        {
            Sequence = string.Empty;
        }

        /// <summary>
        /// The name of the light sequence.
        /// </summary>
        public string Sequence
        {
            get;
            set;
        }

        /// <summary>
        /// The action to perform.
        /// </summary>
        public ActionType Action
        {
            get;
            set;
        }

        /// <summary>
        /// The group ID to apply the action to.
        /// </summary>
        public ushort GroupId
        {
            get;
            set;
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
                    equals = Equals(obj as StreamingAction);
                }
            }

            return equals;
        }

        /// <inheritdoc />
        public bool Equals(StreamingAction other)
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
                    equals = Sequence == other.Sequence
                        && Action == other.Action
                        && GroupId == other.GroupId;
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

        /// <inheritdoc />
        void ICompactSerializable.Serialize(System.IO.Stream stream)
        {
            CompactSerializer.Write(stream, Sequence);
            CompactSerializer.Write(stream, Action);
            CompactSerializer.Write(stream, GroupId);
        }

        /// <inheritdoc />
        void ICompactSerializable.Deserialize(System.IO.Stream stream)
        {
            Sequence = CompactSerializer.ReadString(stream);
            Action = CompactSerializer.ReadEnum<ActionType>(stream);
            GroupId = CompactSerializer.ReadUshort(stream);
        }
    }
}
