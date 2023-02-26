using System;
using UnityEngine;

namespace Game.Editor
{
	static class Snapping
	{
		const float EPSILON = .0001f;

		public static bool Approx(Vector3 lhs, Vector3 rhs)
		{
			return Mathf.Abs(lhs.x - rhs.x) < EPSILON && Mathf.Abs(lhs.y - rhs.y) < EPSILON && Mathf.Abs(lhs.z - rhs.z) < EPSILON;
		}

		internal static bool IsRounded(Vector3 v)
		{
			return (Mathf.Approximately(v.x, 1f) || Mathf.Approximately(v.y, 1f) || Mathf.Approximately(v.z, 1f)) || v == Vector3.zero;
		}

		public static Vector3 Ceil(Vector3 val, Vector3 mask, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				( Mathf.Abs(mask.x) < EPSILON ? _x : Ceil(_x, snapValue) ),
				( Mathf.Abs(mask.y) < EPSILON ? _y : Ceil(_y, snapValue) ),
				( Mathf.Abs(mask.z) < EPSILON ? _z : Ceil(_z, snapValue) )
			);
		}

		public static Vector3 Floor(Vector3 val, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				Floor(_x, snapValue),
				Floor(_y, snapValue),
				Floor(_z, snapValue)
			);
		}

		public static Vector3 Floor(Vector3 val, Vector3 mask, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				( Mathf.Abs(mask.x) < EPSILON ? _x : Floor(_x, snapValue) ),
				( Mathf.Abs(mask.y) < EPSILON ? _y : Floor(_y, snapValue) ),
				( Mathf.Abs(mask.z) < EPSILON ? _z : Floor(_z, snapValue) )
			);
		}

		public static float Snap(float val, float round)
		{
			return round * Mathf.Round(val / round);
		}

		public static float Floor(float val, float snapValue)
		{
			return snapValue * Mathf.Floor(val / snapValue);
		}

		public static float Ceil(float val, float snapValue)
		{
			return snapValue * Mathf.Ceil(val / snapValue);
		}

		public static Vector3 Round(Vector3 val, Vector3 mask, float snapValue)
		{

			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				( Mathf.Abs(mask.x) < EPSILON ? _x : Snap(_x, snapValue) ),
				( Mathf.Abs(mask.y) < EPSILON ? _y : Snap(_y, snapValue) ),
				( Mathf.Abs(mask.z) < EPSILON ? _z : Snap(_z, snapValue) )
			);
		}

		public static Vector3 Round(Vector3 val, float snapValue)
		{
			float _x = val.x, _y = val.y, _z = val.z;
			return new Vector3(
				Snap(_x, snapValue),
				Snap(_y, snapValue),
				Snap(_z, snapValue)
			);
		}

		public static Vector3 Sign(Vector3 v)
		{
			v.x = v.x < 0 ? -1 : 1;
			v.y = v.y < 0 ? -1 : 1;
			v.z = v.z < 0 ? -1 : 1;

			return v;
		}

	}
}
