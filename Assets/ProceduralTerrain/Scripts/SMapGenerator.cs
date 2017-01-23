using UnityEngine;

public class SMapGenerator : MonoBehaviour
{
    [SerializeField] private int m_mapWidth = 20;
    [SerializeField] private int m_mapHeight = 20;
    [SerializeField] private float m_noiseScale = 1f;
    [SerializeField] private bool m_autoUpdate = false;
    [SerializeField] private int m_octaves = 4;
    [SerializeField] [Range(0,1)] private float m_persistance = 0.5f;
    [SerializeField] private float m_lacunarity = 2f;
    [SerializeField] private int m_seed = 1;
    [SerializeField] private Vector2 m_offset = new Vector2(1f, 1f);

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

        SMapDisplay _mapDisplay = FindObjectOfType<SMapDisplay>();

        _mapDisplay.DrawNoiseMap(noiseMap);
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
