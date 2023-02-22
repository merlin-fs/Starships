using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;

namespace Editor.Grids
{
	static class GridRenderer
	{
        const HideFlags SCENE_OBJECT_HIDE_FLAGS = HideFlags.None;// HideFlags.HideAndDontSave;

		//const string k_PreviewObjectName = "ProGridsGridObject";

		const string MATERIAL_OBJECT_NAME = "Editor+8b6ad2eb-aa3f-4f18-ae2d-f289932e3e33";
		const string MESH_OBJECT_NAME = "Editor+134da923-cff2-4dda-a804-b927ccac4459";
		const string GRID_SHADER = "Hidden/Editor/GridShader";
		const int MAX_LINES = 256;
        const int PRIMARY_COLOR_INCREMENT = 10;

        static Mesh GridMesh;
		static Material GridMaterial;
		static int TangentIteration, BitangentIteration, MaxLines = MAX_LINES, GridResolution = 1;

		static float AlphaBump;
		internal static Color gridColorX;
		internal static Color gridColorY;
		internal static Color gridColorZ;

		public static int majorLineIncrement = 10;

		public static bool Init()
		{
			//!majorLineIncrement = EditorPrefs.GetInt(PreferenceKeys.MajorLineIncrement, 10);

			if(majorLineIncrement < 2)
				majorLineIncrement = 2;

			if (GridMesh == null)
			{
				GridMesh = Resources.FindObjectsOfTypeAll<Mesh>().FirstOrDefault(x => x.name.Equals(MESH_OBJECT_NAME));

				if (GridMesh == null)
				{
					GridMesh = new Mesh();
					GridMesh.name = MESH_OBJECT_NAME;
					GridMesh.hideFlags = SCENE_OBJECT_HIDE_FLAGS;
				}
			}

			GridMesh.hideFlags = SCENE_OBJECT_HIDE_FLAGS;

			if (GridMaterial == null)
			{
				GridMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(x => x.name.Equals(MATERIAL_OBJECT_NAME));

				if (GridMaterial == null)
				{
					var shader = Shader.Find(GRID_SHADER);

					if (shader != null)
					{
						GridMaterial = new Material(shader);
						GridMaterial.name = MATERIAL_OBJECT_NAME;
						GridMaterial.hideFlags = SCENE_OBJECT_HIDE_FLAGS;
					}
				}
			}

			LoadPreferences();

			return GridMesh != null || GridMaterial != null;
		}

		public static void Destroy()
		{
			if (GridMesh)
				Object.DestroyImmediate(GridMesh);

			if(GridMaterial)
				Object.DestroyImmediate(GridMaterial);
		}

		internal static void LoadPreferences()
		{
			//AlphaBump = EditorPrefs.GetFloat(PreferenceKeys.AlphaBump, Defaults.AlphaBump);
			//gridColorX = EditorUtility.GetColorFromJson(EditorPrefs.GetString(PreferenceKeys.GridColorX), Defaults.GridColorX);
			//gridColorY = EditorUtility.GetColorFromJson(EditorPrefs.GetString(PreferenceKeys.GridColorY), Defaults.GridColorY);
			//gridColorZ = EditorUtility.GetColorFromJson(EditorPrefs.GetString(PreferenceKeys.GridColorZ), Defaults.GridColorZ);
            AlphaBump = Defaults.AlphaBump;
            gridColorX = Defaults.GridColorX;
            gridColorY = Defaults.GridColorY;
            gridColorZ = Defaults.GridColorZ;
        }

        internal static void Repaint()
		{
			if (GridMesh != null && GridMaterial != null)
			{
				GridMaterial.SetPass(0);
				Graphics.DrawMeshNow(GridMesh, Matrix4x4.identity);
			}
		}

		internal static float SetPerspective(Camera cam, Axis axis, float snapValue, Vector3 position, float offset)
		{
			if ((axis & Axis.X) == Axis.X)
				return GridRenderer.SetPerspective(cam, position + Vector3.right * offset, Vector3.up, Vector3.forward, snapValue, gridColorX);

			if ((axis & Axis.Y) == Axis.Y)
				return GridRenderer.SetPerspective(cam, position + Vector3.up * offset, Vector3.right, Vector3.forward, snapValue, gridColorY);

			if ((axis & Axis.Z) == Axis.Z)
				return GridRenderer.SetPerspective(cam, position + Vector3.forward * offset, Vector3.up, Vector3.right, snapValue, gridColorZ);

			return 0f;
		}


		internal static float SetPerspective(Camera cam, Vector3 pivot, Vector3 tangent, Vector3 bitangent, float snapValue, Color color)
		{
			if(!GridMesh || !GridMaterial)
				Init();

			GridMaterial.SetFloat("_AlphaCutoff", .1f);
			GridMaterial.SetFloat("_AlphaFade", .6f);

			pivot = Snapping.Round(pivot, snapValue);

			Vector3 p = cam.WorldToViewportPoint(pivot);
			bool inFrustum = (p.x >= 0f && p.x <= 1f) &&
							 (p.y >= 0f && p.y <= 1f) &&
							  p.z >= 0f;

			float[] distances = GetDistanceToFrustumPlanes(cam, pivot, tangent, bitangent, 24f);

			if(inFrustum)
			{
				TangentIteration = (int)(Mathf.Ceil( (Mathf.Abs(distances[0]) + Mathf.Abs(distances[2]))/snapValue ));
				BitangentIteration = (int)(Mathf.Ceil( (Mathf.Abs(distances[1]) + Mathf.Abs(distances[3]))/snapValue ));

				MaxLines = Mathf.Max(TangentIteration, BitangentIteration );

				// if the s_MaxLines is around 3x greater than min, we're probably skewing the camera at near-plane
				// angle, so use the min instead.
				if(MaxLines > Mathf.Min(TangentIteration, BitangentIteration) * 2)
					MaxLines = (int) Mathf.Min(TangentIteration, BitangentIteration) * 2;

				GridResolution = 1;

				float dot = Vector3.Dot( cam.transform.position-pivot, Vector3.Cross(tangent, bitangent) );

				if(MaxLines > MAX_LINES)
				{
					if(Vector3.Distance(cam.transform.position, pivot) > 50f * snapValue && Mathf.Abs(dot) > .8f)
					{
						while(MaxLines/GridResolution > MAX_LINES)
							GridResolution += GridResolution;
					}
					else
					{
						MaxLines = MAX_LINES;
					}
				}
			}

			// origin, tan, bitan, increment, iterations, divOffset, color, primary alpha bump
			RebuildPlane(cam, pivot, tangent, bitangent, snapValue * GridResolution, MAX_LINES / GridResolution, GridResolution, color);

			return snapValue * GridResolution * (MAX_LINES / GridResolution);
		}

		internal static void SetPerspective3D(Camera cam, Vector3 pivot, float snapValue)
		{
			if(!GridMesh || !GridMaterial)
				Init();

			GridMaterial.SetFloat("_AlphaCutoff", 0f);
			GridMaterial.SetFloat("_AlphaFade", 0f);

			Vector3 camDir = (pivot - cam.transform.position).normalized;
			pivot = Snapping.Round(pivot, snapValue);

			// Used to flip the grid to match whatever direction the cam is currently
			// coming at the pivot from
			Vector3 right = camDir.x < 0f ? Vector3.right : Vector3.right * -1f;
			Vector3 up = camDir.y < 0f ? Vector3.up : Vector3.up * -1f;
			Vector3 forward = camDir.z < 0f ? Vector3.forward : Vector3.forward * -1f;

			// Get intersecting point for each axis, if it exists
			Ray ray_x = new Ray(pivot, right);
			Ray ray_y = new Ray(pivot, up);
			Ray ray_z = new Ray(pivot, forward);

			float x_dist = 10f, y_dist = 10f, z_dist = 10f;
			bool x_intersect = false, y_intersect = false, z_intersect = false;

			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);

			foreach(Plane p in planes)
			{
				float dist;
				float t = 0;

				if(p.Raycast(ray_x, out dist))
				{
					t = Vector3.Distance(pivot, ray_x.GetPoint(dist));
					if(t < x_dist || !x_intersect)
					{
						x_intersect = true;
						x_dist = t;
					}
				}

				if(p.Raycast(ray_y, out dist))
				{
					t = Vector3.Distance(pivot, ray_y.GetPoint(dist));
					if(t < y_dist || !y_intersect)
					{
						y_intersect = true;
						y_dist = t;
					}
				}

				if(p.Raycast(ray_z, out dist))
				{
					t = Vector3.Distance(pivot, ray_z.GetPoint(dist));
					if(t < z_dist || !z_intersect)
					{
						z_intersect = true;
						z_dist = t;
					}
				}
			}

			int x_iter = (int)(Mathf.Ceil(Mathf.Max(x_dist, y_dist))/snapValue);
			int y_iter = (int)(Mathf.Ceil(Mathf.Max(x_dist, z_dist))/snapValue);
			int z_iter = (int)(Mathf.Ceil(Mathf.Max(z_dist, y_dist))/snapValue);

			int max = Mathf.Max( Mathf.Max(x_iter, y_iter), z_iter );
			int div = 1;
			while (max / div > MAX_LINES)
			{
				div++;
			}

			Vector3[] vertices_t = null;
			Vector3[] normals_t = null;
			Color[] colors_t = null;
			int[] indices_t = null;

			List<Vector3> vertices_m = new List<Vector3>();
			List<Vector3> normals_m = new List<Vector3>();
			List<Color> colors_m = new List<Color>();
			List<int> indices_m = new List<int>();

			// X plane
			RebuildPerspectivePlane(cam, pivot, up, forward, snapValue*div, x_iter/div, gridColorX, out vertices_t, out normals_t, out colors_t, out indices_t, 0);
			vertices_m.AddRange(vertices_t);
			normals_m.AddRange(normals_t);
			colors_m.AddRange(colors_t);
			indices_m.AddRange(indices_t);

			// Y plane
			RebuildPerspectivePlane(cam, pivot, right, forward, snapValue*div, y_iter/div, gridColorY, out vertices_t, out normals_t, out colors_t, out indices_t, vertices_m.Count);
			vertices_m.AddRange(vertices_t);
			normals_m.AddRange(normals_t);
			colors_m.AddRange(colors_t);
			indices_m.AddRange(indices_t);

			// Z plane
			RebuildPerspectivePlane(cam, pivot, right, up, snapValue*div, z_iter/div, gridColorZ, out vertices_t, out normals_t, out colors_t, out indices_t, vertices_m.Count);
			vertices_m.AddRange(vertices_t);
			normals_m.AddRange(normals_t);
			colors_m.AddRange(colors_t);
			indices_m.AddRange(indices_t);

			GridMesh.Clear();
			GridMesh.vertices = vertices_m.ToArray();
			GridMesh.normals = normals_m.ToArray();
			GridMesh.subMeshCount = 1;
			GridMesh.uv = new Vector2[vertices_m.Count];
			GridMesh.colors = colors_m.ToArray();
			GridMesh.SetIndices(indices_m.ToArray(), MeshTopology.Lines, 0);
		}

        public static float ValueFromMask(Vector3 val, Vector3 mask)
        {
            if (Mathf.Abs(mask.x) > .0001f)
                return val.x;
            else if (Mathf.Abs(mask.y) > .0001f)
                return val.y;
            else
                return val.z;
        }

        internal static void DrawOrthographic(Camera cam, Axis camAxis, float snapValue, float angle)
		{
			switch (camAxis)
			{
				case Axis.X:
					DrawOrthographic(cam, camAxis, gridColorX, snapValue, angle);
					break;

				case Axis.Y:
					DrawOrthographic(cam, camAxis, gridColorY, snapValue, angle);
					break;

				case Axis.Z:
					DrawOrthographic(cam, camAxis, gridColorZ, snapValue, angle);
					break;
			}
		}

		static void RebuildPerspectivePlane(Camera cam, Vector3 pivot, Vector3 tan, Vector3 bitan, float increment, int iterations, Color secondary,
			out Vector3[] vertices,
			out Vector3[] normals,
			out Color[] colors,
			out int[] indices, int offset)
		{
			Color primary = secondary;
			primary.a += AlphaBump;

			float len = increment * iterations;

			int highlightOffsetTan 		= (int)(ValueFromMask(pivot, tan) % (increment * majorLineIncrement) / increment);
			int highlightOffsetBitan	= (int)(ValueFromMask(pivot, bitan) % (increment * majorLineIncrement) / increment);

			iterations++;

			// this could only use 3 verts per line
			float fade = .75f;
			float fadeDist = len * fade;
			Vector3 nrm = Vector3.Cross(tan, bitan);

			vertices = new Vector3[iterations*6-3];
			normals = new Vector3[iterations*6-3];
			indices = new int[iterations*8-4];
			colors = new Color[iterations*6-3];

			vertices[0] = pivot;
			vertices[1] = (pivot + bitan*fadeDist);
			vertices[2] = (pivot + bitan*len);

			normals[0] = nrm;
			normals[1] = nrm;
			normals[2] = nrm;

			indices[0] = 0 + offset;
			indices[1] = 1 + offset;
			indices[2] = 1 + offset;
			indices[3] = 2 + offset;

			colors[0] = primary;
			colors[1] = primary;
			colors[2] = primary;
			colors[2].a = 0f;


			int n = 4;
			int v = 3;

			for(int i = 1; i < iterations; i++)
			{
				// MeshTopology doesn't exist prior to Unity 4
				vertices[v+0] = pivot + i * tan * increment;
				vertices[v+1] = (pivot + bitan*fadeDist) + i * tan * increment;
				vertices[v+2] = (pivot + bitan*len) + i * tan * increment;

				vertices[v+3] = pivot + i * bitan * increment;
				vertices[v+4] = (pivot + tan*fadeDist) + i * bitan * increment;
				vertices[v+5] = (pivot + tan*len) + i * bitan * increment;

				normals[v+0] = nrm;
				normals[v+1] = nrm;
				normals[v+2] = nrm;
				normals[v+3] = nrm;
				normals[v+4] = nrm;
				normals[v+5] = nrm;

				indices[n+0] = v + 0 + offset;
				indices[n+1] = v + 1 + offset;
				indices[n+2] = v + 1 + offset;
				indices[n+3] = v + 2 + offset;
				indices[n+4] = v + 3 + offset;
				indices[n+5] = v + 4 + offset;
				indices[n+6] = v + 4 + offset;
				indices[n+7] = v + 5 + offset;

				float alpha = (i/(float)iterations);
				alpha = alpha < fade ? 1f : 1f - ( (alpha-fade)/(1-fade) );

				Color col = (i+highlightOffsetTan) % majorLineIncrement == 0 ? primary : secondary;
				col.a *= alpha;

				colors[v+0] = col;
				colors[v+1] = col;
				colors[v+2] = col;
				colors[v+2].a = 0f;

				col = (i+highlightOffsetBitan) % majorLineIncrement == 0 ? primary : secondary;
				col.a *= alpha;

				colors[v+3] = col;
				colors[v+4] = col;
				colors[v+5] = col;
				colors[v+5].a = 0f;

				n += 8;
				v += 6;
			}
		}

		/// <summary>
		/// Draws a plane grid using pivot point, the right and forward directions, and how far each direction should extend
		/// </summary>
		/// <param name="cam">Camera to use for reference</param>
		/// <param name="pivot">Pivot to use</param>
		/// <param name="tan">Tangent</param>
		/// <param name="bitan">Bitangent</param>
		/// <param name="increment">Increment</param>
		/// <param name="iterations">Iterations</param>
		/// <param name="div">Not used</param>
		/// <param name="secondary">Color</param>
		static void RebuildPlane(Camera cam, Vector3 pivot, Vector3 tan, Vector3 bitan, float increment, int iterations, int div, Color secondary)
		{
			Color primary = secondary;
			primary.a += AlphaBump;

			float len = iterations * increment;

			iterations++;

			Vector3 start = pivot - tan*(len/2f) - bitan*(len/2f);
			start = Snapping.Round(start, bitan+tan, increment);

			float inc = increment;
			int highlightOffsetTan = (int)(ValueFromMask(start, tan) % (inc*majorLineIncrement) / inc);
			int highlightOffsetBitan = (int)(ValueFromMask(start, bitan) % (inc*majorLineIncrement) / inc);

			Vector3[] lines = new Vector3[iterations * 4];
			int[] indices = new int[iterations * 4];
			Color[] colors = new Color[iterations * 4];

			int v = 0, t = 0;

			for(int i = 0; i < iterations; i++)
			{
				Vector3 a = start + tan * i * increment;
				Vector3 b = start + bitan * i * increment;

				lines[v+0] = a;
				lines[v+1] = a + bitan * len;

				lines[v+2] = b;
				lines[v+3] = b + tan * len;

				indices[t++] = v;
				indices[t++] = v+1;
				indices[t++] = v+2;
				indices[t++] = v+3;

				Color col = (i + highlightOffsetTan) % majorLineIncrement == 0 ? primary : secondary;

				// tan
				colors[v+0] = col;
				colors[v+1] = col;

				col = (i + highlightOffsetBitan) % majorLineIncrement == 0 ? primary : secondary;

				// bitan
				colors[v+2] = col;
				colors[v+3] = col;

				v += 4;
			}

			Vector3 nrm = Vector3.Cross(tan, bitan);
			Vector3[] nrms = new Vector3[lines.Length];
			for(int i = 0; i < lines.Length; i++)
				nrms[i] = nrm;


			GridMesh.Clear();
			GridMesh.vertices = lines;
			GridMesh.normals = nrms;
			GridMesh.subMeshCount = 1;
			GridMesh.uv = new Vector2[lines.Length];
			GridMesh.colors = colors;
			GridMesh.SetIndices(indices, MeshTopology.Lines, 0);
		}

		/// <summary>
		/// Returns the distance from the pivot to the frustum plane in the order of float[] { tan, bitan, -tan, -bitan }
		/// </summary>
		/// <param name="cam">Camera to use as reference</param>
		/// <param name="pivot">Pivot point to use as reference</param>
		/// <param name="tan">Tangent</param>
		/// <param name="bitan">Bitangent</param>
		/// <param name="minDist">Minimum distance</paramz>
		/// <returns>The distance from the pivot to the frustrum plane</returns>
		static float[] GetDistanceToFrustumPlanes(Camera cam, Vector3 pivot, Vector3 tan, Vector3 bitan, float minDist)
		{
			Ray[] rays = new Ray[4]
			{
				new Ray(pivot, tan),
				new Ray(pivot, bitan),
				new Ray(pivot, -tan),
				new Ray(pivot, -bitan)
			 };

			float[] intersects = new float[4] { minDist, minDist, minDist, minDist };
			bool[] intersection_found = new bool[4] { false, false, false, false };

			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
			foreach(Plane p in planes)
			{
				float dist;
				float t = 0;

				for(int i = 0; i < 4; i++)
				{
					if(p.Raycast(rays[i], out dist))
					{
						t = Vector3.Distance(pivot, rays[i].GetPoint(dist));

						if(t < intersects[i] || !intersection_found[i])
						{
							intersection_found[i] = true;
							intersects[i] = Mathf.Max(minDist, t);
						}
					}
				}
			}
			return intersects;
		}

		static void DrawOrthographic(Camera camera, Axis camAxis, Color color, float snapValue, float angle)
		{
            //Color previousColor = Handles.color;
            //Debug.DrawLine
            Color previousColor = Color.black;

            Color primaryColor = new Color(color.r, color.g, color.b, color.a + AlphaBump);

			Vector3 bottomLeft = Snapping.Floor(camera.ScreenToWorldPoint(Vector2.zero), snapValue);
			Vector3 bottomRight =
				Snapping.Floor(camera.ScreenToWorldPoint(new Vector2(camera.pixelWidth, 0f)), snapValue);
			Vector3 topLeft = Snapping.Floor(camera.ScreenToWorldPoint(new Vector2(0f, camera.pixelHeight)), snapValue);
			Vector3 topRight =
				Snapping.Floor(camera.ScreenToWorldPoint(new Vector2(camera.pixelWidth, camera.pixelHeight)),
					snapValue);

			Vector3 axis = EnumExtension.VectorWithAxis(camAxis);

			float width = Vector3.Distance(bottomLeft, bottomRight);
			float height = Vector3.Distance(bottomRight, topRight);

			// Shift lines to 10m forward of the camera
			var camTrs = camera.transform;
			var camForward = camTrs.forward;
			var camRight = camTrs.right;
			var camUp = camTrs.up;

			bottomLeft += axis * Mathf.Sign(Vector3.Dot(camForward, axis)) * 10f;
			topRight += axis * Mathf.Sign(Vector3.Dot(camForward, axis)) * 10f;
			bottomRight += axis * Mathf.Sign(Vector3.Dot(camForward, axis)) * 10f;
			topLeft += axis * Mathf.Sign(Vector3.Dot(camForward, axis)) * 10f;

			// Draw Vertical Lines

			float snapValueAtResolution = snapValue;

			int segs = (int) Mathf.Ceil(width / snapValueAtResolution) + 2;

			float n = 2f;

			while (segs > MAX_LINES)
			{
				snapValueAtResolution = snapValueAtResolution * n;
				segs = (int) Mathf.Ceil(width / snapValueAtResolution) + 2;
				n++;
			}

			// Screen start and end
			Vector3 bl = (camRight.x + camRight.y + camRight.z) > 0
				? Snapping.Floor(bottomLeft, camRight, snapValueAtResolution * PRIMARY_COLOR_INCREMENT)
				: Snapping.Ceil(bottomLeft, camRight, snapValueAtResolution * PRIMARY_COLOR_INCREMENT);
			Vector3 start = bl - camUp * (height + snapValueAtResolution * 2);
			Vector3 end = bl + camUp * (height + snapValueAtResolution * 2);

			segs += PRIMARY_COLOR_INCREMENT;

			// The current line start and end
			Vector3 lineStart;
			Vector3 lineEnd;

			for (int i = -1; i < segs; i++)
			{
				lineStart = start + (i * (camRight * snapValueAtResolution));
				lineEnd = end + (i * (camRight * snapValueAtResolution));

                //Handles.color = i % PRIMARY_COLOR_INCREMENT == 0 ? primaryColor : color;
                //Handles.DrawLine(lineStart, lineEnd);
                var lineColor = i % PRIMARY_COLOR_INCREMENT == 0 ? primaryColor : color;
                Debug.DrawLine(lineStart, lineEnd, lineColor);

            }

            // Draw Horizontal Lines
            segs = (int) Mathf.Ceil(height / snapValueAtResolution) + 2;

			n = 2;
			while (segs > MAX_LINES)
			{
				snapValueAtResolution = snapValueAtResolution * n;
				segs = (int) Mathf.Ceil(height / snapValueAtResolution) + 2;
				n++;
			}

			Vector3 tl = (camUp.x + camUp.y + camUp.z) > 0
				? Snapping.Ceil(topLeft, camUp, snapValueAtResolution * PRIMARY_COLOR_INCREMENT)
				: Snapping.Floor(topLeft, camUp, snapValueAtResolution * PRIMARY_COLOR_INCREMENT);
			start = tl - camRight * (width + snapValueAtResolution * 2);
			end = tl + camRight * (width + snapValueAtResolution * 2);

			segs += (int) PRIMARY_COLOR_INCREMENT;

			for (int i = -1; i < segs; i++)
			{
				lineStart = start + (i * (-camUp * snapValueAtResolution));
				lineEnd = end + (i * (-camUp * snapValueAtResolution));
                //Handles.color = i % PRIMARY_COLOR_INCREMENT == 0 ? primaryColor : color;
                //Handles.DrawLine(lineStart, lineEnd);
                var lineColor = i % PRIMARY_COLOR_INCREMENT == 0 ? primaryColor : color;
                Debug.DrawLine(lineStart, lineEnd, lineColor);

            }

            if (angle > 0f)
			{
				Vector3 cen = Snapping.Round(((topRight + bottomLeft) / 2f), snapValue);

				float half = (width > height) ? width : height;

				float opposite = Mathf.Tan(Mathf.Deg2Rad * angle) * half;

				Vector3 up = camera.transform.up * opposite;
				Vector3 right = camera.transform.right * half;

				Vector3 bottomLeftAngle = cen - (up + right);
				Vector3 topRightAngle = cen + (up + right);

				Vector3 bottomRightAngle = cen + (right - up);
				Vector3 topLeftAngle = cen + (up - right);

                //Handles.color = primaryColor;
                //// y = 1x+1
                //Handles.DrawLine(bottomLeftAngle, topRightAngle);
                var lineColor = primaryColor;
                Debug.DrawLine(bottomLeftAngle, topRightAngle, lineColor);
                //// y = -1x-1
                //Handles.DrawLine(topLeftAngle, bottomRightAngle);
                Debug.DrawLine(topLeftAngle, bottomRightAngle, lineColor);
            }
			//Handles.color = previousColor;
		}
	}
}
