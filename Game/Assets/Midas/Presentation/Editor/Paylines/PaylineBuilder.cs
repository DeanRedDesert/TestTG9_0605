using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Midas.Presentation.Editor.Paylines
{
	public static class PaylineBuilder
	{
		#region Payline Disk Defintions

		/// <summary>
		/// The number of half disk vertices
		/// </summary>
		private const uint MaxDiskVertices = 24;

		/// <summary>
		/// The number of half disk indices
		/// </summary>
		private const uint MaxDiskIndices = 18;

		/// <summary>
		/// The vertices for the left half-disk (Cap) for the end of the paylines
		/// </summary>
		private static readonly float[] leftDiskVertices =
		{
			0, 0, 0,
			0, 1, 0,
			-0.5f, 0.87f, 0,
			-0.87f, 0.5f, 0,
			-1, 0, 0,
			-0.87f, -0.5f, 0,
			-0.5f, -0.87f, 0,
			0, -1, 0
		};

		/// <summary>
		/// The vertices for the right half-disk (Cap) for the end of the paylines
		/// </summary>
		private static readonly float[] rightDiskVertices =
		{
			0, 0, 0,
			0, -1, 0,
			0.5f, -0.87f, 0,
			0.87f, -0.5f, 0,
			1, 0, 0,
			0.87f, 0.5f, 0,
			0.5f, 0.87f, 0,
			0, 1, 0
		};

		/// <summary>
		/// The disk indices for both the left and right half-disk (Cap) for the end of the paylines
		/// </summary>
		private static readonly int[] diskIndices =
		{
			0, 2, 1,
			0, 3, 2,
			0, 4, 3,
			0, 5, 4,
			0, 6, 5,
			0, 7, 6
		};

		#endregion

		#region Build Lines

		/// <summary>
		/// Builds the lines from a given list of positions.
		/// </summary>
		/// <param name="pos">A list of the position of the line</param>
		/// <param name="lineWidth">The current line width</param>
		/// <param name="cornerSegments">The number of segments between each line.</param>
		/// <param name="vertexColor">The color of each vertex.</param>
		/// <param name="closedLine">Should the line be closed or open ended.</param>
		public static Mesh BuildLine(IList<Vector3> pos, float lineWidth, int cornerSegments, Color vertexColor, bool closedLine)
		{
			var vertices = new List<Vector3>();
			var normals = new List<Vector3>();
			var indices = new List<int>();
			var texCoords = new List<Vector2>();

			var indexCount = 0;
			// Add the start cap to the new payline mesh
			if (closedLine)
			{
				// Add the 1st line back to complete the shape
				pos.Add(pos[0]);
				pos.Add(pos[1]);
			}
			else
			{
				AddPaylineCap(pos[0], leftDiskVertices, vertices, normals, indices, texCoords, lineWidth);
				indexCount = vertices.Count;
			}

			// Loop through all the positions and build the lines
			for (var posIndex = 0; posIndex < pos.Count; posIndex++)
			{
				if (posIndex + 2 < pos.Count)
				{
					// The 1st point in the payline this is the start of the 1st line
					var point1 = pos[posIndex];

					// The 2nd point in the payline this is the end of the 1st line and the start of the second line
					var point2 = pos[posIndex + 1];

					// The 3rd point in the payline this is the end of the 2nd line.
					var point3 = pos[posIndex + 2];

					var direction1 = point2 - point1;
					var angle1 = LineAngle(direction1) - 90;
					var direction2 = point3 - point2;
					var angle2 = LineAngle(direction2) - 90;

					var vertex1 = new Vector3(0, lineWidth); // Top left 1st
					var vertex2 = new Vector3(0, -lineWidth); // Bottom left 1st
					var vertex3 = new Vector3(0, lineWidth); // Top right 1st
					var vertex4 = new Vector3(0, -lineWidth); // Bottom right 1st
					var vertex5 = new Vector3(0, lineWidth);
					var vertex6 = new Vector3(0, -lineWidth);
					var vertex7 = new Vector3(0, lineWidth);
					var vertex8 = new Vector3(0, -lineWidth);

					vertex1 = Rotate(vertex1, angle1);
					vertex1 += point1;
					vertex2 = Rotate(vertex2, angle1);
					vertex2 += point1;

					vertex3 = Rotate(vertex3, angle1);
					vertex3 += point2;
					vertex4 = Rotate(vertex4, angle1);
					vertex4 += point2;

					vertex5 = Rotate(vertex5, angle2);
					vertex5 += point2;
					vertex6 = Rotate(vertex6, angle2);
					vertex6 += point2;

					vertex7 = Rotate(vertex7, angle2);
					vertex7 += point3;
					vertex8 = Rotate(vertex8, angle2);
					vertex8 += point3;

					// Get the offset vector for the vertices
					direction1.Normalize();
					direction2.Normalize();
					var angle = Mathf.Abs(Mathf.Deg2Rad * Vector3.Angle(direction1, direction2)) + 1.0f;
					direction1 = direction1 * lineWidth * angle;
					direction2 = direction2 * lineWidth * angle;

					// If we have a WinIndicator or payline pos not zero add the direction of vertex 1,2
					if (closedLine || posIndex != 0)
					{
						vertex1 = vertex1 + direction1;
						vertex2 = vertex2 + direction1;
					}

					vertex3 = vertex3 - direction1;
					vertex4 = vertex4 - direction1;
					vertex5 = vertex5 + direction2;
					vertex6 = vertex6 + direction2;
					vertex7 = vertex7 - direction2;
					vertex8 = vertex8 - direction2;

					// If its 1st position of the payline we need to build the start of the line for vertex 1 and 2
					// the rest of the lines will just build off of the previous vertices to make a continuous line.
					if (posIndex == 0)
					{
						BuildLineSegment(vertices, normals, texCoords, vertex1, vertex2);
					}

					// Build Line segment
					BuildLineSegment(vertices, normals, texCoords, vertex3, vertex4);
					BuildLineIndices(indices, ref indexCount);

					// Calculate the intersection points.
					var topIntersection = IntersectionPoint(vertex1, vertex3, vertex5, vertex7);
					var bottomIntersection = IntersectionPoint(vertex2, vertex4, vertex6, vertex8);

					// Calculate the bezier curve for between the lines segments.
					var bezierStep = 1.0f / cornerSegments;
					var step = bezierStep;
					var count = 0;
					while (count < cornerSegments)
					{
						var currentStep = 1 - step;
						var topX = currentStep * currentStep * vertex3.x + 2 * step * currentStep * topIntersection.x + step * step * vertex5.x;
						var topY = currentStep * currentStep * vertex3.y + 2 * step * currentStep * topIntersection.y + step * step * vertex5.y;
						var bottomX = currentStep * currentStep * vertex4.x + 2 * step * currentStep * bottomIntersection.x + step * step * vertex6.x;
						var bottomY = currentStep * currentStep * vertex4.y + 2 * step * currentStep * bottomIntersection.y + step * step * vertex6.y;

						// Build Line
						BuildLineSegment(vertices, normals, texCoords, new Vector3(topX, topY, vertex3.z), new Vector3(bottomX, bottomY, vertex4.z));
						BuildLineIndices(indices, ref indexCount);

						step += bezierStep;
						count++;
					}
				}
				else if (!closedLine && posIndex + 1 < pos.Count)
				{
					var point1 = pos[posIndex];
					var point2 = pos[posIndex + 1];

					var direction = point2 - point1;
					var angle = LineAngle(direction) - 90;

					var vertex3 = new Vector3(0, lineWidth); //top right 1st
					var vertex4 = new Vector3(0, -lineWidth); //bottom right 1st

					vertex3 = Rotate(vertex3, angle);
					vertex3 += point2;
					vertex4 = Rotate(vertex4, angle);
					vertex4 += point2;

					BuildLineSegment(vertices, normals, texCoords, vertex3, vertex4);
					BuildLineIndices(indices, ref indexCount);
				}
			}

			// Add the end cap to the new payline mesh
			if (closedLine)
			{
				MapWinIndicatorTexCoords(vertices, texCoords);
			}
			else
			{
				AddPaylineCap(pos[pos.Count - 1], rightDiskVertices, vertices, normals, indices, texCoords, lineWidth);
				MapPaylineTexCoords(vertices, texCoords);
			}

			return CreateMesh(vertices, normals, indices, texCoords, vertexColor);
		}

		private static Mesh CreateMesh(List<Vector3> vertices, List<Vector3> normals, List<int> indices, List<Vector2> texCoords, Color vertexColor)
		{
			var mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = indices.ToArray();
			mesh.normals = normals.ToArray();
			mesh.uv = texCoords.ToArray();
			mesh.colors = Enumerable.Repeat(vertexColor, vertices.Count).ToArray();

			return mesh;
		}

		/// <summary>
		/// Add a payline cap to the payline. Builds the verts, normals, indices, and texCoords for the payline cap
		/// </summary>
		/// <param name="pos">The position of the cap</param>
		/// <param name="diskVertices">An array of vertices for the cap</param>
		/// <param name="vertices">The vertex list</param>
		/// <param name="normals">The normal list</param>
		/// <param name="indices">The index list</param>
		/// <param name="texCoords">The texture coordinates list</param>
		/// <param name="lineWidth">The current line width</param>
		private static void AddPaylineCap(Vector3 pos, IList<float> diskVertices, ICollection<Vector3> vertices, ICollection<Vector3> normals, ICollection<int> indices, ICollection<Vector2> texCoords, float lineWidth)
		{
			var preVertCount = vertices.Count;

			for (var vertIndex = 0; vertIndex < MaxDiskVertices; vertIndex += 3)
			{
				var paylineVertex = new Vector3(diskVertices[vertIndex], diskVertices[vertIndex + 1], 0.0f);

				// Payline caps are created in unity space and later moved to world space. The U coord for
				// this texture will be mapped later, the V coord is moved from -1 -> 1 to 0 -> 1 and giving
				// it the appropriate UV mapping.
				var texCoord = new Vector2(0.0f, paylineVertex.y * 0.5f + 0.5f);
				normals.Add(paylineVertex);

				// Calculate the new vertices based on the line Width
				paylineVertex *= lineWidth;

				// Move to cap to the correct position
				paylineVertex += pos;
				texCoord.x = paylineVertex.x;
				texCoords.Add(texCoord);

				// Set the vertices
				vertices.Add(paylineVertex);
			}

			// Build the indices
			for (var indicesIndex = 0; indicesIndex < MaxDiskIndices; indicesIndex++)
			{
				indices.Add(diskIndices[indicesIndex] + preVertCount);
			}
		}

		/// <summary>
		/// Build the line's Vertices, Normals, and TexCords
		/// </summary>
		/// <param name="vertices">The vertex list.</param>
		/// <param name="normals">The normals list.</param>
		/// <param name="texCoords">The texture coords list.</param>
		/// <param name="vertex1">the 1st point</param>
		/// <param name="vertex2">the 2nd point</param>
		private static void BuildLineSegment(ICollection<Vector3> vertices, ICollection<Vector3> normals, ICollection<Vector2> texCoords, Vector3 vertex1, Vector3 vertex2)
		{
			// Build the Vertices
			vertices.Add(vertex1);
			vertices.Add(vertex2);

			// Normalize the line
			var normal = vertex1 - vertex2;
			normal.Normalize();

			// Build the Normals
			normals.Add(normal);
			normals.Add(-normal);

			// U coord is placeholder in case mapping functions skip anything.
			// V coord will always go from 0 -> 1 for the UV's to wrap properly.
			texCoords.Add(new Vector2(1.0f, 1.0f));
			texCoords.Add(new Vector2(0.0f, 0.0f));
		}

		/// <summary>
		/// Map the Payline UV's based on the total length of the payline
		/// </summary>
		/// <param name="vertices">The vertex list.</param>
		/// <param name="texCoords">The texture coords list.</param>
		private static void MapPaylineTexCoords(IList<Vector3> vertices, IList<Vector2> texCoords)
		{
			var minX = 999999f;
			var maxX = -999999f;

			for (var index = 0; index < vertices.Count; index++)
			{
				var v = vertices[index];
				if (v.x < minX)
					minX = v.x;
				if (v.x > maxX)
					maxX = v.x;
			}

			var maxRange = maxX - minX;
			// This will only happen if the payline has no length at all, or no vertices.
			if (maxRange <= 0.0f)
			{
				return;
			}

			// Map the UV from the min.x instead of zero based with a percentage of the whole payline
			for (var texIndex = 0; texIndex < texCoords.Count; texIndex++)
			{
				texCoords[texIndex] = new Vector2((vertices[texIndex].x - minX) / maxRange, texCoords[texIndex].y);
			}
		}

		/// <summary>
		/// Map the WinIndicators based on angles between the first vertex and the vertex to be textured.
		/// </summary>
		/// <param name="vertices">The vertex list.</param>
		/// <param name="texCoords">The texture coords list.</param>
		private static void MapWinIndicatorTexCoords(IList<Vector3> vertices, IList<Vector2> texCoords)
		{
			Vector3 min = new Vector2(999999f, 999999f);
			Vector3 max = new Vector2(-999999f, -999999f);

			for (var index = 0; index < vertices.Count; index++)
			{
				var v = vertices[index];
				if (v.x < min.x)
					min.x = v.x;
				if (v.x > max.x)
					max.x = v.x;
				if (v.y < min.y)
					min.y = v.y;
				if (v.y > max.y)
					max.y = v.y;
			}

			// ( Max + Min ) * 0.5f will result in the middle of the two points being added
			var midPoint = new Vector3((max.x + min.x) * 0.5f, (max.y + min.y) * 0.5f, (max.z + min.z) * 0.5f);

			// Start at Vert[1] instead of Vert[0] so there can be no overlapping angles
			var midToStart = new Vector3(vertices[1].x - midPoint.x, vertices[1].y - midPoint.y, vertices[1].z - midPoint.z);

			// Crossing the Z Axis with midToStart.normalized will give us a normal for half space test
			var planeNorm = Vector3.Cross(Vector3.forward, midToStart.normalized);

			// Snap the beginning texCoords to 0 and the ends to 1
			texCoords[0] = new Vector2(0, texCoords[0].y);
			texCoords[1] = new Vector2(0, texCoords[1].y);
			texCoords[texCoords.Count - 2] = new Vector2(1, texCoords[texCoords.Count - 2].y);
			texCoords[texCoords.Count - 1] = new Vector2(1, texCoords[texCoords.Count - 1].y);

			// Skip the vertices we just gave UVs manually
			for (var index = 2; index < texCoords.Count - 2; index++)
			{
				var midToNew = vertices[index] - midPoint;

				// Find angle from the first vertex and the currently iterated vertex in degrees.
				var angleStartToNew = Vector3.Angle(midToNew, midToStart);

				// Convert the angleStartToNew from degrees to radians.
				angleStartToNew = Mathf.PI * angleStartToNew / 180;

				// Half Space Test to determine if we've passed pi radians (180 degrees)
				if (Vector3.Dot(planeNorm, midToNew.normalized) < 0.0f)
				{
					angleStartToNew = 2.0f * Mathf.PI - angleStartToNew;
				}

				// Find the angle's percent of a full circle to assign it a UV
				angleStartToNew /= 2.0f * Mathf.PI;
				texCoords[index] = new Vector2(angleStartToNew, texCoords[index].y);
			}
		}

		/// <summary>
		/// Build the payline indices. The index changed based on the indices this is because we are building the next
		/// segment. So we need the indices to overlap.
		/// </summary>
		/// <param name="indices">The index list</param>
		/// <param name="index">The index to be assign to the indices</param>
		private static void BuildLineIndices(ICollection<int> indices, ref int index)
		{
			indices.Add(index + 0);
			indices.Add(index + 2);
			indices.Add(index + 1);
			indices.Add(index + 3);
			indices.Add(index + 1);
			indices.Add(index + 2);
			index += 2;
		}

		#endregion

		#region Helper functions

		/// <summary>
		/// Calculates the intersection point
		/// </summary>
		/// <param name="point1">Point 1</param>
		/// <param name="point2">Point 2</param>
		/// <param name="point3">Point 3</param>
		/// <param name="point4">Point 4</param>
		/// <returns>A vector with the intersection</returns>
		private static Vector3 IntersectionPoint(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4)
		{
			float intersectionX;
			float intersectionY;

			// Line 1
			var slope1 = point2.x - point1.x != 0 ? (point2.y - point1.y) / (point2.x - point1.x) : 0;
			var intercept1 = point1.y - slope1 * point1.x;

			// Line 2
			var slope2 = point4.x - point3.x != 0 ? (point4.y - point3.y) / (point4.x - point3.x) : 0;
			var intercept2 = point3.y - slope2 * point3.x;

			if (point4.x - point3.x == 0)
			{
				intersectionX = point4.x;
				intersectionY = slope1 * intersectionX + intercept1;
			}
			else if (point2.x - point1.x == 0)
			{
				intersectionX = point2.x;
				intersectionY = slope2 * intersectionX + intercept2;
			}
			else
			{
				if (slope1 - slope2 >= -0.00005 && slope1 - slope2 <= 0.00005)
				{
					return (point2 + point3) / 2;
				}

				intersectionX = (intercept2 - intercept1) / (slope1 - slope2);
				intersectionY = slope1 * intersectionX + intercept1;
			}

			return new Vector3(intersectionX, intersectionY);
		}

		/// <summary>
		/// Rotates a vector based on an angle
		/// </summary>
		/// <param name="vec">The vector to rotate</param>
		/// <param name="angle">The angle to rotate the vector</param>
		/// <returns>The new rotated vector</returns>
		private static Vector3 Rotate(Vector3 vec, float angle)
		{
			var rotatedVec = new Vector3();
			angle = Mathf.Deg2Rad * angle;
			rotatedVec.x = vec.x * Mathf.Cos(angle) - vec.y * Mathf.Sin(angle);
			rotatedVec.y = vec.x * Mathf.Sin(angle) + vec.y * Mathf.Cos(angle);
			return rotatedVec;
		}

		/// <summary>
		/// Calculates the angle of a vector
		/// </summary>
		/// <param name="vec">The vector</param>
		/// <returns>The angle of the vector</returns>
		private static float LineAngle(Vector3 vec)
		{
			return Mathf.Rad2Deg * Mathf.Atan2(vec.x, -vec.y);
		}

		#endregion
	}
}