using UnityEngine;

namespace WorldGeneration
{
    public class MapDisplay : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;

        public void DisplayMap(float[,] noiseMap)
        {
            var texture = GenerateTexture(noiseMap);
            renderer.sharedMaterial.mainTexture = texture;
            renderer.transform.localScale = new Vector3(noiseMap.GetLength(0), 1, noiseMap.GetLength(1));
        }

        private Texture GenerateTexture(float[,] noiseMap)
        {
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);
            var texture = new Texture2D(width, height);

            var colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colors[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                }
            }
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
    }
}
