//-----------------------------------------------------------------------
// <copyright file = "Trigger.Extensions.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.Evaluator.Schemas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using System.Text;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Additions to the schema generated Trigger class.
    /// </summary>
    public partial class Trigger : ICompactSerializable, IDeepCloneable, IEquatable<Trigger>
    {
        /// <summary>
        /// Override function, provide content of Trigger in string format.
        /// </summary>
        public override string ToString()
        {
            var resultBuilder = new StringBuilder("==================Trigger===============\n");
            resultBuilder.AppendLine("Trigger name = " + name);
            resultBuilder.AppendLine("PlayCount = " + PlayCount);
            resultBuilder.AppendLine("MaxPlayCount = " + MaxPlayCount);
            resultBuilder.AppendLine("MaxRetriggers = " + MaxRetriggers);
            resultBuilder.AppendLine("Multiplier = " + Multiplier);
            resultBuilder.AppendLine("MultiplierSpecified = " + MultiplierSpecified);
            resultBuilder.AppendLine("==================Trigger items are listed.===============");
            return resultBuilder.ToString();
        }

        /// <summary>
        /// Construct a new trigger.
        /// </summary>
        public Trigger()
        {
        }

        /// <summary>
        /// Construct a new trigger with the contents of another.
        /// </summary>
        /// <param name="other">Trigger to copy.</param>
        public Trigger(Trigger other)
        {
            CopyMembers(other);
        }

        /// <summary>
        /// Copy the members of another trigger to this one.
        /// </summary>
        /// <param name="other">Trigger to copy.</param>
        private void CopyMembers(Trigger other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other", "Other trigger may not be null.");
            }

            name = other.name;
            executionPriority = other.executionPriority;

            PlayCount = other.PlayCount;
            MaxPlayCount = other.MaxPlayCount;
            MaxRetriggers = other.MaxRetriggers;

            MultiplierSpecified = other.MultiplierSpecified;
            Multiplier = other.Multiplier;
        }

        #region ICompactSerializable Members

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, name);
            CompactSerializer.Write(stream, executionPriority);

            CompactSerializer.Write(stream, PlayCount);
            CompactSerializer.Write(stream, MaxPlayCount);
            CompactSerializer.Write(stream, MaxRetriggers);

            CompactSerializer.Write(stream, MultiplierSpecified);
            CompactSerializer.Write(stream, Multiplier);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            name = CompactSerializer.ReadString(stream);
            executionPriority = CompactSerializer.ReadUint(stream);

            PlayCount = CompactSerializer.ReadUint(stream);
            MaxPlayCount = CompactSerializer.ReadUint(stream);
            MaxRetriggers = CompactSerializer.ReadUint(stream);

            MultiplierSpecified = CompactSerializer.ReadBool(stream);
            Multiplier = CompactSerializer.ReadUint(stream);
        }

        #endregion

        #region IDeepCloneable Members

        /// <inheritdoc />
        public object DeepClone()
        {
            return new Trigger(this);
        }

        #endregion

        #region IEquatable<Trigger> Members

        /// <inheritdoc />
        public bool Equals(Trigger other)
        {
            if(other == null)
            {
                return false;
            }

            return name == other.name
                   && executionPriority == other.executionPriority
                   && PlayCount == other.PlayCount
                   && MaxPlayCount == other.MaxPlayCount
                   && MaxRetriggers == other.MaxRetriggers
                   && MultiplierSpecified == other.MultiplierSpecified
                   && Multiplier == other.Multiplier;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var result = false;
            var other = obj as Trigger;
            if(other != null)
            {
                result = Equals(other);
            }

            return result;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hash = 23;

            hash = name != null ? hash * 37 + name.GetHashCode() : hash;
            hash = hash * 37 + executionPriority.GetHashCode();
            hash = hash * 37 + PlayCount.GetHashCode();
            hash = hash * 37 + MaxPlayCount.GetHashCode();
            hash = hash * 37 + MaxRetriggers.GetHashCode();
            hash = hash * 37 + MultiplierSpecified.GetHashCode();
            hash = hash * 37 + Multiplier.GetHashCode();

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal. False otherwise.</returns>
        public static bool operator ==(Trigger left, Trigger right)
        {
            if(ReferenceEquals(left, right))
            {
                return true;
            }

            if((object)left == null || (object)right == null)
            {
                return false;
            }

            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal. False otherwise.</returns>
        public static bool operator !=(Trigger left, Trigger right)
        {
            return !(left == right);
        }

        #endregion
    }

    /// <summary>
    /// Extensions to the type <![CDATA[List<Trigger>]]>.
    /// </summary>
    public static class TriggerListExtensions
    {
        /// <summary>
        /// Extend a list of triggers with the contents of another one.
        /// </summary>
        /// <param name="triggers">Trigger list  to extend.</param>
        /// <param name="triggersToAdd">List of triggers to add to the other list.</param>
        public static void AddRangeCopy(this List<Trigger> triggers, List<Trigger> triggersToAdd)
        {
            triggers.AddRange(triggersToAdd.Select(trigger => new Trigger(trigger)));
        }
    }
}
