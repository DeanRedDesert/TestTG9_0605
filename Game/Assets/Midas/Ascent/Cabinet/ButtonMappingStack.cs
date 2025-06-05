using System;
using System.Collections.Generic;
using System.Linq;
using ButtonMappingHandle = Midas.Presentation.ButtonHandling.ButtonMappingHandle;
using PhysicalButton = Midas.Presentation.ButtonHandling.PhysicalButton;
using ButtonFunction = Midas.Presentation.ButtonHandling.ButtonFunction;

namespace Midas.Ascent.Cabinet
{
	using PhysicalButton2ButtonFunctionList = IReadOnlyList<(PhysicalButton Button, ButtonFunction ButtonFunction)>;
	using PhysicalButton2ButtonFunctions = IEnumerable<(PhysicalButton Button, ButtonFunction ButtonFunction)>;

	internal sealed class ButtonMappingStack
	{
		private readonly List<(ButtonMappingHandle handle, PhysicalButton2ButtonFunctionList buttonMapping)> buttonMappingStack =
			new List<(ButtonMappingHandle handle, PhysicalButton2ButtonFunctionList buttonMapping)>();

		private readonly List<(ButtonMappingHandle handle, Func<PhysicalButton2ButtonFunctions> buttonMapping)> buttonMappingFunctionStack =
			new List<(ButtonMappingHandle handle, Func<PhysicalButton2ButtonFunctions> buttonMapping)>();

		public PhysicalButton2ButtonFunctionList DefaultButtonToButtonFunctionMapping => buttonMappingStack.Count > 0 ? buttonMappingStack.First().buttonMapping : null;

		public PhysicalButton2ButtonFunctionList ActiveButtonMapping => buttonMappingStack.Count > 0 ? buttonMappingStack.Last().buttonMapping : null;

		public IEnumerable<PhysicalButton2ButtonFunctionList> ButtonMappingHistory => buttonMappingStack.Select(item => item.buttonMapping);

		public ButtonMappingHandle MergeAndPushButtonMapping(Func<PhysicalButton2ButtonFunctions> buttonToFunctionMapFunction)
		{
			var handle = new ButtonMappingHandle();
			buttonMappingFunctionStack.Add((handle, buttonToFunctionMapFunction));
			Apply(handle, buttonToFunctionMapFunction);
			return handle;
		}

		public void PopButtonMapping(ButtonMappingHandle handle)
		{
			// Check if handle is the last one pushed on the stack.

			var index = FindHandleIndex(handle);
			if (index == -1)
			{
				Log.Instance.Error("PopButtonMapping tried to remove buttonMapping which is not found anymore");
			}
			else if (index == buttonMappingFunctionStack.Count - 1)
			{
				RemoveLast(handle); //shortcut, we can remove the last without any issue
			}
			else if (index > -1)
			{
				buttonMappingFunctionStack.RemoveAt(index);
				ReApplyStack();
			}
		}

		public ButtonFunction GetButtonFunctionFromButton(PhysicalButton button)
		{
			return ActiveButtonMapping?.FirstOrDefault(bm => bm.Button == button).ButtonFunction
				?? ButtonFunction.Undefined;
		}

		public ButtonFunction GetDefaultButtonFunctionFromButton(PhysicalButton button)
		{
			return DefaultButtonToButtonFunctionMapping.FirstOrDefault(bm => bm.Button == button).ButtonFunction;
		}

		public IEnumerable<PhysicalButton> GetDefaultButtonsFromButtonFunction(ButtonFunction function)
		{
			return DefaultButtonToButtonFunctionMapping
				.Where(bm => bm.ButtonFunction.Equals(function))
				.Select(bm => bm.Button);
		}

		public void OnAfterUnLoadGame()
		{
			buttonMappingStack.Clear();
			buttonMappingFunctionStack.Clear();
		}

		public void SetDefaultMapping(PhysicalButton2ButtonFunctionList newMapping)
		{
			// Check if the mapping is same as prev one

			var previousMapping = DefaultButtonToButtonFunctionMapping;
			if (newMapping == null && previousMapping == null)
			{
				return;
			}

			if (newMapping != null && previousMapping != null && previousMapping.SequenceEqual(newMapping))
			{
				return;
			}

			if (previousMapping == null)
			{
				buttonMappingFunctionStack.Add((new ButtonMappingHandle(), () => newMapping));
			}
			else
			{
				var handle = buttonMappingFunctionStack[0].handle;
				buttonMappingFunctionStack[0] = (handle, () => newMapping);
			}

			ReApplyStack();
		}

		private void ReApplyStack()
		{
			buttonMappingStack.Clear();
			foreach (var mappingFunctionPair in buttonMappingFunctionStack)
			{
				Apply(mappingFunctionPair.handle, mappingFunctionPair.buttonMapping);
			}
		}

		private void Apply(ButtonMappingHandle handle, Func<PhysicalButton2ButtonFunctions> buttonToFunctionMapFunction)
		{
			var buttonToFunctionMap = buttonToFunctionMapFunction();
			var activeButtonMapping = ActiveButtonMapping;

			var mergedButtonToFunction = activeButtonMapping != null
				? activeButtonMapping.ToList()
				: new List<(PhysicalButton Button, ButtonFunction ButtonFunction)>();
			foreach (var valueTuple in buttonToFunctionMap)
			{
				var index = mergedButtonToFunction.FindIndex(entry => entry.Button == valueTuple.Button);
				if (index != -1)
				{
					mergedButtonToFunction[index] = valueTuple;
				}
				else
				{
					mergedButtonToFunction.Add(valueTuple);
				}
			}

			buttonMappingStack.Add((handle, mergedButtonToFunction));
		}

		private void RemoveLast(ButtonMappingHandle handle)
		{
			var index = FindHandleIndex(handle);
			if (index == -1 || index != buttonMappingFunctionStack.Count - 1)
			{
				Log.Instance.Error("The handle {handle} is not the last element but {index}");
				return;
			}

			buttonMappingFunctionStack.RemoveAt(index);
			buttonMappingStack.RemoveAt(index);
		}

		private int FindHandleIndex(ButtonMappingHandle handle)
		{
			//check if handle is the last one pushed on the stack.
			for (var i = 0; i < buttonMappingFunctionStack.Count; ++i)
			{
				if (ReferenceEquals(handle, buttonMappingFunctionStack[i].handle))
				{
					return i;
				}
			}

			return -1;
		}
	}
}