// Copyright (c) 2023 IGT

using UnityEngine;
using UnityEngine.UI;

namespace Midas.CreditPlayoff.Presentation
{
	public static class ColorExt
	{
		public static Color ChangeAlpha(this Color color, float alpha)
		{
			return new Color(color.r, color.g, color.b, alpha);
		}

		public static void SetAlpha(this Graphic graphic, float alpha)
		{
			graphic.color = graphic.color.ChangeAlpha(alpha);
		}

		public static void SetAlpha(this SpriteRenderer renderer, float alpha)
		{
			renderer.color = renderer.color.ChangeAlpha(alpha);
		}

		public static void SetAlpha(this SpriteRenderer[] renderers, float alpha)
		{
			foreach (var renderer in renderers)
			{
				renderer.color = renderer.color.ChangeAlpha(alpha);
			}
		}

		public static void SetAlpha(this MeshRenderer[] renderers, float alpha)
		{
			foreach (var renderer in renderers)
			{
				var materials = renderer.materials;
				foreach (var material in materials)
				{
					material.color = material.color.ChangeAlpha(alpha);
				}
			}
		}

		public static void SetAlpha(this MeshRenderer renderer, float alpha)
		{
			var materials = renderer.materials;
			for (var i = 0; i < renderer.materials.Length; ++i)
			{
				materials[i].color = materials[i].color.ChangeAlpha(alpha);
			}
		}

		public static void SetAlpha(this Material material, float alpha)
		{
			material.color = material.color.ChangeAlpha(alpha);
		}

		public static void SetAlpha(this TextMesh textMesh, float alpha)
		{
			textMesh.color = textMesh.color.ChangeAlpha(alpha);
		}
	}
}