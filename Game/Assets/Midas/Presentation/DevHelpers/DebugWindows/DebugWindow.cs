using System;
using UnityEngine;

namespace Midas.Presentation.DevHelpers.DebugWindows
{
	public abstract class DebugWindow : MonoBehaviour
	{
		#region Types

		[Flags]
		private enum ResizeMode
		{
			None = 0,
			Left = 1,
			Right = 2,
			Top = 4,
			Bottom = 8
		}

		#endregion

		#region Fields

		private const int ResizeArea = 20;
		private static int windowIds = 1;

		private int windowId;
		private ResizeMode resizeMode = ResizeMode.None;
		private float currentScaleFactor;
		private static GUIStyle horizontalLineStyle;

		#endregion

		#region Inspector Fields

		[SerializeField]
		private Font font = null;

		[SerializeField]
		private int fontSize = 32;

		[SerializeField]
		private Color textColor = new Color(0.9f, 0.9f, 0.9f);

		[SerializeField]
		private Color backgroundColor = new Color(0.3f, 0.0f, 0.1f);

		[SerializeField]
		private Rect windowRect;

		[SerializeField]
		private bool dragAble = true;

		[SerializeField]
		private bool resizeAble = true;

		[SerializeField]
		private string windowName = "";

		[SerializeField]
		private GUISkin guiSkin;

		#endregion

		#region Properties

		public string WindowName
		{
			get => windowName;
			set => windowName = value;
		}

		public bool DragAble
		{
			get => dragAble;
			set => dragAble = value;
		}

		public bool ResizeAble
		{
			get => resizeAble;
			set => resizeAble = value;
		}

		public Rect WindowRect
		{
			get => windowRect;
			set => windowRect = value;
		}

		public Color TextColor
		{
			get => textColor;
			set => textColor = value;
		}

		public Color BackgroundColor
		{
			get => backgroundColor;
			set => backgroundColor = value;
		}

		public Font Font
		{
			get => font;
			set => font = value;
		}

		public int FontSize
		{
			get => fontSize;
			set => fontSize = value;
		}

		protected GUIStyle ButtonStyle { get; private set; }

		protected GUIStyle HorizontalScrollbarStyle { get; private set; }

		protected GUIStyle VerticalScrollbarStyle { get; private set; }

		protected GUIStyle LabelStyle { get; private set; }

		protected GUIStyle ToggleStyle { get; private set; }

		protected GUIStyle TextFieldStyle { get; private set; }

		protected GUIStyle WindowStyle { get; private set; }

		#endregion

		protected abstract void RenderWindowContent();

		protected virtual void Awake()
		{
			windowId = windowIds++;
		}

		protected virtual void Reset()
		{
			enabled = false;
			font = Resources.Load<Font>("CONSOLA");
			guiSkin = Resources.Load<GUISkin>("DebugWindowGuiSkinNonTransparent");
		}

		protected static void HorizontalLine(Color color)
		{
			var c = GUI.color;
			GUI.color = color;
			GUILayout.Box(GUIContent.none, horizontalLineStyle);
			GUI.color = c;
		}

		protected static void CreateHorizontalLineStyle(Texture2D background)
		{
			horizontalLineStyle ??= new GUIStyle
			{
				normal =
				{
					background = background
				},
				margin = new RectOffset(0, 0, 8, 8),
				fixedHeight = 2
			};
		}

		protected virtual void CreateStyles()
		{
			if (guiSkin == null)
				guiSkin = GUI.skin;

			WindowStyle = new GUIStyle(guiSkin.window);
			ButtonStyle = new GUIStyle(guiSkin.button);
			LabelStyle = new GUIStyle(guiSkin.label);
			ToggleStyle = new GUIStyle(guiSkin.toggle);
			TextFieldStyle = new GUIStyle(guiSkin.textField);
			HorizontalScrollbarStyle = new GUIStyle(guiSkin.horizontalScrollbar);
			VerticalScrollbarStyle = new GUIStyle(guiSkin.verticalScrollbar);
			CreateHorizontalLineStyle(guiSkin.box.normal.background);
			WindowStyle.font = font;
			WindowStyle.fontSize = 28;
			ButtonStyle.font = font;
			ButtonStyle.fontSize = FontSize;
			LabelStyle.font = font;
			LabelStyle.fontSize = FontSize;
			ToggleStyle.font = font;
			ToggleStyle.fontSize = FontSize;
			TextFieldStyle.font = font;
			TextFieldStyle.fontSize = FontSize;
		}

		// GUI will appear on display 0
		[GUITarget(0)] //Display.main = Synonymous with Display.displays[0]
		private void OnGUI()
		{
			if (WindowStyle == null)
				CreateStyles();

			var tmpSkin = GUI.skin;
			var tmpMatrix = GUI.matrix;
			var tmpTextColor = GUI.contentColor;
			var tmpBackgroundColor = GUI.backgroundColor;

			GUI.contentColor = TextColor;
			GUI.backgroundColor = BackgroundColor;

			SetupScaling();

			if (resizeAble)
			{
				ResizeWindowRect();
			}

			GUI.skin = guiSkin;
			windowRect = GUI.Window(windowId, windowRect, RenderWindowContentPrivate, WindowName, WindowStyle);

			// GUI.skin = tmpSkin;
			// GUI.matrix = tmpMatrix;
			// GUI.contentColor = tmpTextColor;
			// GUI.backgroundColor = tmpBackgroundColor;
		}

		private void SetupScaling()
		{
			currentScaleFactor = 1.0f / (2160.0f / Screen.width);
			var scaleVector = new Vector3(currentScaleFactor, currentScaleFactor, 1);
			GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scaleVector);
		}

		private void RenderWindowContentPrivate(int id)
		{
			RenderCloseButton();

			RenderWindowContent();

			if (dragAble)
			{
				GUI.DragWindow(new Rect(ResizeArea, 0, windowRect.width - ResizeArea * 2, windowRect.height - ResizeArea));
			}
		}

		private void RenderCloseButton()
		{
			if (GUI.Button(new Rect(windowRect.width - GUI.skin.window.padding.right - GUI.skin.window.padding.top, 0, GUI.skin.window.padding.top, GUI.skin.window.padding.top),
					"x"))
			{
				enabled = false;
			}
		}

		private void ResizeWindowRect()
		{
			Event current = Event.current;
			if (current.displayIndex != 0) //Display.main = Synonymous with Display.displays[0]
			{
				return;
			}

			// if mouse is no longer dragging, stop tracking direction of drag
			if (current.type == EventType.MouseUp ||
				current.type == EventType.MouseLeaveWindow ||
				current.type == EventType.MouseEnterWindow)
			{
				resizeMode = ResizeMode.None;
			}
			else if (current.type == EventType.MouseDown)
			{
				resizeMode = FindResizeMode(current.mousePosition);
			}

			if (current.type == EventType.MouseDrag)
			{
				ResizeWindowRect(current.mousePosition);
			}
		}

		private void ResizeWindowRect(Vector2 mousePosition)
		{
			if (resizeMode.HasFlag(ResizeMode.Left))
			{
				windowRect.xMin = mousePosition.x;
			}

			if (resizeMode.HasFlag(ResizeMode.Right))
			{
				windowRect.xMax = mousePosition.x;
			}

			if (resizeMode.HasFlag(ResizeMode.Top))
			{
				windowRect.yMin = mousePosition.y;
			}

			if (resizeMode.HasFlag(ResizeMode.Bottom))
			{
				windowRect.yMax = mousePosition.y;
			}
		}

		private ResizeMode FindResizeMode(Vector2 mousePosition)
		{
			ResizeMode result = ResizeMode.None;
			// not in window rect
			if (mousePosition.x <= windowRect.xMin - ResizeArea ||
				mousePosition.x >= windowRect.xMax + ResizeArea ||
				mousePosition.y <= windowRect.yMin - ResizeArea ||
				mousePosition.y >= windowRect.yMax + ResizeArea)
			{
				return result;
			}

			// Left
			if (mousePosition.x > windowRect.xMin - ResizeArea &&
				mousePosition.x < windowRect.xMin + ResizeArea)
			{
				return ResizeMode.Left;
			}

			// Right
			if (mousePosition.x > windowRect.xMax - ResizeArea &&
				mousePosition.x < windowRect.xMax + ResizeArea)
			{
				result |= ResizeMode.Right;
			}

			// Top
			if (mousePosition.y > windowRect.yMin - ResizeArea &&
				mousePosition.y < windowRect.yMin)
			{
				return ResizeMode.Top;
			}

			// Bottom
			if (mousePosition.y > windowRect.yMax - ResizeArea &&
				mousePosition.y < windowRect.yMax + ResizeArea)
			{
				result |= ResizeMode.Bottom;
			}

			return result;
		}
	}
}