using System;
using UnityEngine;

namespace Midas.Presentation.Editor.Utilities
{
	public sealed class ChangeGuiBackgroundColorScope : IDisposable
	{
		#region Public

		public ChangeGuiBackgroundColorScope(Color color)
		{
			_color = GUI.backgroundColor;
			GUI.backgroundColor = color;
		}

		~ChangeGuiBackgroundColorScope()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region Private

		private void Dispose(bool disposing)
		{
			if (!_disposeValue)
			{
				if (disposing)
				{
					GUI.backgroundColor = _color;
				}

				_disposeValue = true;
			}
		}

		private bool _disposeValue;
		private readonly Color _color;

		#endregion
	}
}