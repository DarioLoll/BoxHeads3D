using UnityEngine;

namespace WorldGeneration
{
    [CreateAssetMenu]
    public class TerrainData : UpdatableData
    {
        public float uniformScale = 2.5f;
        public bool useFalloff;
        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;
    }
}
