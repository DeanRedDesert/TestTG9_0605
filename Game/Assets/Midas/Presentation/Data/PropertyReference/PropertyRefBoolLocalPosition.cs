using System;
using UnityEngine;

namespace Midas.Presentation.Data.PropertyReference
{
	public sealed class PropertyRefBoolLocalPosition : PropertyRefBool
	{
		[Serializable]
		private class PositionData
		{
			public Transform target;
			public Vector3 localPositionIfTrue;
			public Vector3 localPositionIfFalse;
		}

		[SerializeField]
		private PositionData[] positionData;

		protected override void Refresh(bool value)
		{
			if (positionData == null || positionData.Length == 0)
				return;

			if (value)
			{
				foreach (var d in positionData)
				{
					if (d.target)
						d.target.localPosition = d.localPositionIfTrue;
				}
			}
			else
			{
				foreach (var d in positionData)
				{
					if (d.target)
						d.target.localPosition = d.localPositionIfFalse;
				}
			}
		}
	}
}