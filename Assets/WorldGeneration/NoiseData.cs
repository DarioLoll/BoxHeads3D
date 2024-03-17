using UnityEngine;

namespace WorldGeneration
{
    [CreateAssetMenu]
    public class NoiseData : UpdatableData
    {
        public int seed;
        public float noiseScale;
        public int octaves;
        [Range(0, 1)]
        public float persistence;
        public float lacunarity;
        public Vector2 offset;
        public Noise.NormalizeMode normalizeMode;

        protected override void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1;
            if (octaves < 0) octaves = 0;
            base.OnValidate();
        }
    }
}
