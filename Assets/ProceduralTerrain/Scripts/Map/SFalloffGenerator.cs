using UnityEngine;

public static class SFalloffGenerator
{
	public static float[,] GenerateFalloffMap(int size)
    {
		float[,] _map = new float[size, size];

		for (int i = 0; i < size; ++i)
        {
			for (int j = 0; j < size; ++j)
            {
				float _x = i / (float)size * 2 - 1;
				float _y = j / (float)size * 2 - 1;

				float _value = Mathf.Max (Mathf.Abs (_x), Mathf.Abs (_y));
				_map [i, j] = Evaluate(_value);
			}
		}
		return _map;
	}

	private static float Evaluate(float value)
    {
		float _a = 3;
		float _b = 2.2f;

		return Mathf.Pow (value, _a) / (Mathf.Pow (value, _a) + Mathf.Pow (_b - _b * value, _a));
	}
}
