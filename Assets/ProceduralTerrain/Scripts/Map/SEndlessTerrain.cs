using UnityEngine;
using System.Collections.Generic;

public class SEndlessTerrain : MonoBehaviour
{
	private const float m_scale = 2.5f;
    private const float m_viewerMoveThresholdForChunkUpdate = 25f;
    private const float m_sqrViewerMoveThresholdForChunkUpdate = m_viewerMoveThresholdForChunkUpdate * m_viewerMoveThresholdForChunkUpdate;

	[SerializeField] private LODInfo[] m_detailLevels;

    private static float m_maxViewDst;

    [SerializeField] private Transform m_viewer;
    [SerializeField] private Material m_mapMaterial;

    private static Vector2 m_viewerPosition;
    private Vector2 m_viewerPositionOld;
    private static SMapGenerator m_mapGenerator;
    private int m_chunkSize;
    private int m_chunksVisibleInViewDst;

    private Dictionary<Vector2, TerrainChunk> m_terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private static List<TerrainChunk> m_terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    private void Start()
    {
		m_mapGenerator = FindObjectOfType<SMapGenerator> ();

		m_maxViewDst = m_detailLevels [m_detailLevels.Length - 1].visibleDstThreshold;
		m_chunkSize = SMapGenerator.mapChunkSize - 1;
		m_chunksVisibleInViewDst = Mathf.RoundToInt(m_maxViewDst / m_chunkSize);

		UpdateVisibleChunks ();
	}

    private void Update()
    {
		m_viewerPosition = new Vector2 (m_viewer.position.x, m_viewer.position.z) / m_scale;

		if ((m_viewerPositionOld - m_viewerPosition).sqrMagnitude > m_sqrViewerMoveThresholdForChunkUpdate)
        {
			m_viewerPositionOld = m_viewerPosition;
			UpdateVisibleChunks ();
		}
	}

    private void UpdateVisibleChunks()
    {
		for (int i = 0; i < m_terrainChunksVisibleLastUpdate.Count; ++i)
        {
			m_terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}
		m_terrainChunksVisibleLastUpdate.Clear ();
			
		int _currentChunkCoordX = Mathf.RoundToInt (m_viewerPosition.x / m_chunkSize);
		int _currentChunkCoordY = Mathf.RoundToInt (m_viewerPosition.y / m_chunkSize);

		for (int yOffset = -m_chunksVisibleInViewDst; yOffset <= m_chunksVisibleInViewDst; ++yOffset)
        {
			for (int xOffset = -m_chunksVisibleInViewDst; xOffset <= m_chunksVisibleInViewDst; ++xOffset)
            {
				Vector2 _viewedChunkCoord = new Vector2 (_currentChunkCoordX + xOffset, _currentChunkCoordY + yOffset);

				if (m_terrainChunkDictionary.ContainsKey (_viewedChunkCoord))
                {
					m_terrainChunkDictionary [_viewedChunkCoord].UpdateTerrainChunk ();
				}
                else
                {
					m_terrainChunkDictionary.Add (_viewedChunkCoord, new TerrainChunk (_viewedChunkCoord, m_chunkSize, m_detailLevels, transform, m_mapMaterial));
				}
			}
		}
	}

	public class TerrainChunk
    {
        private GameObject m_meshObject;
        private Vector2 m_position;
        private Bounds m_bounds;

        private MeshRenderer m_meshRenderer;
        private MeshFilter m_meshFilter;
        private MeshCollider m_meshCollider;

        private LODInfo[] m_detailLevels;
        private LODMesh[] m_lodMeshes;
        private LODMesh m_collisionLODMesh;

        private MapData m_mapData;
        private bool m_mapDataReceived;
        private int m_previousLODIndex = -1;

		public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
			this.m_detailLevels = detailLevels;

			m_position = coord * size;
			m_bounds = new Bounds(m_position,Vector2.one * size);
			Vector3 _positionV3 = new Vector3(m_position.x, 0, m_position.y);

			m_meshObject = new GameObject("Terrain Chunk");
			m_meshRenderer = m_meshObject.AddComponent<MeshRenderer>();
			m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
			m_meshCollider = m_meshObject.AddComponent<MeshCollider>();
			m_meshRenderer.material = material;

			m_meshObject.transform.position = _positionV3 * m_scale;
			m_meshObject.transform.parent = parent;
			m_meshObject.transform.localScale = Vector3.one * m_scale;
			SetVisible(false);

			m_lodMeshes = new LODMesh[detailLevels.Length];

			for (int i = 0; i < detailLevels.Length; ++i)
            {
				m_lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);

				if (detailLevels[i].useForCollider)
                {
					m_collisionLODMesh = m_lodMeshes[i];
				}
			}
			m_mapGenerator.RequestMapData(m_position,OnMapDataReceived);
		}

		private void OnMapDataReceived(MapData mapData)
        {
			this.m_mapData = mapData;
			m_mapDataReceived = true;

			Texture2D _texture = STextureGenerator.TextureFromColorMap (mapData.colorMap, SMapGenerator.mapChunkSize, SMapGenerator.mapChunkSize);
			m_meshRenderer.material.mainTexture = _texture;

			UpdateTerrainChunk ();
		}

		public void UpdateTerrainChunk()
        {
			if (m_mapDataReceived)
            {
				float _viewerDstFromNearestEdge = Mathf.Sqrt (m_bounds.SqrDistance (m_viewerPosition));
				bool _visible = _viewerDstFromNearestEdge <= m_maxViewDst;

				if (_visible)
                {
					int _lodIndex = 0;

					for (int i = 0; i < m_detailLevels.Length - 1; ++i)
                    {
						if (_viewerDstFromNearestEdge > m_detailLevels [i].visibleDstThreshold)
                        {
							_lodIndex = i + 1;
						}
                        else
                        {
							break;
						}
					}

					if (_lodIndex != m_previousLODIndex)
                    {
						LODMesh _lodMesh = m_lodMeshes [_lodIndex];
						if (_lodMesh.HasMesh)
                        {
							m_previousLODIndex = _lodIndex;
							m_meshFilter.mesh = _lodMesh.Mesh;
						}
                        else if (!_lodMesh.HasRequestedMesh)
                        {
							_lodMesh.RequestMesh (m_mapData);
						}
					}

					if (_lodIndex == 0)
                    {
						if (m_collisionLODMesh.HasMesh)
                        {
							m_meshCollider.sharedMesh = m_collisionLODMesh.Mesh;
						}
                        else if (!m_collisionLODMesh.HasRequestedMesh)
                        {
							m_collisionLODMesh.RequestMesh (m_mapData);
						}
					}

					m_terrainChunksVisibleLastUpdate.Add (this);
				}
				SetVisible (_visible);
			}
		}

		public void SetVisible(bool visible)
        {
			m_meshObject.SetActive (visible);
		}

        public bool IsVisible()
        {
            return m_meshObject.activeSelf;
        }
	}

	private class LODMesh
    {
		private Mesh mesh;
        private bool hasRequestedMesh;
        private bool hasMesh;
        private int lod;
        private System.Action updateCallback;

        public bool HasRequestedMesh
        {
            get
            {
                return hasRequestedMesh;
            }

            set
            {
                hasRequestedMesh = value;
            }
        }

        public bool HasMesh
        {
            get
            {
                return hasMesh;
            }

            set
            {
                hasMesh = value;
            }
        }

        public Mesh Mesh
        {
            get
            {
                return mesh;
            }

            set
            {
                mesh = value;
            }
        }

        public LODMesh(int lod, System.Action updateCallback)
        {
			this.lod = lod;
			this.updateCallback = updateCallback;
		}

		private void OnMeshDataReceived(MeshData meshData)
        {
			Mesh = meshData.CreateMesh ();
			HasMesh = true;

			updateCallback ();
		}

		public void RequestMesh(MapData mapData)
        {
			HasRequestedMesh = true;
			m_mapGenerator.RequestMeshData (mapData, lod, OnMeshDataReceived);
		}
	}

	[System.Serializable]
	public struct LODInfo
    {
		public int lod;
		public float visibleDstThreshold;
		public bool useForCollider;
	}
}
