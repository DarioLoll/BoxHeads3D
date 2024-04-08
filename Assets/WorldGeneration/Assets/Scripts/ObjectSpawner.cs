using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace WorldGeneration.Assets.Scripts
{
    public class ObjectSpawner : MonoBehaviour
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
        }
        
        [SerializeField] private SpawnableObject[] objects;

        [Header("Prefab Variation Settings")] [SerializeField] [Range(0, 1)]
        private float rotateTowardsNormal;

        [SerializeField] private Vector2 rotationRange;
        [SerializeField] private Vector3 minScale;

        [SerializeField] private Vector3 maxScale;
        

        [ContextMenu("Generate")]
        public void GenerateOnChunk(Bounds chunkBounds, Transform chunk, float waitTime = 0)
        {
            StartCoroutine(GenerateOnChunkCoroutine(chunkBounds, chunk, waitTime));
        }

        private IEnumerator GenerateOnChunkCoroutine(Bounds chunkBounds, Transform chunk, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            foreach (var spawnableObject in objects)
            {
                for (var i = 0; i < spawnableObject.density; i++)
                {
                    var sampleX = Random.Range(chunkBounds.min.x, chunkBounds.max.x);
                    var sampleY = Random.Range(chunkBounds.min.z, chunkBounds.max.z);
                    var rayStart = new Vector3(sampleX, spawnableObject.maxHeight, sampleY);

                    if (!Physics.Raycast(rayStart, Vector3.down, out var hit, Mathf.Infinity))
                        continue;

                    if (hit.point.y < spawnableObject.minHeight)
                        continue;

                    var instantiatedPrefab = Instantiate(spawnableObject.prefabs[Random.Range(0, spawnableObject.prefabs.Length)], chunk);
                    instantiatedPrefab.transform.position = hit.point;
                    instantiatedPrefab.transform.Rotate(Vector3.up, Random.Range(rotationRange.x, rotationRange.y),
                        Space.Self);
                    instantiatedPrefab.transform.rotation = Quaternion.Lerp(transform.rotation,
                        transform.rotation * Quaternion.FromToRotation(instantiatedPrefab.transform.up, hit.normal),
                        rotateTowardsNormal);
                    instantiatedPrefab.transform.localScale = new Vector3(
                        Random.Range(minScale.x, maxScale.x),
                        Random.Range(minScale.y, maxScale.y),
                        Random.Range(minScale.z, maxScale.z));
                }
            }
            
        }

        public void Clear()
        {
            while (transform.childCount != 0) DestroyImmediate(transform.GetChild(0).gameObject);
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