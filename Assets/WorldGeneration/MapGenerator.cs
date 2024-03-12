using UnityEngine;

namespace WorldGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        [SerializeField] private int mapWidth;
        [SerializeField] private int mapHeight;
        [SerializeField] private float noiseScale;
        public bool autoUpdate;

        public void GenerateMap()
        {
            var noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);
            var display = gameObject.GetComponent<MapDisplay>();
            display.DisplayMap(noiseMap);
        }
    }
}
