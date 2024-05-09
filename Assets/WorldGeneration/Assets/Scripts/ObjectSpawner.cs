using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace WorldGeneration.Assets.Scripts
{
    public class ObjectSpawner : NetworkBehaviour
    {
        public static ObjectSpawner Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _populatedChunkCoords = new NetworkList<Vector2>();
        }
        
        [SerializeField] private SpawnableObject[] objects;

        [Header("Prefab Variation Settings")] [SerializeField] [Range(0, 1)]
        private float rotateTowardsNormal;

        [SerializeField] private Vector2 rotationRange;
        [SerializeField] private Vector3 minScale;

        [SerializeField] private Vector3 maxScale;
        
        private NetworkList<Vector2> _populatedChunkCoords;
        
        public void GenerateOnChunk(Vector3 chunkBoundsMin, Vector3 chunkBoundsMax, Vector2 chunkCoord)
        {
            if (_populatedChunkCoords.Contains(chunkCoord)) return;
            OnChunkPopulatedServerRpc(chunkCoord);
            for (var i = 0; i < objects.Length; i++)
            {
                var spawnableObject = objects[i];
                for (var j = 0; j < spawnableObject.density; j++)
                {
                    var sampleX = Random.Range(chunkBoundsMin.x, chunkBoundsMax.x);
                    var sampleY = Random.Range(chunkBoundsMin.z, chunkBoundsMax.z);
                    var rayStart = new Vector3(sampleX, spawnableObject.maxHeight, sampleY);

                    if (!Physics.Raycast(rayStart, Vector3.down, out var hit, Mathf.Infinity))
                        continue;

                    if (hit.point.y < spawnableObject.minHeight)
                        continue;

                    var position = hit.point;
                    var scale = new Vector3(
                        Random.Range(minScale.x, maxScale.x),
                        Random.Range(minScale.y, maxScale.y),
                        Random.Range(minScale.z, maxScale.z));
                    var prefab = Random.Range(0, spawnableObject.prefabs.Length);
                    SpawnObjectServerRpc(i, prefab, position, scale, hit.normal);
                }
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void OnChunkPopulatedServerRpc(Vector2 chunkCoord)
        {
            _populatedChunkCoords.Add(chunkCoord);
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void SpawnObjectServerRpc(int objectIndex, int prefabIndex, Vector3 worldPosition, Vector3 localScale, Vector3 normal)
        {
            var obj = objects[objectIndex].prefabs[prefabIndex];
            var instantiatedPrefab = Instantiate(obj);
            instantiatedPrefab.transform.position = worldPosition;
            instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(rotationRange.x, rotationRange.y),
                Space.Self);
            instantiatedPrefab.transform.rotation = Quaternion.Lerp(transform.rotation,
                transform.rotation * Quaternion.FromToRotation(instantiatedPrefab.transform.up, normal), rotateTowardsNormal);
            instantiatedPrefab.transform.localScale = localScale;
            instantiatedPrefab.GetComponent<NetworkObject>().Spawn(true);
        }
        
    }
}

[Serializable]
public struct SpawnableObject
{
    public GameObject[] prefabs;
    public int density;
    public float minHeight;
    public float maxHeight;
}