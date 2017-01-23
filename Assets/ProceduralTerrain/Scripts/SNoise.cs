using UnityEngine;

public static class SNoise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float noiseScale,
        int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] _noiseMap = new float[mapWidth, mapHeight];

        System.Random _prng = new System.Random(seed);
        Vector2[] _octaveOffsets = new Vector2[octaves];
        for(int i = 0; i < octaves; ++i)
        {
            float _xOffset = _prng.Next(-100000, 100000) + offset.x;
            float _yOffset = _prng.Next(-100000, 100000) + offset.y;
            _octaveOffsets[i] = new Vector2(_xOffset, _yOffset);
        }

        if(noiseScale <= 0f)
        {
            noiseScale = 0.0001f;
        }

        float _maxNoiseHeight = float.MinValue;
        float _minNoiseHeight = float.MaxValue;

        float _halfWidth = mapWidth / 2f;
        float _halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; ++y)
        {
            for (int x = 0; x < mapWidth; ++x)
            {
                float _amplitude = 1f;
                float _frequency = 1f;
                float _noiseHeight = 0f;

                for(int i = 0; i < octaves; ++i)
                {
                    float _xSample = (x - _halfWidth) / noiseScale * _frequency + _octaveOffsets[i].x;
                    float _ySample = (y - _halfHeight) / noiseScale * _frequency + _octaveOffsets[i].y;

                    float _perlinValue = Mathf.PerlinNoise(_xSample, _ySample) * 2 - 1;

                    _noiseHeight += _perlinValue * _amplitude;

                    _amplitude *= persistance;
                    _frequency *= lacunarity;
                }

                if (_noiseHeight > _maxNoiseHeight)
                {
                    _maxNoiseHeight = _noiseHeight; 
                }
                else if ((_noiseHeight < _minNoiseHeight))
                {
                    _minNoiseHeight = _noiseHeight;
                }

                _noiseMap[x, y] = _noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; ++y)
        {
            for (int x = 0; x < mapWidth; ++x)
            {
                _noiseMap[x, y] = Mathf.InverseLerp(_minNoiseHeight, _maxNoiseHeight, _noiseMap[x, y]);
            }
        }
        return _noiseMap;
    }
}
