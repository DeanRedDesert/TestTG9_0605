using UnityEngine;
using UnityEngine.Events;

namespace Midas.Presentation.Data.PropertyReference
{
	public sealed class PropertyRefBoolUnityEvent : PropertyRefBool
	{
		[SerializeField]
		private UnityEvent eventIfTrue = new UnityEvent();

		[SerializeField]
		private UnityEvent eventIfFalse = new UnityEvent();

		protected override void Refresh(bool value)
		{
			if (value)
				eventIfTrue.Invoke();
			else
				eventIfFalse.Invoke();
		}

		#region Editor

#if UNITY_EDITOR
		public void ConfigureForMakeGame(string path, (UnityAction<bool> Action, bool Value) trueAction, (UnityAction<bool> Action, bool Value) falseAction)
		{
			base.ConfigureForMakeGame(path);

			UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(eventIfTrue, trueAction.Action, trueAction.Value);
			UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(eventIfFalse, falseAction.Action, falseAction.Value);
		}
#endif

		#endregion
	}
}