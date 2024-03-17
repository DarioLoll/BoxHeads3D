using UnityEditor;
using UnityEngine;

namespace WorldGeneration
{
    [CustomEditor(typeof(MapGenerator)), CanEditMultipleObjects]
    public class MapGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var mapGen = (MapGenerator) target;
            if (DrawDefaultInspector())
            {
                if (mapGen.autoUpdate)
                {
                    mapGen.DrawMapInEditor();
                }
            }

            if (GUILayout.Button("Generate"))
            {
                mapGen.DrawMapInEditor();
            }
        }
    }
}
