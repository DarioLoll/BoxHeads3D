using System.Collections.Generic;
using UnityEngine;

namespace WorldGeneration.Assets.Scripts
{
    public class GrassSpawner : MonoBehaviour
    {
        public static GrassSpawner Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        [SerializeField] public SpawnableObject grass;
        [SerializeField] public Mesh[] grassMeshes;
        [SerializeField] public Material[] grassMaterials;
        
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        
        
    }
}
