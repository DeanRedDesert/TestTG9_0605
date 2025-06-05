// -----------------------------------------------------------------------
// <copyright file = "RuntimeGameEventsConfiguration.cs" company = "IGT">
//     Copyright (c) 2020 IGT.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace IGT.Game.Core.Communication.Foundation.InterfaceExtensions.RuntimeGameEvents
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using Cloneable;
    using CompactSerialization;

    /// <summary>
    /// Container class for runtime game events configuration.
    /// </summary>
    public class RuntimeGameEventsConfiguration : ICompactSerializable, IDeepCloneable,
        IEquatable<RuntimeGameEventsConfiguration>
    {
        #region Properties

        /// <summary>
        /// Gets the flag indicating if the client should report the WaitingForGenericInput status change.
        /// </summary>
        public bool WaitingForGenericInputStatusUpdateEnabled { get; private set; }

        /// <summary>
        /// Gets the flag indicating if the client should report the PlayerChoice status change.
        /// </summary>
        public bool PlayerChoiceUpdateEnabled { get; private set; }

        /// <summary>
        /// Gets the flag indicating if the client should report the GameSelection status change.
        /// </summary>
        public bool GameSelectionStatusUpdateEnabled { get; private set; }

        /// <summary>
        /// Gets the flag indicating if the client should report the GameBetMeter change.
        /// </summary>
        public bool GameBetMeterUpdateEnabled { get; private set; }

        /// <summary>
        /// Get the flag indicating if the client should report the GameBetMeterKeys change.
        /// </summary>
        public bool GameBetMeterKeysUpdateEnabled { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs an instance of <see cref="RuntimeGameEventsConfiguration"/>.
        /// </summary>
        /// <remarks>
        /// Types implementing ICompactSerializable must have a public parameter-less constructor.
        /// </remarks>
        public RuntimeGameEventsConfiguration()
        {
        }

        /// <summary>
        /// Constructs an instance of <see cref="RuntimeGameEventsConfiguration"/>.
        /// </summary>
        /// <param name="waitingForGenericInputStatusUpdateEnabled">
        /// If the WaitingForGenericInputStatusUpdate message is enabled.
        /// </param>
        /// <param name="playerChoiceUpdateEnabled">
        /// If the PlayerChoiceUpdate message is enabled.
        /// </param>
        /// <param name="gameSelectionStatusUpdateEnabled">
        /// If the GameSelectionStatusUpdate message is enabled.
        /// </param>
        /// <param name="gameBetMeterUpdateEnabled">
        /// If the GameBetMeterUpdate message is enabled.
        /// </param>
        /// <param name="gameBetMeterKeysUpdateEnabled">
        /// If the GameBetMeterKeysUpdate message is enabled.
        /// </param>
        public RuntimeGameEventsConfiguration(
            bool waitingForGenericInputStatusUpdateEnabled,
            bool playerChoiceUpdateEnabled,
            bool gameSelectionStatusUpdateEnabled,
            bool gameBetMeterUpdateEnabled,
            bool gameBetMeterKeysUpdateEnabled
        )
        {
            WaitingForGenericInputStatusUpdateEnabled = waitingForGenericInputStatusUpdateEnabled;
            PlayerChoiceUpdateEnabled = playerChoiceUpdateEnabled;
            GameSelectionStatusUpdateEnabled = gameSelectionStatusUpdateEnabled;
            GameBetMeterUpdateEnabled = gameBetMeterUpdateEnabled;
            GameBetMeterKeysUpdateEnabled = gameBetMeterKeysUpdateEnabled;
        }

        #endregion

        #region Implementation of ICompactSerializable

        /// <inheritdoc />
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, WaitingForGenericInputStatusUpdateEnabled);
            CompactSerializer.Write(stream, PlayerChoiceUpdateEnabled);
            CompactSerializer.Write(stream, GameSelectionStatusUpdateEnabled);
            CompactSerializer.Write(stream, GameBetMeterUpdateEnabled);
            CompactSerializer.Write(stream, GameBetMeterKeysUpdateEnabled);
        }

        /// <inheritdoc />
        public void Deserialize(Stream stream)
        {
            WaitingForGenericInputStatusUpdateEnabled = CompactSerializer.ReadBool(stream);
            PlayerChoiceUpdateEnabled = CompactSerializer.ReadBool(stream);
            GameSelectionStatusUpdateEnabled = CompactSerializer.ReadBool(stream);
            GameBetMeterUpdateEnabled = CompactSerializer.ReadBool(stream);
            GameBetMeterKeysUpdateEnabled = CompactSerializer.ReadBool(stream);
        }

        #endregion

        #region Implementation of IDeepCloneable

        /// <inheritdoc />
        public object DeepClone()
        {
            var copy = new RuntimeGameEventsConfiguration(
                WaitingForGenericInputStatusUpdateEnabled,
                PlayerChoiceUpdateEnabled,
                GameSelectionStatusUpdateEnabled,
                GameBetMeterUpdateEnabled,
                GameBetMeterKeysUpdateEnabled
            );

            return copy;
        }

        #endregion

        #region Equality members, generated by ReSharper

        /// <inheritdoc />
        public bool Equals(RuntimeGameEventsConfiguration other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return WaitingForGenericInputStatusUpdateEnabled == other.WaitingForGenericInputStatusUpdateEnabled &&
                   PlayerChoiceUpdateEnabled == other.PlayerChoiceUpdateEnabled &&
                   GameSelectionStatusUpdateEnabled == other.GameSelectionStatusUpdateEnabled &&
                   GameBetMeterUpdateEnabled == other.GameBetMeterUpdateEnabled &&
                   GameBetMeterKeysUpdateEnabled == other.GameBetMeterKeysUpdateEnabled;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != GetType()) return false;
            return Equals((RuntimeGameEventsConfiguration)obj);
        }

        /// <inheritdoc />
        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = WaitingForGenericInputStatusUpdateEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ PlayerChoiceUpdateEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ GameSelectionStatusUpdateEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ GameBetMeterUpdateEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ GameBetMeterKeysUpdateEnabled.GetHashCode();
                return hashCode;
            }
        }

        /// <inheritDoc/>
        public static bool operator ==(RuntimeGameEventsConfiguration left, RuntimeGameEventsConfiguration right)
        {
            return Equals(left, right);
        }

        /// <inheritDoc/>
        public static bool operator !=(RuntimeGameEventsConfiguration left, RuntimeGameEventsConfiguration right)
        {
            return !Equals(left, right);
        }

        #endregion

        /// <summary>
        /// Override base implementation to provide better information.
        /// </summary>
        /// <returns>
        /// A string describing the object.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder("Runtime Game Events Configuration:");
            sb.AppendLine($"\tWaitingForGenericInputStatusUpdateEnabled: {WaitingForGenericInputStatusUpdateEnabled}");
            sb.AppendLine($"\tPlayerChoiceUpdateEnabled: {PlayerChoiceUpdateEnabled}");
            sb.AppendLine($"\tGameSelectionStatusUpdateEnabled: {GameSelectionStatusUpdateEnabled}");
            sb.AppendLine($"\tGameBetMeterUpdateEnabled: {GameBetMeterUpdateEnabled}");
            sb.AppendLine($"\tGameBetMeterKeysUpdateEnabled: {GameBetMeterKeysUpdateEnabled}");

            return sb.ToString();
        }
    }
}
