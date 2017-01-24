using UnityEngine;

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}

public class SMapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap
    }

    [SerializeField] private DrawMode m_drawMode;
    [SerializeField] private int m_mapWidth = 20;
    [SerializeField] private int m_mapHeight = 20;
    [SerializeField] private float m_noiseScale = 1f;
    [SerializeField] private bool m_autoUpdate = false;
    [SerializeField] private int m_octaves = 4;
    [SerializeField] [Range(0,1)] private float m_persistance = 0.5f;
    [SerializeField] private float m_lacunarity = 2f;
    [SerializeField] private int m_seed = 1;
    [SerializeField] private Vector2 m_offset = new Vector2(1f, 1f);
    [SerializeField] private TerrainType[] m_regions;

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

    public void GenerateMap()
    {
        float[,] noiseMap = SNoise.GenerateNoiseMap(m_mapWidth, m_mapHeight, m_seed, m_noiseScale,
            m_octaves, m_persistance, m_lacunarity, m_offset);

        Color[] _colorMap = new Color[m_mapWidth * m_mapHeight];

        for (int y = 0; y < m_mapHeight; ++y)
        {
            for (int x = 0; x < m_mapWidth; ++x)
            {
                float _currentHeight = noiseMap[x, y];
                for (int i = 0; i < m_regions.Length; ++i)
                {
                    if(_currentHeight <= m_regions[i].height)
                    {
                        _colorMap[y * m_mapWidth + x] = m_regions[i].color;
                        break;
                    }
                }
            }
        }
        SMapDisplay _mapDisplay = FindObjectOfType<SMapDisplay>();

        if(m_drawMode == DrawMode.NoiseMap)
        {
            _mapDisplay.DrawTexture(STextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if(m_drawMode == DrawMode.ColorMap)
        {
            _mapDisplay.DrawTexture(STextureGenerator.TextureFromColorMap(_colorMap, m_mapWidth, m_mapHeight));
        }
    }

    private void OnValidate()
    {
        if(m_mapWidth < 1)
        {
            m_mapWidth = 1;
        }
        if (m_mapHeight < 1)
        {
            m_mapHeight = 1;
        }
        if (m_lacunarity < 1)
        {
            m_lacunarity = 1;
        }
        if (m_octaves < 0)
        {
            m_octaves = 0;
        }
    }
}
