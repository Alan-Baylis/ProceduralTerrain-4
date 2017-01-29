using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (SMapGenerator))]
public class SMapGeneratorEditor : Editor
{
	public override void OnInspectorGUI()
    {
		SMapGenerator _mapGen = (SMapGenerator)target;

		if (DrawDefaultInspector ())
        {
			if (_mapGen.AutoUpdate)
            {
				_mapGen.DrawMapInEditor ();
			}
		}

		if (GUILayout.Button ("Generate"))
        {
			_mapGen.DrawMapInEditor ();
		}
	}
}
