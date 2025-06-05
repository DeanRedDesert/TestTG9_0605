using System;
using UnityEngine;

namespace Midas.Presentation.Editor.General
{
	public sealed class ChangeGuiColorScope : IDisposable
	{
		private bool disposeValue;

		private readonly Color color;

		public ChangeGuiColorScope(Color color)
		{
			this.color = GUI.color;
			GUI.color = color;
		}

		~ChangeGuiColorScope()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposeValue)
			{
				if (disposing)
				{
					GUI.color = color;
				}

				disposeValue = true;
			}
		}
	}
}