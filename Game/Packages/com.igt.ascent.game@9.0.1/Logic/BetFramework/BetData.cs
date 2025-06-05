//-----------------------------------------------------------------------
// <copyright file = "BetData.cs" company = "IGT">
//     Copyright (c) 2011 IGT.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace IGT.Game.Core.Logic.BetFramework
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Binary;
    using Attributes;
    using CompactSerialization;
    using Exceptions;
    using Interfaces;

    /// <summary>
    /// The high-level container for all data needed to generate bet lists and costs.
    /// </summary>
    public abstract class BetData : IBetData, ICompactSerializable
    {
        /// <summary>The binding flags used in reflection.</summary>
        public const BindingFlags ReflectionBindingFlags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        #region Private Fields

        /// <summary>A cache of all variables discovered at runtime.</summary>
        private Dictionary<string, MemberInfo> variables = new Dictionary<string, MemberInfo>();

        /// <summary>
        /// Flag indicating this bet object should commit credits.
        /// </summary>
        private bool commit;

        /// <summary>
        /// Flag indicating the bet starts a game (e.g. repeat bet or max bet)
        /// </summary>
        private bool startGame;

        #endregion

        /// <summary>
        /// Default constructor. <see cref="Commit"/> and <see cref="StartGame"/> are false.
        /// </summary>
        protected BetData()
        {
            foreach(var member in GetType().GetMembers(ReflectionBindingFlags))
            {
                if(member.GetCustomAttributes(typeof(BetVariableAttribute), true).Length > 0)
                {
                    if(member is PropertyInfo && !(member as PropertyInfo).CanRead)
                    {
                        // Write-only variables cannot be bet variables
                        throw new InvalidVariableException(member.Name);
                    }
                    variables.Add(member.Name, member);
                }
            }
        }

        /// <summary>
        /// Constructor. Sets <see cref="Commit"/> and <see cref="StartGame"/> to <paramref name="commitAndStartGame"/>.
        /// </summary>
        /// <param name="commitAndStartGame">
        /// Initial value of the <see cref="Commit"/> and <see cref="StartGame"/> flags.
        /// </param>
        protected BetData(bool commitAndStartGame)
            : this()
        {
            Commit = commitAndStartGame;
            StartGame = commitAndStartGame;
        }

        /// <summary>
        /// Constructor. Sets <see cref="Commit"/> to <paramref name="commit"/>
        /// and <see cref="StartGame"/> to <paramref name="startGame"/>.
        /// </summary>
        /// <param name="commit">Initial value of the <see cref="Commit"/> flag.</param>
        /// <param name="startGame">Initial value of the <see cref="StartGame"/> flag.</param>
        /// <remarks>
        /// If <paramref name="startGame"/> is true, Commit is also
        /// set true regardless of the <paramref name="commit"/> parameter.
        /// </remarks>
        protected BetData(bool commit, bool startGame)
            : this()
        {
            Commit = commit;
            StartGame = startGame;
        }

        #region IBetData

        /// <inheritdoc/>
        [BetVariable]
        public bool Enabled { get; set; }

        /// <inheritdoc/>
        [BetVariable]
        public bool Commit
        {
            get => commit;
            set
            {
                commit = value;
                startGame &= commit; // if commit is set false, we can't start
            }
        }

        /// <inheritdoc/>
        [BetVariable]
        public bool StartGame
        {
            get => startGame;
            set
            {
                startGame = value;
                commit |= startGame; // if start is set true, we must commit
            }
        }

        /// <inheritdoc/>
        [BetVariable]
        public bool BetChanged { get; set; }

        /// <inheritdoc/>
        public TVariable GetVariable<TVariable>(string variableName)
        {
            if(variableName == null)
            {
                throw new ArgumentNullException(nameof(variableName));
            }
            if(!variables.ContainsKey(variableName))
            {
                throw new InvalidVariableException(variableName);
            }

            var variable = variables[variableName];

            var field = variable as FieldInfo;
            if(field != null)
            {
                if(field.FieldType != typeof(TVariable))
                {
                    throw new VariableTypeException(variableName, typeof(TVariable), field.FieldType);
                }
                return (TVariable)field.GetValue(field.IsStatic ? null : this);
            }

            var property = variable as PropertyInfo;
            if(property != null)
            {
                if(property.PropertyType != typeof(TVariable))
                {
                    throw new VariableTypeException(variableName, typeof(TVariable), property.PropertyType);
                }
                var getter = property.GetGetMethod(true);
                return (TVariable)property.GetValue(getter.IsStatic ? null : this, null);
            }

            throw new InvalidVariableException(variableName);
        }

        /// <inheritdoc/>
        public void SetVariable(string variableName, object value)
        {
            if(variableName == null)
            {
                throw new ArgumentNullException(nameof(variableName));
            }
            if(!variables.ContainsKey(variableName))
            {
                throw new InvalidVariableException(variableName);
            }

            var variable = variables[variableName];

            var field = variable as FieldInfo;
            if(field != null)
            {
                if(field.IsInitOnly)
                {
                    throw new ReadOnlyVariableException(field);
                }

                if(!field.FieldType.IsInstanceOfType(value))
                {
                    throw new VariableTypeException(field.Name, value.GetType(), field.FieldType);
                }

                field.SetValue(this, value);
            }

            var property = variable as PropertyInfo;
            if(property != null)
            {
                if(!property.CanWrite)
                {
                    throw new ReadOnlyVariableException(property);
                }

                if(!property.PropertyType.IsInstanceOfType(value))
                {
                    throw new VariableTypeException(property.Name, value.GetType(), property.PropertyType);
                }

                property.SetValue(this, value, null);
            }

            if(field == null && property == null)
            {
                throw new InvalidVariableException(variableName);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<MemberInfo> GetVariables()
        {
            return variables.Values;
        }

        /// <inheritdoc/>
        public virtual bool IsValid()
        {
            return true;
        }

        /// <inheritdoc/>
        public abstract long Total();

        /// <inheritdoc/>
        public abstract IEnumerable<KeyValuePair<string, long>> BetDefinitions();

        /// <inheritdoc/>
        public IBetData Clone()
        {
            var betData = MemberwiseClone() as BetData;
            if(betData != null && variables != null)
            {
                betData.variables = new Dictionary<string, MemberInfo>(variables);
            }
            return betData;
        }

        /// <inheritdoc/>
        public TBetData Clone<TBetData>() where TBetData : class, IBetData
        {
            if(!(Clone() is TBetData betData))
            {
                throw new InvalidBetTypeException(typeof(IBetData), typeof(TBetData));
            }
            return betData;
        }

        #endregion

        #region ICompactSerializable

        /// <inheritdoc/>
        public void Serialize(Stream stream)
        {
            var formatter = new BinaryFormatter();

            CompactSerializer.Write(stream, variables.Count);
            foreach(var pair in variables)
            {
                CompactSerializer.Write(stream, pair.Key);

                using(var memory = new MemoryStream())
                {
                    var field = pair.Value as FieldInfo;
                    if(field != null)
                    {
                        var value = field.GetValue(field.IsStatic ? null : this);
                        formatter.Serialize(memory, value);
                    }
                    var property = pair.Value as PropertyInfo;
                    if(property != null)
                    {
                        var getter = property.GetGetMethod(true);
                        var value = property.GetValue(getter.IsStatic ? null : this, null);
                        formatter.Serialize(memory, value);
                    }
                    CompactSerializer.Serialize(stream, memory.GetBuffer());
                }
            }
        }

        /// <inheritdoc/>
        public void Deserialize(Stream stream)
        {
            var formatter = new BinaryFormatter();
            var count = CompactSerializer.ReadInt(stream);
            for(var index = 0; index < count; index++)
            {
                var key = CompactSerializer.ReadString(stream);
                var memberInfo = variables[key];

                var bytes = CompactSerializer.ReadByteArray(stream);
                object value;
                using(var memory = new MemoryStream(bytes))
                {
                    value = formatter.Deserialize(memory);
                }

                var field = memberInfo as FieldInfo;
                if(field != null && !field.IsLiteral) // don't attempt to write a literal value
                {
                    field.SetValue(field.IsStatic ? null : this, value);
                }
                var property = memberInfo as PropertyInfo;
                if(property != null && property.CanWrite) // can't write to a read-only property
                {
                    var setter = property.GetSetMethod(true);
                    property.SetValue(setter.IsStatic ? null : this, value, null);
                }
            }
        }

        #endregion
    }
}
