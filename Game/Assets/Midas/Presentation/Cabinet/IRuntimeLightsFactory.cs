using System;
using System.Collections.Generic;
using UnityEngine;

namespace Midas.Presentation.Cabinet
{
	public interface IRuntimeLightsFactory
	{
		int LightCount { get; }

		void AddFrame(IReadOnlyList<Color?> frameData, TimeSpan displayTime);
		void AddAllLightsFrame(Color color, TimeSpan displayTime);
		void AddFacingLightsFrame(Color color, TimeSpan displayTime);
		void AddBackingLightsFrame(Color color, TimeSpan displayTime);
	}

	public static class RumtimeLightsFactoryExtensionMethods
	{
		private static readonly TimeSpan shortFrame = TimeSpan.FromMilliseconds(33);
		public static void AddFrame(this IRuntimeLightsFactory factory, IReadOnlyList<Color?> frameData) => factory.AddFrame(frameData, shortFrame);
		public static void AddAllLightsFrame(this IRuntimeLightsFactory factory, Color color) => factory.AddAllLightsFrame(color, shortFrame);
		public static void AddFacingLightsFrame(this IRuntimeLightsFactory factory, Color color) => factory.AddFacingLightsFrame(color, shortFrame);
		public static void AddBackingLightsFrame(this IRuntimeLightsFactory factory, Color color) => factory.AddBackingLightsFrame(color, shortFrame);
	}

	public interface IStreamingLights
	{
		string ChoreographyId { get; }
		Color FallbackColor { get; }
	}

	public interface IRuntimeLights
	{
		string Name { get; }
		TimeSpan Duration { get; }
		Color FallbackColor { get; }
		void CreateSequence(IRuntimeLightsFactory factory);
	}
}