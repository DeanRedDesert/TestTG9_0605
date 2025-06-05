//-----------------------------------------------------------------------
// <copyright file = "GameTimerStorage.cs" company = "IGT">
//     Copyright (c) 2015 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.StateMachine.GameTimer
{
    using System.IO;
    using System;
    using CompactSerialization;

    /// <summary>
    /// Class for storing the state and related information about a game timer; used in persisting to and restoring from critical data.
    /// </summary>
    [Serializable]
    internal class GameTimerStorage : ICompactSerializable
    {
        #region Fields

        /// <summary>
        /// The timer name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The timer tick interval specified in seconds.
        /// </summary>
        public uint TimerTickInterval { get; set; }

        /// <summary>
        /// The timer state persistence interval specified in seconds. Should be greater than or equal to the tick interval.
        /// </summary>
        public uint PersistenceInterval { get; set; }

        /// <summary>
        /// The target, ending value of the timer.
        /// </summary>
        public uint TargetValue { get; set; }

        /// <summary>
        /// The base, starting value of the timer.
        /// </summary>
        public uint BaseValue { get; set; }

        /// <summary>
        /// The current value of this timer when it was persisted to critical data.
        /// </summary>
        public uint CurrentValue { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Overloaded constructor - creates an instance based on an existing timer object.
        /// </summary>
        /// <param name="timer">An object of type <see cref="GameTimer"/>.</param>
        internal GameTimerStorage(GameTimer timer) : this()
        {
            BaseValue = timer.BaseValue;
            CurrentValue = timer.CurrentValue;
            Name = timer.Name;
            TargetValue = timer.TargetValue;
            TimerTickInterval = timer.TimerTickInterval;
            PersistenceInterval = timer.PersistenceInterval;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public GameTimerStorage()
        {
        }

        #endregion

        #region ICompactSerializable implementation

        /// <summary>
        /// Serialize this object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to use when serializing.</param>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="stream"/> is null.</exception>
        public void Serialize(Stream stream)
        {
            CompactSerializer.Write(stream, Name);
            CompactSerializer.Write(stream, BaseValue);
            CompactSerializer.Write(stream, CurrentValue);
            CompactSerializer.Write(stream, PersistenceInterval);
            CompactSerializer.Write(stream, TargetValue);
            CompactSerializer.Write(stream, TimerTickInterval);
        }

        /// <summary>
        /// Deserialize this object.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to use when deserializing.</param>
        /// <exception cref="ArgumentNullException">Thrown if <see paramref="stream"/> is null.</exception>
        public void Deserialize(Stream stream)
        {
            Name = CompactSerializer.ReadString(stream);
            BaseValue = CompactSerializer.ReadUint(stream);
            CurrentValue = CompactSerializer.ReadUint(stream);
            PersistenceInterval = CompactSerializer.ReadUint(stream);
            TargetValue = CompactSerializer.ReadUint(stream);
            TimerTickInterval = CompactSerializer.ReadUint(stream);
        }
    }

    #endregion
}