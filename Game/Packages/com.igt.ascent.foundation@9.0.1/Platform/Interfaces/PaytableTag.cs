//-----------------------------------------------------------------------
// <copyright file = "PaytableTag.cs" company = "IGT">
//     Copyright (c) 2014 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Ascent.Communication.Platform.Interfaces
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Struct that represents the identification of a specific paytable in
    /// a specific paytable file.
    /// </summary>
    [Serializable]
    public readonly struct PaytableTag : IEquatable<PaytableTag>
    {
        /// <summary>
        /// Gets the identifier of the paytable. This is the identifier maintained by
        /// the Foundation, used in communication with the Foundation to identify an
        /// individual "pay variation".
        /// </summary>
        public string PaytableIdentifier { get; }

        /// <summary>
        /// Gets the name of the file that contains the paytable which corresponds to
        /// the Tag Data File Name specified in the payvar registry.
        /// </summary>
        public string PaytableFileName { get; }

        /// <summary>
        /// Gets the name of the paytable which corresponds to the Tag Name specified in
        /// the payvar registry.
        /// </summary>
        public string PaytableName { get; }

        /// <summary>
        /// Gets the flag indicating if this paytable is in a paytable group.
        /// </summary>
        public bool IsInPaytableGroup { get; }

        /// <summary>
        /// Gets the group tag file name for this paytable which corresponds to the Group
        /// Tag File Name specified in the payvar group registry.
        /// Meaningful only when <see cref="IsInPaytableGroup"/> is true.
        /// </summary>
        public string GroupTagFileName { get; }

        /// <summary>
        /// Gets the group tag name for this paytable which corresponds to the Group Tag
        /// Name specified in the payvar group registry.
        /// Meaningful only when <see cref="IsInPaytableGroup"/> is true.
        /// </summary>
        public string GroupTagName { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="PaytableTag"/> which is not a member
        /// of a paytable group.
        /// </summary>
        /// <param name="paytableIdentifier">
        /// The foundation-maintained identifier of the paytable or "pay variation".
        /// </param>
        /// <param name="paytableFileName">The name of the file that contains the paytable.</param>
        /// <param name="paytableName">The name of the paytable.</param>
        public PaytableTag(string paytableIdentifier, string paytableFileName, string paytableName)
            : this(paytableIdentifier, paytableFileName, paytableName, false, null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="PaytableTag"/>.
        /// </summary>
        /// <param name="paytableIdentifier">
        /// The foundation-maintained identifier of the paytable or "pay variation".
        /// </param>
        /// <param name="paytableFileName">The name of the file that contains the paytable.</param>
        /// <param name="paytableName">The name of the paytable.</param>
        /// <param name="isInPaytableGroup">The flag indicating if this paytable is in a paytable group.</param>
        /// <param name="groupTagFileName">The group tag file name for the paytable.</param>
        /// <param name="groupTagName">The group tag name for this paytable.</param>
        public PaytableTag(string paytableIdentifier, string paytableFileName, string paytableName,
            bool isInPaytableGroup, string groupTagFileName, string groupTagName)
            : this()
        {
            PaytableIdentifier = paytableIdentifier ?? string.Empty;
            PaytableFileName = paytableFileName ?? string.Empty;
            PaytableName = paytableName ?? string.Empty;
            IsInPaytableGroup = isInPaytableGroup;
            GroupTagFileName = groupTagFileName ?? string.Empty;
            GroupTagName = groupTagName ?? string.Empty;
        }

        #region IEquatable<PaytableTag> Members

        /// <inheritdoc/>
        public bool Equals(PaytableTag rightHand)
        {
            return PaytableIdentifier == rightHand.PaytableIdentifier &&
                   PaytableName == rightHand.PaytableName &&
                   PaytableFileName == rightHand.PaytableFileName &&
                   IsInPaytableGroup == rightHand.IsInPaytableGroup &&
                   GroupTagFileName == rightHand.GroupTagFileName &&
                   GroupTagName == rightHand.GroupTagName;
        }

        #endregion

        /// <summary>
        /// Override value type's implementation for better performance.
        /// </summary>
        /// <param name="obj">The right hand object for the equality check.</param>
        /// <returns>True if the right hand object equals to this object.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            var result = false;

            if (obj is PaytableTag)
            {
                result = Equals((PaytableTag)obj);
            }

            return result;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <returns>The hash code generated.</returns>
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            var hash = 23;

            hash = PaytableIdentifier != null ? hash * 37 + PaytableIdentifier.GetHashCode() : hash;
            hash = PaytableName != null ? hash * 37 + PaytableName.GetHashCode() : hash;
            hash = PaytableFileName != null ? hash * 37 + PaytableFileName.GetHashCode() : hash;
            hash = hash * 37 + IsInPaytableGroup.GetHashCode();
            hash = GroupTagFileName != null ? hash * 37 + GroupTagFileName.GetHashCode() : hash;
            hash = GroupTagName != null ? hash * 37 + GroupTagName.GetHashCode() : hash;

            return hash;
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are considered equal.  False otherwise.</returns>
        public static bool operator ==(PaytableTag left, PaytableTag right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to go with the overridden Equals method.
        /// </summary>
        /// <param name="left">Left operand.</param>
        /// <param name="right">Right operand.</param>
        /// <returns>True if two operands are not considered equal.  False otherwise.</returns>
        public static bool operator !=(PaytableTag left, PaytableTag right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>A string describing the object.</returns>
        public override string ToString()
        {
            return
                $"Paytable Id({PaytableIdentifier})/File({PaytableFileName})/Paytable({PaytableName})/IsInPaytableGroup({IsInPaytableGroup}/GroupTagFile({GroupTagFileName})/GroupTag({GroupTagName}))";
        }
    }
}
