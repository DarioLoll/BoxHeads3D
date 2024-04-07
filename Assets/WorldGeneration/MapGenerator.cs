using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace WorldGeneration
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColourMap,
            Mesh,
            FalloffMap
        }
    
        public NoiseData noiseData;
        public TerrainData terrainData;

        public DrawMode drawMode;

        public const int mapChunkSize = 239;
        [Range(0, 6)] public int editorPreviewLOD;
        public bool autoUpdate;

        public TerrainType[] regions;

        private float[,] falloffMap;

        private readonly Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new();
        private readonly Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
        
        private void OnValuesUpdated()
        {
            if (!Application.isPlaying) DrawMapInEditor();
        }

        public void DrawMapInEditor()
        {
            var mapData = GenerateMapData(Vector2.zero);

            var display = FindObjectOfType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
                display.DisplayMap(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            else if (drawMode == DrawMode.ColourMap)
                display.DisplayMap(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            else if (drawMode == DrawMode.Mesh)
                display.DisplayMesh(
                    MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve,
                        editorPreviewLOD),
                    TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
            else if (drawMode == DrawMode.FalloffMap)
                display.DisplayMap(TextureGenerator.TextureFromHeightMap(Noise.GenerateFalloffMap(mapChunkSize + 2)));
        }

        public void RequestMapData(Vector2 centre, Action<MapData> callback)
        {
            ThreadStart threadStart = delegate { MapDataThread(centre, callback); };

            new Thread(threadStart).Start();
        }

        private void MapDataThread(Vector2 centre, Action<MapData> callback)
        {
            var mapData = GenerateMapData(centre);
            lock (mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
        {
            ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };

            new Thread(threadStart).Start();
        }

        private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            var meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod);
            lock (meshDataThreadInfoQueue)
            {
                meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void Update()
        {
            if (mapDataThreadInfoQueue.Count > 0)
                for (var i = 0; i < mapDataThreadInfoQueue.Count; i++)
                {
                    var threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }

            if (meshDataThreadInfoQueue.Count > 0)
                for (var i = 0; i < meshDataThreadInfoQueue.Count; i++)
                {
                    var threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
        }

        private MapData GenerateMapData(Vector2 centre)
        {
            var noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, noiseData.seed, 
                noiseData.noiseScale, noiseData.octaves, noiseData.persistence, noiseData.lacunarity, centre + noiseData.offset, noiseData.normalizeMode);

            if (terrainData.useFalloff) falloffMap = Noise.GenerateFalloffMap(mapChunkSize + 2);
            
            var colourMap = new Color[(mapChunkSize+2) * (mapChunkSize+2)];
            for (var y = 0; y < mapChunkSize+2; y++)
            for (var x = 0; x < mapChunkSize+2; x++)
            {
                if (terrainData.useFalloff) noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                var currentHeight = noiseMap[x, y];
                for (var i = 0; i < regions.Length; i++)
                    if (currentHeight >= regions[i].height)
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                    else
                        break;
            }


            return new MapData(noiseMap, colourMap);
        }

        private void OnValidate()
        {
            
            if (terrainData != null)
            {
                terrainData.OnValuesUpdated -= OnValuesUpdated;
                terrainData.OnValuesUpdated += OnValuesUpdated;
            }
            if (noiseData != null)
            {
                noiseData.OnValuesUpdated -= OnValuesUpdated;
                noiseData.OnValuesUpdated += OnValuesUpdated;
            }
        }

        private struct MapThreadInfo<T>
        {
            public readonly Action<T> callback;
            public readonly T parameter;

            public MapThreadInfo(Action<T> callback, T parameter)
            {
                this.callback = callback;
                this.parameter = parameter;
            }
        }
    }

    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colourMap;

        public MapData(float[,] heightMap, Color[] colourMap)
        {
            this.heightMap = heightMap;
            this.colourMap = colourMap;
        }
    }
}