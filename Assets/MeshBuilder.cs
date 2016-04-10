using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//TODO: Fix first/last pipe vertex normals

public class MeshBuilder
{
	public List<Vector3> vertices = new List<Vector3>();
	public List<int> indices = new List<int>();
	public List<Vector3> normals = new List<Vector3>();
	public List<Vector2> uvs = new List<Vector2>();
	

	public void Clear()
	{
		this.vertices.Clear();
		this.indices.Clear();
		this.normals.Clear();
		this.uvs.Clear();
	}

	public static Vector3 GetBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		t = t > 1 ? 1 : t;
		t = t < 0 ? 0 : t;
		// [x, y]=(1–t)^3*P0+3(1–t)^2*t*P1+3(1–t)*t^2*P2+t^3*P3
		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * t;

		Vector3 p = uuu * p0; //first term
		p += 3 * uu * t * p1; //second term
		p += 3 * u * tt * p2; //third term
		p += ttt * p3; //fourth term

		return p;
	}

	public StartAndEnd CreatePipe(PipeConfig config)
	{
		Vector3 centrePos = Vector3.zero;
		float bendAngleRadians = config.bendAngle * Mathf.Deg2Rad;
		float angleInc = bendAngleRadians / config.heightSegmentCount;
		float bendRadius = config.height / bendAngleRadians;
		Vector3 startOffset = new Vector3(bendRadius, 0.0f, 0.0f);
		float heightInc = (float)config.height / config.heightSegmentCount;
		Quaternion rotation = config.offsetRotation;
		float radius = config.radius;

		for (int i = 0; i <= config.heightSegmentCount; i++)
		{
			//We probably can combine these, for some nice geometry
			if (config.bendAngle == 0.0f)
			{
				centrePos = Vector3.up * (config.HeightModifier == null ? heightInc * i : config.HeightModifier(i));
			}
			else
			{
				centrePos.x = Mathf.Cos(angleInc * i);
				centrePos.y = Mathf.Sin(angleInc * i);
				centrePos *= bendRadius;
				centrePos -= startOffset;

				float zAngleDegrees = angleInc * i * Mathf.Rad2Deg;
				rotation = Quaternion.Euler(0.0f, 0.0f, zAngleDegrees);
			}
			radius = config.WidthModifier == null ? radius : config.WidthModifier((float)i / config.heightSegmentCount);

			float v = (float)i / config.heightSegmentCount;

			centrePos = config.offsetRotation * centrePos;
			centrePos += config.offset;

			CreateRing(config.radialSegmentCount, centrePos, radius, v, i > 0, rotation, config.flipNormals);
		}

		StartAndEnd result = new StartAndEnd();
		result.startPoint.position = Vector3.zero;
		result.startPoint.rotation = default(Quaternion);
		result.endPoint.position = centrePos;
		result.endPoint.rotation = Quaternion.Euler(0.0f, 0.0f, angleInc * config.heightSegmentCount * Mathf.Rad2Deg);

		if (config.createCaps)
		{
			CreateCap(Vector3.zero + config.offset, true, config.radialSegmentCount, (config.WidthModifier == null ? radius : config.WidthModifier(0)), config.offsetRotation);
			CreateCap(result.endPoint.position, false, config.radialSegmentCount, radius, result.endPoint.rotation);
		}

		return result;
	}

	void CreateCap(Vector3 centre, bool reverseDirection, int radialSegmentCount, float radius, Quaternion rotation = default(Quaternion))
	{
		Vector3 normal = reverseDirection ? Vector3.down : Vector3.up;

		//one vertex in the center:
		vertices.Add(centre);
		normals.Add(normal);
		uvs.Add(new Vector2(0.5f, 0.5f));

		int centreVertexIndex = vertices.Count - 1;

		//vertices around the edge:
		float angleInc = (Mathf.PI * 2.0f) / radialSegmentCount;

		for (int i = 0; i <= radialSegmentCount; i++)
		{
			float angle = angleInc * i;

			Vector3 unitPosition = Vector3.zero;
			unitPosition.x = Mathf.Cos(angle);
			unitPosition.z = Mathf.Sin(angle);
			unitPosition = rotation * unitPosition;

			this.vertices.Add(centre + unitPosition * radius);
			this.normals.Add(normal);

			Vector2 uv = new Vector2(unitPosition.x + 1.0f, unitPosition.z + 1.0f) * 0.5f;
			this.uvs.Add(uv);

			//build a triangle:
			if (i > 0)
			{
				int baseIndex = vertices.Count - 1;

				if (reverseDirection)
					this.AddTriangle(centreVertexIndex, baseIndex - 1,
						baseIndex);
				else
					this.AddTriangle(centreVertexIndex, baseIndex,
						baseIndex - 1);
			}
		}
	}

	void CreateRing(int segmentCount, Vector3 centre, float radius, float v, bool buildTriangles, Quaternion rotation, bool flipNormals = false)
	{
		float angleInc = (Mathf.PI * 2.0f) / segmentCount;

		for (int i = 0; i <= segmentCount; i++)
		{
			float angle = angleInc * i;

			Vector3 unitPosition = Vector3.zero;
			unitPosition.x = Mathf.Cos(angle); //beautiful
			unitPosition.z = Mathf.Sin(angle);

			unitPosition = rotation * unitPosition;

			this.vertices.Add(centre + unitPosition * radius);
			this.normals.Add(unitPosition);
			this.uvs.Add(new Vector2((float)i / segmentCount, v));

			if (i > 0 && buildTriangles)
			{
				int baseIndex = this.vertices.Count - 1;

				int vertsPerRow = segmentCount + 1;

				int index0 = baseIndex;
				int index1 = baseIndex - 1;
				int index2 = baseIndex - vertsPerRow;
				int index3 = baseIndex - vertsPerRow - 1;

				if (flipNormals)
				{
					this.AddTriangle(index0, index1, index2);
					this.AddTriangle(index2, index1, index3);
				}
				else
				{
					this.AddTriangle(index0, index2, index1);
					this.AddTriangle(index2, index3, index1);
				}
			}
		}
	}

	public float postHeight; //CONFIG!!
	void CreateFence()
	{
		float m_DistBetweenPosts = 0.3f;
		int m_SectionCount = 70;
		float postWidth = 0.1f;
		float crossPieceHeight = 1f;
		float postHeight = 1f;
		float crossPieceLenght = 0.1f;
		Vector3 prevCrossPosition = Vector3.zero;
		Quaternion prevRotation = Quaternion.identity;

		for (int i = 0; i <= m_SectionCount; i++)
		{
			Vector3 offset = Vector3.right * m_DistBetweenPosts * i;
			offset.y += Mathf.Sin(offset.x );

			float xAngle = Random.Range(-10f, 10f); //CONFIG!!
			float zAngle = Random.Range(-10f, 10f); //CONFIG!!
			Quaternion rotation = Quaternion.Euler(xAngle, 0.0f, zAngle);
			postHeight = Random.Range(0.8f, 1f);

			CreateFencePost(postHeight, offset, rotation);

			//Crosspiece
			Vector3 crossPosition = offset;
			//Offset back to post
			crossPosition += (Vector3.forward * postWidth * 0.5f);
			//Offset height
			crossPosition += rotation * (Vector3.up * (postHeight + crossPieceLenght) / 2);
			//Offset length
			crossPosition += Vector3.right * (postWidth / 2); //CONFIG!!

			float randomYStart = Random.Range(-0.20f, 0.20f);//CONFIG!!
			float randomYEnd = Random.Range(-0.20f, 0.20f);//CONFIG!!

			Vector3 crossYOffsetStart = prevRotation * (Vector3.up * randomYStart);
			Vector3 crossYOffsetEnd = rotation * (Vector3.up * randomYEnd);

			if (i != 0)
			{
				CreateFenceCrossPiece(prevCrossPosition + crossYOffsetStart, crossPosition + crossYOffsetEnd);
			}
			prevCrossPosition = crossPosition;
			prevRotation = rotation;
		}
	}

	void CreateFencePost(float postHeight, Vector3 position, Quaternion rotation = default(Quaternion))
	{
		this.CreateHexahedron(postHeight, 0.1f, 0.1f, position, rotation);
	}

	void CreateFenceCrossPiece(Vector3 start, Vector3 end)
	{
		Vector3 offset = new Vector3(start.x, start.y, start.z);
		Vector3 dir = end - start;
		float length = dir.magnitude;
		Quaternion rotation = Quaternion.LookRotation(dir);
		CreateHexahedron(0.1f, 0.05f, length, offset, rotation);
	}

	void CreateHouse(float height = 1f, float roofHeight = 0.5f, float width = 1f, float depth = 1f)
	{
		float roofOverhangSide = 0.1f;
		float roofOverhangFront = 0.1f;
		float roofBias = 0.01f;

		//Body
		CreateHexahedron();
		//Triangles
		Vector3 roofPeak = Vector3.up * (height + roofHeight) + (Vector3.right * width) / 2;
		Vector3 wallTopLeft = Vector3.up * height;
		Vector3 wallTopRight = wallTopLeft + Vector3.right * width;
		Vector3 forwardDir = Vector3.forward * depth;
		CreateTriangle(wallTopLeft, roofPeak, wallTopRight);
		CreateTriangle(wallTopLeft + forwardDir, wallTopRight + forwardDir, roofPeak + forwardDir);
		//Roof
		Vector3 dirFromPeakLeft = wallTopLeft - roofPeak;
		dirFromPeakLeft += dirFromPeakLeft.normalized * roofOverhangSide;
		Vector3 dirFromPeakRight = wallTopRight - roofPeak;
		dirFromPeakRight += dirFromPeakRight.normalized * roofOverhangSide;

		roofPeak -= Vector3.forward * roofOverhangFront;
		roofPeak += Vector3.up * roofBias;
		forwardDir += Vector3.forward * roofOverhangFront * 2;

		CreateQuad(roofPeak, forwardDir, dirFromPeakLeft);
		CreateQuad(roofPeak, dirFromPeakRight, forwardDir);
		CreateQuad(roofPeak, dirFromPeakLeft, forwardDir);
		CreateQuad(roofPeak, forwardDir, dirFromPeakRight);


	}

	void CreateTriangle(Vector3 corner0, Vector3 corner1, Vector3 corner2)
	{
		Vector3 normal = Vector3.Cross((corner1 - corner0), (corner2 - corner0)).normalized;
		this.vertices.Add(corner0);
		this.vertices.Add(corner1);
		this.vertices.Add(corner2);

		this.uvs.Add(new Vector2(0f, 0f));
		this.uvs.Add(new Vector2(0f, 1f));
		this.uvs.Add(new Vector2(1f, 1f));

		this.normals.Add(normal);
		this.normals.Add(normal);
		this.normals.Add(normal);

		int baseIndex = this.vertices.Count - 3;
		this.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
	}

	void CreateHexahedron(float height = 1f, float width = 1f, float lenght = 1f, Vector3 offset = default(Vector3), Quaternion rotation = default(Quaternion))
	{
		Vector3 upDir = rotation * Vector3.up * height;
		Vector3 rightDir = rotation * Vector3.right * width;
		Vector3 forwardDir = rotation * Vector3.forward * lenght;

		Vector3 nearCorner = Vector3.zero + offset;
		Vector3 farCorner = upDir + rightDir + forwardDir + offset;

		CreateQuad(nearCorner, forwardDir, rightDir);
		CreateQuad(nearCorner, rightDir, upDir);
		CreateQuad(nearCorner, upDir, forwardDir);

		CreateQuad(farCorner, -rightDir, -forwardDir);
		CreateQuad(farCorner, -upDir, -rightDir);
		CreateQuad(farCorner, -forwardDir, -upDir);
	}

	void CreateQuadForGrid(Vector3 position, Vector2 uv, bool buildTriangles, int vertsPerRow)
	{
		this.vertices.Add(position);
		this.uvs.Add(uv);

		if (buildTriangles)
		{
			int baseIndex = this.vertices.Count - 1;

			int index0 = baseIndex;
			int index1 = baseIndex - 1;
			int index2 = baseIndex - vertsPerRow;
			int index3 = baseIndex - vertsPerRow - 1;

			this.AddTriangle(index0, index2, index1);
			this.AddTriangle(index2, index3, index1);
		}
	}

	void CreateQuad(Vector3 offset, Vector3 widthDir, Vector3 lenghtDir)
	{
		Vector3 normal = Vector3.Cross(lenghtDir, widthDir).normalized;

		this.vertices.Add(offset);
		this.vertices.Add(offset + lenghtDir);
		this.vertices.Add(offset + lenghtDir + widthDir);
		this.vertices.Add(offset + widthDir);

		this.uvs.Add(new Vector2(0.0f, 0.0f));
		this.uvs.Add(new Vector2(0.0f, 1.0f));
		this.uvs.Add(new Vector2(1.0f, 1.0f));
		this.uvs.Add(new Vector2(1.0f, 0.0f));

		this.normals.Add(normal);
		this.normals.Add(normal);
		this.normals.Add(normal);
		this.normals.Add(normal);

		int baseIndex = this.vertices.Count - 4;

		this.AddTriangle(baseIndex, baseIndex + 1, baseIndex + 2);
		this.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 3);
	}

	public void AddTriangle(int index_0, int index_1, int index_2)
	{
		indices.Add(index_0);
		indices.Add(index_1);
		indices.Add(index_2);
	}

	public Mesh BuildMesh()
	{
		Mesh mesh = new Mesh();
		mesh.vertices = vertices.ToArray();
		mesh.triangles = indices.ToArray();

		//normals are optional. Only use them if we have the correct amount:
		if (normals.Count == vertices.Count)
		{
			mesh.normals = normals.ToArray();
		}

		//UVs are optional. Only use them if we have the correct amount:
		if (uvs.Count == vertices.Count)
		{
			mesh.uv = uvs.ToArray();
		}
		mesh.RecalculateBounds();


		return mesh;
	}

} /* ####GRID MAKER TEMPLATE. JUST LOOP THROUGH PREDEFINED HEIGHTMAP#### void Start() { int m_SegmentCount = 5; int m_Length = 1; int m_Height = 1; int m_Width = 1; for (int i = 0; i <= m_SegmentCount; i++) { float z = m_Length * i; float v = (1.0f / m_SegmentCount) * i; for (int j = 0; j <= m_SegmentCount; j++) { float x = m_Width * j; float u = (1.0f / m_SegmentCount) * j;
 
       Vector3 offset = new Vector3(x, Random.Range(0.0f, m_Height), z);
 
       Vector2 uv = new Vector2(u, v);
       bool buildTriangles = i > 0 && j > 0;
 
       CreateQuadForGrid(offset, uv, buildTriangles, m_SegmentCount + 1);
   }
 
}
 
//Create the mesh: Mesh mesh = this.BuildMesh(); mesh.RecalculateNormals(); MeshFilter filter = GetComponent<MeshFilter>(); filter.sharedMesh = filter.mesh = mesh; }*/

public class StartAndEnd
{
	public Point startPoint;
	public Point endPoint;
	public StartAndEnd()
	{
		startPoint = new Point();
		endPoint = new Point();
	}

}

public struct Point
{
	public Vector3 position;
	public Quaternion rotation;
}