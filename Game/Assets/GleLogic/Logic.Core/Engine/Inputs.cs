using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Logic.Core.Utility;

namespace Logic.Core.Engine
{
	public sealed class Inputs : IReadOnlyList<Input>, IToString
	{
		public static readonly Inputs Empty = new Inputs(Array.Empty<Input>());

		private readonly IReadOnlyList<Input> inputs;
		private readonly IReadOnlyDictionary<string, Input> inputsDict;

		public Inputs(IReadOnlyList<Input> inputs)
		{
			this.inputs = inputs;
			inputsDict = inputs.ToDictionary(i => i.Name, i => i);
		}

		public static Inputs Create(params Input[] inputs) => new Inputs(inputs);

		#region Public Methods

		public int Count => inputs.Count;
		public Input this[int index] => inputs[index];
		public IEnumerator<Input> GetEnumerator() => inputs.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => inputs.GetEnumerator();

		/// <summary>
		/// Get the value of the input.
		/// </summary>
		public object GetInput(string name) => inputsDict[name].Value;

		/// <summary>
		/// Get the value of the input cast to the type T.
		/// </summary>
		public T GetInput<T>(string name) => (T)GetInput(name);

		/// <summary>
		/// Try get the value of the input, returns false if the input doesn't exist.
		/// </summary>
		public bool TryGetInput(string name, out object value)
		{
			if (inputsDict.TryGetValue(name, out var input))
			{
				value = input.Value;
				return true;
			}

			value = null;
			return false;
		}

		/// <summary>
		/// Get the value of the input cast to the type T, returns false if the input doesn't exist.
		/// </summary>
		public bool TryGetInput<T>(string name, out T value)
		{
			var found = TryGetInput(name, out var v);
			value = found ? (T)v : default;
			return found;
		}

		/// <summary>
		/// Helper to get the <see cref="Cycles"/> input.
		/// Will throw an exception if 'Cycles' doesn't exist.
		/// </summary>
		public Cycles GetCycles() => (Cycles)GetInput("Cycles");

		/// <summary>
		/// Helper to get the current stage of the <see cref="Cycles"/> input.
		/// Will throw an exception if 'Cycles' doesn't exist.
		/// </summary>
		public string CurrentStage() => TryGetInput<Cycles>("Cycles", out var c) ? c.Current.Stage : null;

		/// <summary>
		/// Add the inputsToAdd, then return a new Inputs collection.
		/// </summary>
		public Inputs Add(params Input[] inputsToAdd)
		{
			foreach (var input in inputsToAdd)
			{
				if (TryGetInput(input.Name, out _))
					throw new Exception($"Input {input.Name} already exists");
			}

			var newInputs = new List<Input>();
			newInputs.AddRange(this);
			newInputs.AddRange(inputsToAdd);
			return new Inputs(newInputs);
		}

		/// <summary>
		/// Overwrites or adds the inputs, then return a new Inputs collection.
		/// </summary>
		public Inputs ReplaceOrAdd<T>(IReadOnlyList<T> inputsToAdd) where T : Input
		{
			if (!inputsToAdd.Any())
				return this;

			var dict = new Dictionary<string, int>();

			for (var i = 0; i < inputsToAdd.Count; i++)
				dict[inputsToAdd[i].Name] = i;

			var newInputs = new List<Input>();

			foreach (var input in this)
			{
				if (dict.TryGetValue(input.Name, out var i))
				{
					newInputs.Add(inputsToAdd[i]);
					dict.Remove(input.Name);
				}
				else
				{
					newInputs.Add(input);
				}
			}

			foreach (var i in dict.Values.OrderBy(x => x))
				newInputs.Add(inputsToAdd[i]);

			return new Inputs(newInputs);
		}

		/// <summary>
		/// Overwrites the named input with the given value, then return a new Inputs collection.
		/// Throws an exception if the input doesn't exist.
		/// </summary>
		public Inputs Replace(string name, object value)
		{
			var newInputs = new List<Input>();
			var set = false;

			foreach (var input in this)
			{
				if (input.Name == name)
				{
					newInputs.Add(input.WithValue(value));
					set = true;
				}
				else
				{
					newInputs.Add(input);
				}
			}

			if (!set)
				throw new Exception($"Input {name} not found");

			return new Inputs(newInputs);
		}

		// ReSharper disable once UnusedMember.Global - Keep this for debugging
		public string ToDebugString() => string.Join(" ", inputs.OrderBy(i => i.Name).Select(i => $"{i.Name}={i.Value}"));

		#endregion

		/// <inheritdoc cref="IToString.ToString(string?)" />
		public IResult ToString(string format)
		{
			if (format == "ML")
			{
				var lines = new List<string>();

				foreach (var input in inputs)
				{
					if (input.Value is Cycles c)
						lines.Add($"{input.Name}:\n{c.ToStringOrThrow("ML").Indent(2, ' ')}");
					else if (StringConverter.TryToString(input.Value, "SL", out var sl))
						lines.Add($"{input.Name}: {sl}");
					else if (StringConverter.TryToString(input.Value, "ML", out var ml))
					{
						var sp = ml.ToLines(false, false);

						switch (sp.Count)
						{
							case 1: lines.Add($"{input.Name}: {sp[0]}"); break;
							default:
							{
								lines.Add($"{input.Name}:");

								foreach (var s in sp)
									lines.Add($"  {s}");

								break;
							}
						}
					}
					else
						lines.Add($"{input.Name}: <no conversion>");
				}

				return lines.Join().ToSuccess();
			}

			return new NotSupported();
		}
	}
}
