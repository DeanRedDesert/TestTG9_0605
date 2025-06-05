using System.Collections.Generic;

namespace Logic.Core.Engine
{
	/// <summary>
	/// Interface to access types in dynamically loaded dlls.
	/// </summary>
	// ReSharper disable UnusedMember.Global - Used in unity
	public interface IInputSets
	{
		/// <summary>
		/// Get the number of inputs sets.
		/// </summary>
		int GetCount();

		/// <summary>
		/// Gets the number of different input names.
		/// </summary>
		int GetInputCount();

		/// <summary>
		/// Gets the names of the inputs.
		/// </summary>
		IReadOnlyList<string> GetInputNames();

		/// <summary>
		/// Gets the names of the configuration inputs. This will be a subset of the <see cref="GetInputNames"/>.
		/// </summary>
		IReadOnlyList<string> GetConfigurationInputNames();

		/// <summary>
		/// Gets the possible values of a specific input in a specific input set
		/// </summary>
		IReadOnlyList<object> GetInputValues(int inputSetIndex, int inputIndex);

		/// <summary>
		/// Returns an <see cref="Inputs"/> populated with the specific inputs.
		/// Also will contain the total bet and cycle inputs.
		/// </summary>
		/// <param name="inputSetIndex">The input set index.</param>
		/// <param name="selections">The indexes of each selection in each input.</param>
		Inputs GetInputs(int inputSetIndex, IReadOnlyList<int> selections);
	}
}