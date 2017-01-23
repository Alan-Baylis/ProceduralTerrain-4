using UnityEngine;

public class SMapDisplay : MonoBehaviour
{
    [SerializeField] private Renderer m_renderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int _width = noiseMap.GetLength(0);
        int _height = noiseMap.GetLength(1);

        Texture2D _texture = new Texture2D(_width, _height);

        Color[] _colorMap = new Color[_width * _height];

        for(int y = 0; y < _height; ++y)
        {
            for (int x = 0; x < _width; ++x)
            {
                _colorMap[y * _width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        _texture.SetPixels(_colorMap);
        _texture.Apply();

        m_renderer.sharedMaterial.mainTexture = _texture;
        m_renderer.transform.localScale = new Vector3(_width, 1, _height);
    }
}
