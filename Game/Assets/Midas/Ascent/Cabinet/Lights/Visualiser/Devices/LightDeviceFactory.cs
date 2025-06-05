using System;
using System.Collections.Generic;
using IGT.Game.Core.Presentation.PeripheralLights;
using Midas.Ascent.Cabinet.Lights.Visualiser.Simulated;
using UnityEngine;
using Color = UnityEngine.Color;

namespace Midas.Ascent.Cabinet.Lights.Visualiser.Devices
{
	/// <summary>
	/// Cabinet LED definitions:
	///  Width - 64 leds
	///  Height - 19 leds
	/// </summary>
	public static class LightDeviceFactory
	{
		#region Public Methods

		public static IDictionary<StreamingLightHardware, GameObject> InstantiateDevices(Transform parent, Sprite ledSprite, Vector2 ledSize, params StreamingLightHardware[] lightDevices)
		{
			var result = new Dictionary<StreamingLightHardware, GameObject>();

			foreach (var streamingLightHardware in lightDevices)
			{
				var lightDevice = BuildLightDevice(parent, streamingLightHardware, ledSprite, ledSize);
				if (lightDevice == null)
					continue;

				result.Add(streamingLightHardware, lightDevice);
			}

			return result;
		}

		#endregion

		#region Private Methods

		private static GameObject BuildLightDevice(Component parent, StreamingLightHardware streamingLightHardware, Sprite ledSprite, Vector2 ledSize)
		{
			var schema = GetLedSchema(streamingLightHardware);
			if (schema == null)
				return null;

			var obj = SimulatedLightDevice.Construct(parent.transform, GetLightDevicePosition(streamingLightHardware), streamingLightHardware);

			BuildLights(obj.transform, schema, ledSprite, ledSize);

			return obj;
		}

		private static void BuildLights(Component parent, LedSchema ledSchema, Sprite ledSprite, Vector2 ledSize)
		{
			ledSchema.Reset();

			var i = 0;
			while (ledSchema.MoveNext())
			{
				var position = ledSchema.Current;
				var index = i++;
				var name = $"LED{index}";
				var gObj = new GameObject();
				gObj.name = name;
				gObj.layer = LayerMask.NameToLayer("UI");
				var sr = gObj.AddComponent<SpriteRenderer>();
				gObj.AddComponent<SimulatedLed>();
				sr.sprite = ledSprite;
				sr.drawMode = SpriteDrawMode.Tiled;
				sr.size = ledSize;
				sr.color = new Color(0f, 0f, 0f, 0.75f);

				gObj.transform.SetParent(parent.transform);
				gObj.transform.localPosition = new Vector3(position.x, position.y, 0f);
				gObj.transform.localRotation = Quaternion.AngleAxis(position.z, Vector3.forward);
			}
		}

		private static Vector3 GetLightDevicePosition(StreamingLightHardware lightDevice)
		{
			switch (lightDevice)
			{
				case StreamingLightHardware.GamesmanButtonLightRing: return new Vector3(25.625f, -7.05f, 0f);
				case StreamingLightHardware.AustraliaCrystalCoreTrim: return new Vector3(0f, -11f, 0f);
				case StreamingLightHardware.CrystalCore30EdgeLights: return Vector3.zero;
				case StreamingLightHardware.CrystalCoreGills: break;
				case StreamingLightHardware.StreamingDppLightRing: return new Vector3(11.5f, -10.75f, 0f);
				case StreamingLightHardware.GenericBacklitTopper:
				case StreamingLightHardware.VideoTopper: return new Vector3(2f, 45f, 0f);
				case StreamingLightHardware.AxxisVideoTopper: return new Vector3(2.5f, 45f, 0f);
				case StreamingLightHardware.CrystalCurveGills: return Vector3.zero;
				case StreamingLightHardware.CrystalCurveMonitorLights: return new Vector3(9f, 0f, 0f);
				case StreamingLightHardware.CrystalCore27EdgeLights: return new Vector3(18.5f, 38f, 0f);
				case StreamingLightHardware.Unknown: return new Vector3(2.5f, 45f, 0f);
				default: throw new NotSupportedException($"The StreamingLightHardware \"{lightDevice}\" is not supported yet.");
			}

			return Vector3.zero;
		}

		private static LedSchema GetLedSchema(StreamingLightHardware lightDevice)
		{
			var ledSchema = new LedSchema();

			switch (lightDevice)
			{
				case StreamingLightHardware.GamesmanButtonLightRing:
					ledSchema.Ellipse(0, 11, -90f);
					break;

				case StreamingLightHardware.AustraliaCrystalCoreTrim:
					ledSchema.LedStrip(0, 9, LedSchema.StripOrientation.BottomToTop);
					ledSchema.LedStrip(10, 19, LedSchema.StripOrientation.TopToBottom, 37f, 9f);
					break;

				case StreamingLightHardware.CrystalCore30EdgeLights:
					ledSchema.LedStrip(0, 43, LedSchema.StripOrientation.BottomToTop);
					ledSchema.LedStrip(44, 75, LedSchema.StripOrientation.LeftToRight, 3f, 43f);
					ledSchema.LedStrip(76, 119, LedSchema.StripOrientation.TopToBottom, 37f, 43f);
					break;

				case StreamingLightHardware.CrystalCoreGills:
					// First section (left)
					ledSchema.ShiftLeft(3f);
					ledSchema.AddUp(4);

					// Second section (left)
					ledSchema.ShiftDown(3f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddUp(6);

					// Third section (left)
					ledSchema.ShiftDown(5f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddUp(10);

					// Forth section (left)
					ledSchema.ShiftDown(9f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddUp(10);

					// Forth section (right)
					ledSchema.ShiftRight(37f);
					ledSchema.AddDown(10);

					// Third section (right)
					ledSchema.ShiftRight(1f);
					ledSchema.ShiftUp(9f);
					ledSchema.AddDown(10);

					// Second section (right)
					ledSchema.ShiftRight(1f);
					ledSchema.ShiftUp(5f);
					ledSchema.AddDown(6);

					// First section (right)
					ledSchema.ShiftRight(1f);
					ledSchema.ShiftUp(3f);
					ledSchema.AddDown(4);
					break;

				case StreamingLightHardware.StreamingDppLightRing:
					ledSchema.LedStrip(0, 5, LedSchema.StripOrientation.BottomToTop, angle: -20f);
					ledSchema.LedStrip(6, 7, LedSchema.StripOrientation.BottomToTop, -1.9f, 5.875f);
					ledSchema.LedStrip(8, 10, LedSchema.StripOrientation.BottomToTop, -1.465f, 8.1f, 40f);
					ledSchema.LedStrip(11, 23, LedSchema.StripOrientation.LeftToRight, 1.025f, 10.2f);
					ledSchema.LedStrip(24, 26, LedSchema.StripOrientation.TopToBottom, 14.21f, 9.88f, -60f);
					ledSchema.LedStrip(27, 30, LedSchema.StripOrientation.TopToBottom, 15.82f, 2.49f, 30f);
					ledSchema.LedStrip(31, 43, LedSchema.StripOrientation.RightToLeft, 13.14f, -0.8f);
					break;

				case StreamingLightHardware.GenericBacklitTopper:
				case StreamingLightHardware.VideoTopper:
					ledSchema.AddUp(20);
					ledSchema.ShiftRight(2f);
					ledSchema.AddRight(30);
					ledSchema.ShiftRight(2f);
					ledSchema.AddDown(20);
					break;

				case StreamingLightHardware.AxxisVideoTopper:
					// Left
					ledSchema.AddUp(19);
					// Top
					ledSchema.ShiftUp(1f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddRight(31);
					// Right
					ledSchema.ShiftRight(1f);
					ledSchema.ShiftDown(1f);
					ledSchema.AddDown(19);
					//Bottom
					ledSchema.ShiftLeft(1f);
					ledSchema.ShiftDown(1f);
					ledSchema.AddLeft(31);
					break;

				// Crystal Curve Gills
				case StreamingLightHardware.CrystalCurveGills:
					// First section (left)
					ledSchema.ShiftLeft(2f);
					ledSchema.AddUp(4);

					// Second section (left)
					ledSchema.ShiftDown(3f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddUp(6);

					// Third section (left)
					ledSchema.ShiftDown(5f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddUp(10);

					// Third section (right)
					ledSchema.ShiftRight(37f);
					ledSchema.AddDown(10);

					// Second section (right)
					ledSchema.ShiftRight(1f);
					ledSchema.ShiftUp(5f);
					ledSchema.AddDown(6);

					// First section (right)
					ledSchema.ShiftRight(1f);
					ledSchema.ShiftUp(3f);
					ledSchema.AddDown(4);
					break;

				// Crystal Curve Front and Back side lights
				case StreamingLightHardware.CrystalCurveMonitorLights:
					// Left front strip
					ledSchema.AddUp(38);

					// Right front strip
					ledSchema.ShiftRight(21f);
					ledSchema.AddDown(38);

					// Left back strip
					ledSchema.ShiftLeft(22f);
					ledSchema.AddUp(39);

					// Middle-top back strip
					ledSchema.ShiftRight(1f);
					ledSchema.AddRight(22);

					// Right back strip
					ledSchema.ShiftRight(1f);
					ledSchema.AddDown(39);
					break;

				case StreamingLightHardware.CrystalCore27EdgeLights:
					// Top-Display
				{
					// Front-Top + Corner
					ledSchema.AddRight(13);
					// Front-Right + Corner
					ledSchema.AddDown(15);
					// Front-Bottom
					ledSchema.AddLeft(13);

					// Back-Bottom + Corner
					ledSchema.ShiftUp(1f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddRight(12);

					// Back-Right + Corner
					ledSchema.AddUp(13);

					// Back-Top
					ledSchema.AddLeft(10);

					// Front-Bottom + Corner
					ledSchema.ShiftDown(14f);
					ledSchema.ShiftLeft(3f);
					ledSchema.AddLeft(10);
					ledSchema.ShiftDown(1f);
					ledSchema.ShiftUp(1f); // Shift the frame corner into itself
					ledSchema.AddLeft(2);

					// Front-Left + Corner
					ledSchema.AddUp(14);
					ledSchema.ShiftDown(1f);
					ledSchema.ShiftUp(1f); // Shift the frame corner into itself
					ledSchema.AddUp(2);

					// Front-Top
					ledSchema.AddRight(13);

					// Back-Top + Corner
					ledSchema.ShiftDown(1);
					ledSchema.ShiftLeft(1);
					ledSchema.AddLeft(12);

					// Back-Left + Corner
					ledSchema.AddDown(13);

					// Back-Bottom
					ledSchema.AddRight(10);
				}
					// Bottom-Display
				{
					ledSchema.ShiftDown(3f);
					ledSchema.ShiftRight(1f);
					// Front-Top + Corner
					ledSchema.AddRight(13);
					// Front-Right + Corner
					ledSchema.AddDown(15);
					// Front-Bottom
					ledSchema.AddLeft(13);

					// Back-Bottom + Corner
					ledSchema.ShiftUp(1f);
					ledSchema.ShiftRight(1f);
					ledSchema.AddRight(12);

					// Back-Right + Corner
					ledSchema.AddUp(13);

					// Back-Top
					ledSchema.AddLeft(10);

					// Front-Bottom + Corner
					ledSchema.ShiftDown(14f);
					ledSchema.ShiftLeft(3f);
					ledSchema.AddLeft(10);
					ledSchema.ShiftDown(1f);
					ledSchema.ShiftUp(1f); // Shift the frame corner into itself
					ledSchema.AddLeft(2);

					// Front-Left + Corner
					ledSchema.AddUp(14);
					ledSchema.ShiftDown(1f);
					ledSchema.ShiftUp(1f); // Shift the frame corner into itself
					ledSchema.AddUp(2);

					// Front-Top
					ledSchema.AddRight(13);

					// Back-Top + Corner
					ledSchema.ShiftDown(1);
					ledSchema.ShiftLeft(1);
					ledSchema.AddLeft(12);

					// Back-Left + Corner
					ledSchema.AddDown(13);

					// Back-Bottom
					ledSchema.AddRight(10);
				}
					break;

				default:
					return null;
			}

			return ledSchema;
		}

		#endregion
	}
}