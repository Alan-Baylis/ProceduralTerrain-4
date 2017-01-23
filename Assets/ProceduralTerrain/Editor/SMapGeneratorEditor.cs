using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SMapGenerator))]
public class SMapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SMapGenerator _mapGenerator = (SMapGenerator)target;

        if(DrawDefaultInspector())
        {
            if(_mapGenerator.AutoUpdate)
            {
                _mapGenerator.GenerateMap();
            }
        }

        if(GUILayout.Button("Generate"))
        {
            _mapGenerator.GenerateMap();
        }
    }
}
