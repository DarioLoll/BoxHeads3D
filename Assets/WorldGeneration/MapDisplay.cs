using UnityEngine;

namespace WorldGeneration
{
    public class MapDisplay : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;

        public void DisplayMap(Texture2D texture)
        {
            renderer.sharedMaterial.mainTexture = texture;
            renderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        }
        
        public void DisplayMesh(MeshData meshData, Texture2D texture)
        {
            meshFilter.sharedMesh = meshData.CreateMesh();
            meshRenderer.sharedMaterial.mainTexture = texture;
        }
    }
}
