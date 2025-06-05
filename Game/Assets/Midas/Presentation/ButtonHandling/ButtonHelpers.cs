using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Midas.Core;
using Midas.Core.ExtensionMethods;

namespace Midas.Presentation.ButtonHandling
{
	/// <summary>
	///     PhysicalButton ids used for receiving button events and button state changed information
	///     These button functions are mapped to buttons depending on the mechanical button panel
	/// </summary>
	public static class ButtonHelpers
	{
		#region Public

		public static int GetButtonFunctionIndexOfId(int value)
		{
			UpdateButtonFunctions();

			return allButtonFunctions.FindIndex(s => s.Function.Id == value);
		}

		public static int GetButtonFunctionIndexOfName(string name)
		{
			UpdateButtonFunctions();

			return allButtonFunctions.FindIndex(s => s.Function.Name.Equals(name));
		}

		public static ButtonFunction GetButtonFunctionOfName(string name)
		{
			UpdateButtonFunctions();

			return allButtonFunctions.FirstOrDefault(s => s.Function.Name.Equals(name)).Function;
		}

		public static ButtonFunction GetButtonFunctionOfId(int value)
		{
			UpdateButtonFunctions();
			return allButtonFunctions.FirstOrDefault(s => s.Function.Id == value).Function;
		}

		public static string GetButtonFunctionNameOfId(int value)
		{
			var idx = GetButtonFunctionIndexOfId(value);
			return idx == -1 ? string.Empty : allButtonFunctions[idx].Name;
		}

		public static IReadOnlyList<(string PathName, string Name, ButtonFunction Function)> AllButtonFunctions
		{
			get
			{
				UpdateButtonFunctions();
				return allButtonFunctions;
			}
		}

		#endregion

		#region Private

		private static void UpdateButtonFunctions()
		{
			if (allButtonFunctions != null)
			{
				return;
			}

			var selectableButtonFunctions = new List<(string PathName, string Name, ButtonFunction Function)>();
			foreach (var type in ReflectionUtil.GetAllTypes(
						t => t.GetCustomAttributes(typeof(ButtonFunctionsAttribute), false).FirstOrDefault()
							is ButtonFunctionsAttribute))
			{
				AddButtonFunctionProperties(selectableButtonFunctions, type);
			}

			// sort them and make sure Undefined us the last
			selectableButtonFunctions.RemoveAt(selectableButtonFunctions.FindIndex(b => b.Function.Equals(ButtonFunction.Undefined)));
			selectableButtonFunctions.Sort();
			selectableButtonFunctions.Add((ButtonFunction.Undefined.Name, ButtonFunction.Undefined.Name, ButtonFunction.Undefined));

			allButtonFunctions = selectableButtonFunctions.ToArray();
		}

		private static void AddButtonFunctionProperties(List<(string PathName, string Name, ButtonFunction Function)> selectableButtonFunctions, Type type)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
			foreach (var property in properties)
			{
				if (property.GetValue(null) is ButtonFunction buttonFunction)
				{
					var idx = selectableButtonFunctions.FindIndex(b => b.Name.Equals(buttonFunction.Name));
					if (idx != -1)
					{
						throw new InvalidProgramException($"Button function with same name='{buttonFunction.Name}' exists '{selectableButtonFunctions[idx].PathName}'");
					}

					idx = selectableButtonFunctions.FindIndex(b => b.Function.Id == buttonFunction.Id);
					if (idx != -1)
					{
						throw new InvalidProgramException(
							$"Button function with same id='{buttonFunction.Id}':'{buttonFunction.Name}' exists '{selectableButtonFunctions[idx].PathName}'");
					}

					var group = !(type.GetCustomAttribute(typeof(ButtonFunctionsAttribute), false) is ButtonFunctionsAttribute buttonFunctionsAttribute)
						|| string.IsNullOrEmpty(buttonFunctionsAttribute.Group)
							? string.Empty
							: $@"{buttonFunctionsAttribute.Group}/";
					selectableButtonFunctions.Add((group + buttonFunction.Name, buttonFunction.Name, buttonFunction));
				}
			}
		}

		private static IReadOnlyList<(string PathName, string Name, ButtonFunction Function)> allButtonFunctions;

		#endregion
	}
}