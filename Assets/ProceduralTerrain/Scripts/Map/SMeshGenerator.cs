using UnityEngine;

public static class SMeshGenerator
{
	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, 
        int levelOfDetail, bool useFlatShading)
    {
		AnimationCurve heightCurve = new AnimationCurve (_heightCurve.keys);

		int _meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
		int _borderedSize = heightMap.GetLength (0);
		int _meshSize = _borderedSize - 2*_meshSimplificationIncrement;
		int _meshSizeUnsimplified = _borderedSize - 2;

		float _topLeftX = (_meshSizeUnsimplified - 1) / -2f;
		float _topLeftZ = (_meshSizeUnsimplified - 1) / 2f;

		int _verticesPerLine = (_meshSize - 1) / _meshSimplificationIncrement + 1;

		MeshData _meshData = new MeshData (_verticesPerLine, useFlatShading);

		int[,] _vertexIndicesMap = new int[_borderedSize,_borderedSize];
		int _meshVertexIndex = 0;
		int _borderVertexIndex = -1;

		for (int y = 0; y < _borderedSize; y += _meshSimplificationIncrement)
        {
			for (int x = 0; x < _borderedSize; x += _meshSimplificationIncrement)
            {
				bool _isBorderVertex = y == 0 || y == _borderedSize - 1 || x == 0 || x == _borderedSize - 1;

				if (_isBorderVertex)
                {
					_vertexIndicesMap [x, y] = _borderVertexIndex;
					_borderVertexIndex--;
				}
                else
                {
					_vertexIndicesMap [x, y] = _meshVertexIndex;
					_meshVertexIndex++;
				}
			}
		}

		for (int y = 0; y < _borderedSize; y += _meshSimplificationIncrement)
        {
			for (int x = 0; x < _borderedSize; x += _meshSimplificationIncrement)
            {
				int _vertexIndex = _vertexIndicesMap [x, y];
				Vector2 _percent = new Vector2 ((x-_meshSimplificationIncrement) / (float)_meshSize, (y-_meshSimplificationIncrement) / (float)_meshSize);
				float _height = heightCurve.Evaluate (heightMap [x, y]) * heightMultiplier;
				Vector3 _vertexPosition = new Vector3 (_topLeftX + _percent.x * _meshSizeUnsimplified, _height, _topLeftZ - _percent.y * _meshSizeUnsimplified);

				_meshData.AddVertex (_vertexPosition, _percent, _vertexIndex);

				if (x < _borderedSize - 1 && y < _borderedSize - 1)
                {
					int _a = _vertexIndicesMap [x, y];
					int _b = _vertexIndicesMap [x + _meshSimplificationIncrement, y];
					int _c = _vertexIndicesMap [x, y + _meshSimplificationIncrement];
					int _d = _vertexIndicesMap [x + _meshSimplificationIncrement, y + _meshSimplificationIncrement];

					_meshData.AddTriangle (_a,_d,_c);
					_meshData.AddTriangle (_d,_a,_b);
				}
				_vertexIndex++;
			}
		}

		_meshData.ProcessMesh ();

		return _meshData;
	}
}

public class MeshData
{
	Vector3[] m_vertices;
	int[] m_triangles;
	Vector2[] m_uvs;
	Vector3[] m_bakedNormals;

	Vector3[] m_borderVertices;
	int[] m_borderTriangles;

	int m_triangleIndex;
	int m_borderTriangleIndex;

	bool m_useFlatShading;

	public MeshData(int verticesPerLine, bool useFlatShading)
    {
		this.m_useFlatShading = useFlatShading;

		m_vertices = new Vector3[verticesPerLine * verticesPerLine];
		m_uvs = new Vector2[verticesPerLine * verticesPerLine];
		m_triangles = new int[(verticesPerLine-1)*(verticesPerLine-1)*6];

		m_borderVertices = new Vector3[verticesPerLine * 4 + 4];
		m_borderTriangles = new int[24 * verticesPerLine];
	}

	public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
		if (vertexIndex < 0)
        {
			m_borderVertices [-vertexIndex - 1] = vertexPosition;
		}
        else
        {
			m_vertices [vertexIndex] = vertexPosition;
			m_uvs [vertexIndex] = uv;
		}
	}

	public void AddTriangle(int a, int b, int c)
    {
		if (a < 0 || b < 0 || c < 0)
        {
			m_borderTriangles [m_borderTriangleIndex] = a;
			m_borderTriangles [m_borderTriangleIndex + 1] = b;
			m_borderTriangles [m_borderTriangleIndex + 2] = c;
			m_borderTriangleIndex += 3;
		}
        else
        {
			m_triangles [m_triangleIndex] = a;
			m_triangles [m_triangleIndex + 1] = b;
			m_triangles [m_triangleIndex + 2] = c;
			m_triangleIndex += 3;
		}
	}

	private Vector3[] CalculateNormals()
    {
		Vector3[] _vertexNormals = new Vector3[m_vertices.Length];
		int _triangleCount = m_triangles.Length / 3;

		for (int i = 0; i < _triangleCount; ++i)
        {
			int _normalTriangleIndex = i * 3;
			int _vertexIndexA = m_triangles [_normalTriangleIndex];
			int _vertexIndexB = m_triangles [_normalTriangleIndex + 1];
			int _vertexIndexC = m_triangles [_normalTriangleIndex + 2];

			Vector3 _triangleNormal = SurfaceNormalFromIndices (_vertexIndexA, _vertexIndexB, _vertexIndexC);

			_vertexNormals [_vertexIndexA] += _triangleNormal;
			_vertexNormals [_vertexIndexB] += _triangleNormal;
			_vertexNormals [_vertexIndexC] += _triangleNormal;
		}

		int _borderTriangleCount = m_borderTriangles.Length / 3;

		for (int i = 0; i < _borderTriangleCount; ++i)
        {
			int _normalTriangleIndex = i * 3;
			int _vertexIndexA = m_borderTriangles [_normalTriangleIndex];
			int _vertexIndexB = m_borderTriangles [_normalTriangleIndex + 1];
			int _vertexIndexC = m_borderTriangles [_normalTriangleIndex + 2];

			Vector3 _triangleNormal = SurfaceNormalFromIndices (_vertexIndexA, _vertexIndexB, _vertexIndexC);

			if (_vertexIndexA >= 0)
            {
				_vertexNormals [_vertexIndexA] += _triangleNormal;
			}
			if (_vertexIndexB >= 0)
            {
				_vertexNormals [_vertexIndexB] += _triangleNormal;
			}
			if (_vertexIndexC >= 0)
            {
				_vertexNormals [_vertexIndexC] += _triangleNormal;
			}
		}


		for (int i = 0; i < _vertexNormals.Length; ++i)
        {
			_vertexNormals [i].Normalize ();
		}

		return _vertexNormals;
	}

	private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
		Vector3 _pointA = (indexA < 0)?m_borderVertices[-indexA-1] : m_vertices [indexA];
		Vector3 _pointB = (indexB < 0)?m_borderVertices[-indexB-1] : m_vertices [indexB];
		Vector3 _pointC = (indexC < 0)?m_borderVertices[-indexC-1] : m_vertices [indexC];

		Vector3 _sideAB = _pointB - _pointA;
		Vector3 _sideAC = _pointC - _pointA;

		return Vector3.Cross (_sideAB, _sideAC).normalized;
	}

	public void ProcessMesh()
    {
		if (m_useFlatShading)
        {
			FlatShading ();
		}
        else
        {
			BakeNormals ();
		}
	}

	private void BakeNormals()
    {
		m_bakedNormals = CalculateNormals ();
	}

	private void FlatShading()
    {
		Vector3[] _flatShadedVertices = new Vector3[m_triangles.Length];
		Vector2[] _flatShadedUvs = new Vector2[m_triangles.Length];

		for (int i = 0; i < m_triangles.Length; ++i)
        {
			_flatShadedVertices [i] = m_vertices [m_triangles [i]];
			_flatShadedUvs [i] = m_uvs [m_triangles [i]];
			m_triangles [i] = i;
		}

		m_vertices = _flatShadedVertices;
		m_uvs = _flatShadedUvs;
	}

	public Mesh CreateMesh()
    {
		Mesh _mesh = new Mesh ();
		_mesh.vertices = m_vertices;
		_mesh.triangles = m_triangles;
		_mesh.uv = m_uvs;

		if (m_useFlatShading)
        {
			_mesh.RecalculateNormals ();
		}
        else
        {
			_mesh.normals = m_bakedNormals;
		}
		return _mesh;
	}
}