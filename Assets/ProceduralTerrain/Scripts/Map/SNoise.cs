using UnityEngine;

public static class SNoise
{
	public enum NormalizeMode
    {
        Local,
        Global
    };

	public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, 
        float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {
		float[,] _noiseMap = new float[mapWidth, mapHeight];

		System.Random _prng = new System.Random (seed);
		Vector2[] _octaveOffsets = new Vector2[octaves];

		float _maxPossibleHeight = 0;
		float _amplitude = 1;
		float _frequency = 1;

		for (int i = 0; i < octaves; ++i)
        {
			float _offsetX = _prng.Next (-100000, 100000) + offset.x;
			float _offsetY = _prng.Next (-100000, 100000) - offset.y;
			_octaveOffsets [i] = new Vector2 (_offsetX, _offsetY);

			_maxPossibleHeight += _amplitude;
			_amplitude *= persistance;
		}

		if (scale <= 0)
        {
			scale = 0.0001f;
		}

		float _maxLocalNoiseHeight = float.MinValue;
		float _minLocalNoiseHeight = float.MaxValue;

		float _halfWidth = mapWidth / 2f;
		float _halfHeight = mapHeight / 2f;


		for (int y = 0; y < mapHeight; ++y)
        {
			for (int x = 0; x < mapWidth; ++x)
            {
				_amplitude = 1;
				_frequency = 1;
				float _noiseHeight = 0;

				for (int i = 0; i < octaves; ++i)
                {
					float _sampleX = (x - _halfWidth + _octaveOffsets[i].x) / scale * _frequency;
					float _sampleY = (y - _halfHeight + _octaveOffsets[i].y) / scale * _frequency;

					float _perlinValue = Mathf.PerlinNoise (_sampleX, _sampleY) * 2 - 1;
					_noiseHeight += _perlinValue * _amplitude;

					_amplitude *= persistance;
					_frequency *= lacunarity;
				}

				if (_noiseHeight > _maxLocalNoiseHeight)
                {
					_maxLocalNoiseHeight = _noiseHeight;
				}
                else if (_noiseHeight < _minLocalNoiseHeight)
                {
					_minLocalNoiseHeight = _noiseHeight;
				}

				_noiseMap [x, y] = _noiseHeight;
			}
		}

		for (int y = 0; y < mapHeight; ++y)
        {
			for (int x = 0; x < mapWidth; ++x)
            {
				if (normalizeMode == NormalizeMode.Local)
                {
					_noiseMap [x, y] = Mathf.InverseLerp (_minLocalNoiseHeight, _maxLocalNoiseHeight, _noiseMap [x, y]);
				}
                else
                {
					float _normalizedHeight = (_noiseMap [x, y] + 1) / (_maxPossibleHeight / 0.9f);
					_noiseMap [x, y] = Mathf.Clamp(_normalizedHeight, 0, int.MaxValue);
				}
			}
		}
		return _noiseMap;
	}
}
