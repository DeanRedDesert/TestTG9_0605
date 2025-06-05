using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.ButtonHandling;

namespace Midas.Presentation.DevHelpers
{
	public sealed class ButtonStatusView : TreeViewControl
	{
		protected override IEnumerable<object> GetChildren(object node)
		{
			switch (node)
			{
				case ButtonState buttonState:
					return ButtonManager.ButtonStates
						.Where(bs => bs.ButtonState == buttonState)
						.OrderBy(b => b.ButtonFunction.Name);
				case null:
					return new[] { (object)ButtonState.Enabled, ButtonState.DisabledShow, ButtonState.DisabledHide };
				default:
					return Enumerable.Empty<object>();
			}
		}

		protected override bool HasChildren(object node)
		{
			switch (node)
			{
				case ButtonState buttonState:
					return ButtonManager.ButtonStates
						.Any(bs => bs.ButtonState == buttonState);
				case null:
					return true;
			}

			return false;
		}

		protected override string GetValueAsString(object item)
		{
			switch (item)
			{
				case ButtonState buttonState:
					return buttonState.ToString();
				case ButtonStateData buttonStateData:
					return $"{buttonStateData.ButtonFunction.Name,35}: {buttonStateData.LightState.State,10}";
			}

			return string.Empty;
		}
	}
}