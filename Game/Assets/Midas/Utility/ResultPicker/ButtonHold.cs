using System;
using Midas.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Midas.Utility.ResultPicker
{
	public class ButtonHold : Selectable
	{
		private TimeSpan nextTriggerTime;

		public Button.ButtonClickedEvent OnPress
		{
			get { return onClick; }
			set { onClick = value; }
		}

		// Event delegates triggered on click.
		[SerializeField]
		private Button.ButtonClickedEvent onClick = new Button.ButtonClickedEvent();

		// Start is called before the first frame update
		public void Update()
		{
			if (IsPressed() && FrameTime.CurrentTime >= nextTriggerTime)
			{
				nextTriggerTime = FrameTime.CurrentTime + TimeSpan.FromMilliseconds(200);
				OnPress.Invoke();
			}
		}

		public override void OnPointerDown(PointerEventData data)
		{
			base.OnPointerDown(data);
			nextTriggerTime = FrameTime.CurrentTime + TimeSpan.FromMilliseconds(500);
			OnPress.Invoke();
		}
	}
}