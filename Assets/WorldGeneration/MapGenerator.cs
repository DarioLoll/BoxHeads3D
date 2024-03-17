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

        public DrawMode drawMode;
        
        public Noise.NormalizeMode normalizeMode;

        public const int MapChunkSize = 241;
        [Range(0, 6)] public int editorPreviewLOD;
        public float noiseScale;

        public int octaves;
        [Range(0, 1)] public float persistence;
        public float lacunarity;

        public int seed;
        public Vector2 offset;

        public float meshHeightMultiplier;
        public AnimationCurve meshHeightCurve;
        
        public bool useFalloff;

        public bool autoUpdate;

        public TerrainType[] regions;

        private readonly Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new();
        private readonly Queue<MapThreadInfo<MeshData>> _meshDataThreadInfoQueue = new();

        private float[,] _falloffMap;

        private void Awake()
        {
            _falloffMap = Noise.GenerateFalloffMap(MapChunkSize);            
        }

        public void DrawMapInEditor()
        {
            var mapData = GenerateMapData(Vector2.zero);

            var display = FindFirstObjectByType<MapDisplay>();
            if (drawMode == DrawMode.NoiseMap)
                display.DisplayMap(TextureGenerator.GenerateTexture(mapData.heightMap));
            else if (drawMode == DrawMode.ColourMap)
                display.DisplayMap(TextureGenerator.GenerateTexture(mapData.colourMap, MapChunkSize, MapChunkSize));
            else if (drawMode == DrawMode.Mesh)
                display.DisplayMesh(
                    MeshGenerator.GenerateMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve,
                        editorPreviewLOD),
                    TextureGenerator.GenerateTexture(mapData.colourMap, MapChunkSize, MapChunkSize));
            else if (drawMode == DrawMode.FalloffMap)
                display.DisplayMap(TextureGenerator.GenerateTexture(Noise.GenerateFalloffMap(MapChunkSize)));
        }

        public void RequestMapData(Vector2 centre, Action<MapData> callback)
        {
            ThreadStart threadStart = delegate { MapDataThread(centre, callback); };

            new Thread(threadStart).Start();
        }

        private void MapDataThread(Vector2 centre, Action<MapData> callback)
        {
            var mapData = GenerateMapData(centre);
            lock (_mapDataThreadInfoQueue)
            {
                _mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
        {
            ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };
            new Thread(threadStart).Start();
        }

        private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            var meshData = MeshGenerator.GenerateMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, lod);
            lock (_meshDataThreadInfoQueue)
            {
                _meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void Update()
        {
            if (_mapDataThreadInfoQueue.Count > 0)
                for (var i = 0; i < _mapDataThreadInfoQueue.Count; i++)
                {
                    var threadInfo = _mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }

            if (_meshDataThreadInfoQueue.Count > 0)
                for (var i = 0; i < _meshDataThreadInfoQueue.Count; i++)
                {
                    var threadInfo = _meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
        }

        private MapData GenerateMapData(Vector2 centre)
        {
            var noiseMap = Noise.GenerateNoiseMap(MapChunkSize, MapChunkSize, seed, noiseScale, octaves, persistence,
                lacunarity, centre + offset, normalizeMode);

            var colourMap = new Color[MapChunkSize * MapChunkSize];
            for (var y = 0; y < MapChunkSize; y++)
            for (var x = 0; x < MapChunkSize; x++)
            {
                if (useFalloff) 
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - _falloffMap[x, y]);
                var currentHeight = noiseMap[x, y];
                for (var i = 0; i < regions.Length; i++)
                    if (currentHeight >= regions[i].height)
                    {
                        colourMap[y * MapChunkSize + x] = regions[i].colour;
                    }
                    else
                    {
                        break;
                    }
            }


            return new MapData(noiseMap, colourMap);
        }

        private void OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1;
            if (octaves < 0) octaves = 0;
            
            _falloffMap = Noise.GenerateFalloffMap(MapChunkSize);
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