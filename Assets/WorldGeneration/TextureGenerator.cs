using UnityEngine;

namespace WorldGeneration
{
    public static class TextureGenerator
    {
        public static Texture2D TextureFromColourMap(Color[] colors, int width, int height)
        {
            var texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }
        
        public static Texture2D TextureFromHeightMap(float[,] noiseMap)
        {
            var width = noiseMap.GetLength(0);
            var height = noiseMap.GetLength(1);

            var colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    colors[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
                }
            }
            return TextureFromColourMap(colors, width, height);
        }
    }
}
