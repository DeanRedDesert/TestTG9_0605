using System;
using System.Collections.Generic;
using System.Linq;
using Midas.Presentation.Data;

namespace Midas.Presentation.Lights
{
	public class LightsDetails
	{
		public string Name { get; }
		public LightsHandle Handle { get; }
		public int Priority { get; }

		public LightsDetails(LightsBase lights)
		{
			Name = lights.Name;
			Handle = lights.Register();
			Priority = lights.Priority;
		}
	}

	public sealed class LightsStatus : StatusBlock
	{
		private StatusProperty<IReadOnlyList<LightsDetails>> registeredLights;
		private StatusProperty<IReadOnlyList<LightsDetails>> activeLights;
		private StatusProperty<LightsDetails> activeOneShot;

		public IReadOnlyList<LightsDetails> RegisteredLights => registeredLights.Value;

		public IReadOnlyList<LightsDetails> ActiveLights
		{
			get => activeLights.Value;
			set => activeLights.Value = value.ToArray();
		}

		public LightsDetails ActiveOneShot
		{
			get => activeOneShot.Value;
			set => activeOneShot.Value = value;
		}

		public LightsStatus() : base(nameof(LightsStatus))
		{
		}

		public LightsDetails RegisterLights(LightsBase lights)
		{
			var details = new LightsDetails(lights);
			registeredLights.Value = RegisteredLights.Append(details).ToArray();
			return details;
		}

		protected override void DoResetProperties()
		{
			base.DoResetProperties();

			registeredLights = AddProperty<IReadOnlyList<LightsDetails>>(nameof(RegisteredLights), Array.Empty<LightsDetails>());
			activeLights = AddProperty<IReadOnlyList<LightsDetails>>(nameof(ActiveLights), Array.Empty<LightsDetails>());
			activeOneShot = AddProperty(nameof(ActiveOneShot), default(LightsDetails));
		}
	}
}