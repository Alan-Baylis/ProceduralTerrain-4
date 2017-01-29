using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;

public class SMapGenerator : MonoBehaviour
{
	private enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh,
        FalloffMap
    };

	[SerializeField] private DrawMode m_drawMode;
    [SerializeField] private SNoise.NormalizeMode m_normalizeMode;
    [SerializeField] private bool m_useFlatShading;
	[Range(0,6)] [SerializeField] private int m_editorPreviewLOD;
    [SerializeField] private float m_noiseScale;
    [SerializeField] private int m_octaves;
	[Range(0,1)] [SerializeField] private float m_persistance;
    [SerializeField] private float m_lacunarity;
    [SerializeField] private int m_seed;
    [SerializeField] private Vector2 m_offset;
    [SerializeField] private bool m_useFalloff;
    [SerializeField] private float m_meshHeightMultiplier;
    [SerializeField] private AnimationCurve m_meshHeightCurve;
    [SerializeField] private bool m_autoUpdate;
    [SerializeField] private TerrainType[] m_regions;

	private static SMapGenerator m_instance;
	private float[,] m_falloffMap;
	private Queue<MapThreadInfo<MapData>> m_mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	private Queue<MapThreadInfo<MeshData>> m_meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	private void Awake()
    {
		m_falloffMap = SFalloffGenerator.GenerateFalloffMap (mapChunkSize);
	}

	public static int mapChunkSize
    {
		get
        {
			if (m_instance == null)
            {
				m_instance = FindObjectOfType<SMapGenerator> ();
			}

			if (m_instance.m_useFlatShading)
            {
				return 95;
			}
            else
            {
				return 239;
			}
		}
	}

    public bool AutoUpdate
    {
        get
        {
            return m_autoUpdate;
        }

        set
        {
            m_autoUpdate = value;
        }
    }

    public void DrawMapInEditor()
    {
		MapData _mapData = GenerateMapData (Vector2.zero);

		SMapDisplay _display = FindObjectOfType<SMapDisplay> ();

        switch(m_drawMode)
        {
            case DrawMode.NoiseMap:
                _display.DrawTexture(STextureGenerator.TextureFromHeightMap(_mapData.heightMap));
                break;
            case DrawMode.ColorMap:
                _display.DrawTexture(STextureGenerator.TextureFromColorMap(_mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.Mesh:
                _display.DrawMesh(SMeshGenerator.GenerateTerrainMesh(_mapData.heightMap, m_meshHeightMultiplier, m_meshHeightCurve, m_editorPreviewLOD, m_useFlatShading), STextureGenerator.TextureFromColorMap(_mapData.colorMap, mapChunkSize, mapChunkSize));
                break;
            case DrawMode.FalloffMap:
                _display.DrawTexture(STextureGenerator.TextureFromHeightMap(SFalloffGenerator.GenerateFalloffMap(mapChunkSize)));
                break;
        }
	}

	public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
		ThreadStart _threadStart = delegate 
        {
			MapDataThread (center, callback);
		};

		new Thread (_threadStart).Start ();
	}

	private void MapDataThread(Vector2 center, Action<MapData> callback)
    {
		MapData _mapData = GenerateMapData (center);
		lock (m_mapDataThreadInfoQueue)
        {
			m_mapDataThreadInfoQueue.Enqueue (new MapThreadInfo<MapData> (callback, _mapData));
		}
	}

	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
		ThreadStart _threadStart = delegate 
        {
			MeshDataThread (mapData, lod, callback);
		};

		new Thread (_threadStart).Start ();
	}

	private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
		MeshData _meshData = SMeshGenerator.GenerateTerrainMesh (mapData.heightMap, m_meshHeightMultiplier, m_meshHeightCurve, lod, m_useFlatShading);
		lock (m_meshDataThreadInfoQueue)
        {
			m_meshDataThreadInfoQueue.Enqueue (new MapThreadInfo<MeshData> (callback, _meshData));
		}
	}

	private void Update()
    {
		if (m_mapDataThreadInfoQueue.Count > 0)
        {
			for (int i = 0; i < m_mapDataThreadInfoQueue.Count; ++i)
            {
				MapThreadInfo<MapData> _threadInfo = m_mapDataThreadInfoQueue.Dequeue ();
				_threadInfo.callback (_threadInfo.parameter);
			}
		}

		if (m_meshDataThreadInfoQueue.Count > 0)
        {
			for (int i = 0; i < m_meshDataThreadInfoQueue.Count; ++i)
            {
				MapThreadInfo<MeshData> _threadInfo = m_meshDataThreadInfoQueue.Dequeue ();
				_threadInfo.callback (_threadInfo.parameter);
			}
		}
	}

	MapData GenerateMapData(Vector2 center)
    {
		float[,] _noiseMap = SNoise.GenerateNoiseMap (mapChunkSize + 2, mapChunkSize + 2, m_seed, m_noiseScale, m_octaves, m_persistance, m_lacunarity, center + m_offset, m_normalizeMode);

		Color[] _colorMap = new Color[mapChunkSize * mapChunkSize];

		for (int y = 0; y < mapChunkSize; ++y)
        {
			for (int x = 0; x < mapChunkSize; ++x)
            {
				if (m_useFalloff)
                {
					_noiseMap [x, y] = Mathf.Clamp01(_noiseMap [x, y] - m_falloffMap [x, y]);
				}

				float _currentHeight = _noiseMap [x, y];

				for (int i = 0; i < m_regions.Length; ++i)
                {
					if (_currentHeight >= m_regions [i].height)
                    {
						_colorMap [y * mapChunkSize + x] = m_regions [i].color;
					}
                    else
                    {
						break;
					}
				}
			}
		}
		return new MapData (_noiseMap, _colorMap);
	}

	private void OnValidate()
    {
		if (m_lacunarity < 1)
        {
			m_lacunarity = 1;
		}
		if (m_octaves < 0)
        {
			m_octaves = 0;
		}

		m_falloffMap = SFalloffGenerator.GenerateFalloffMap (mapChunkSize);
	}

	struct MapThreadInfo<T>
    {
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo (Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}
	}

}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public float height;
	public Color color;
}

public struct MapData
{
	public readonly float[,] heightMap;
	public readonly Color[] colorMap;

	public MapData (float[,] heightMap, Color[] colorMap)
	{
		this.heightMap = heightMap;
		this.colorMap = colorMap;
	}
}
