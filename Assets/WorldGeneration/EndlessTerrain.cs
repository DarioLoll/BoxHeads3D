using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldGeneration
{
    public class EndlessTerrain : MonoBehaviour
    {
        private const float Scale = 5f;
        
        private const float ViewerMoveThresholdForChunkUpdate = 25f;

        private const float SqrViewerMoveThresholdForChunkUpdate =
            ViewerMoveThresholdForChunkUpdate * ViewerMoveThresholdForChunkUpdate;

        public LODInfo[] detailLevels;
        public static float maxViewDst;

        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        private Vector2 _viewerPositionOld;
        private static MapGenerator _mapGenerator;
        private int _chunkSize;
        private int _chunksVisibleInViewDst;

        private readonly Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new();
        private static List<TerrainChunk> _terrainChunksVisibleLastUpdate = new();

        private void Start()
        {
            _mapGenerator = FindFirstObjectByType<MapGenerator>();

            maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
            _chunkSize = MapGenerator.MapChunkSize - 1;
            _chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / _chunkSize);

            UpdateVisibleChunks();
        }

        private void Update()
        {
            var position = viewer.position / Scale;
            viewerPosition = new Vector2(position.x, position.z);

            if ((_viewerPositionOld - viewerPosition).sqrMagnitude > SqrViewerMoveThresholdForChunkUpdate)
            {
                _viewerPositionOld = viewerPosition;
                UpdateVisibleChunks();
            }
        }

        private void UpdateVisibleChunks()
        {
            foreach (var t in _terrainChunksVisibleLastUpdate)
                t.SetVisible(false);

            _terrainChunksVisibleLastUpdate.Clear();

            var currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / _chunkSize);
            var currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / _chunkSize);

            for (var yOffset = -_chunksVisibleInViewDst; yOffset <= _chunksVisibleInViewDst; yOffset++)
            for (var xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++)
            {
                var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (_terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    _terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    _terrainChunkDictionary.Add(viewedChunkCoord,
                        new TerrainChunk(viewedChunkCoord, _chunkSize, detailLevels, transform, mapMaterial));
                }
            }
        }

        public class TerrainChunk
        {
            private readonly GameObject _meshObject;
            private readonly Vector2 position;
            private Bounds _bounds;

            private readonly MeshRenderer _meshRenderer;
            private readonly MeshFilter _meshFilter;

            private readonly LODInfo[] _detailLevels;
            private readonly LODMesh[] _lodMeshes;

            private MapData _mapData;
            private bool _mapDataReceived;
            private int _previousLODIndex = -1;

            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
            {
                _detailLevels = detailLevels;

                position = coord * size;
                _bounds = new Bounds(position, Vector2.one * size);
                var positionV3 = new Vector3(position.x, 0, position.y);

                _meshObject = new GameObject("Terrain Chunk");
                _meshRenderer = _meshObject.AddComponent<MeshRenderer>();
                _meshFilter = _meshObject.AddComponent<MeshFilter>();
                _meshRenderer.material = material;

                _meshObject.transform.position = positionV3 * Scale;
                _meshObject.transform.parent = parent;
                _meshObject.transform.localScale = Vector3.one * Scale;
                SetVisible(false);

                _lodMeshes = new LODMesh[detailLevels.Length];
                for (var i = 0; i < detailLevels.Length; i++)
                    _lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);

                _mapGenerator.RequestMapData(position, OnMapDataReceived);
            }

            private void OnMapDataReceived(MapData mapData)
            {
                this._mapData = mapData;
                _mapDataReceived = true;

                Texture2D texture = TextureGenerator.GenerateTexture(mapData.colourMap, MapGenerator.MapChunkSize,
                    MapGenerator.MapChunkSize);
                _meshRenderer.material.mainTexture = texture;

                UpdateTerrainChunk();
            }


            public void UpdateTerrainChunk()
            {
                if (_mapDataReceived)
                {
                    var viewerDstFromNearestEdge = Mathf.Sqrt(_bounds.SqrDistance(viewerPosition));
                    var visible = viewerDstFromNearestEdge <= maxViewDst;

                    if (visible)
                    {
                        var lodIndex = 0;

                        for (var i = 0; i < _detailLevels.Length - 1; i++)
                            if (viewerDstFromNearestEdge > _detailLevels[i].visibleDstThreshold)
                                lodIndex = i + 1;
                            else
                                break;

                        if (lodIndex != _previousLODIndex)
                        {
                            var lodMesh = _lodMeshes[lodIndex];
                            if (lodMesh.hasMesh)
                            {
                                _previousLODIndex = lodIndex;
                                _meshFilter.mesh = lodMesh.mesh;
                            }
                            else if (!lodMesh.hasRequestedMesh)
                            {
                                lodMesh.RequestMesh(_mapData);
                            }
                        }
                        
                        _terrainChunksVisibleLastUpdate.Add(this);
                    }

                    SetVisible(visible);
                }
            }

            public void SetVisible(bool visible)
            {
                _meshObject.SetActive(visible);
            }

            public bool IsVisible()
            {
                return _meshObject.activeSelf;
            }
        }

        private class LODMesh
        {
            public Mesh mesh;
            public bool hasRequestedMesh;
            public bool hasMesh;
            private readonly int lod;
            private readonly Action updateCallback;

            public LODMesh(int lod, Action updateCallback)
            {
                this.lod = lod;
                this.updateCallback = updateCallback;
            }

            private void OnMeshDataReceived(MeshData meshData)
            {
                mesh = meshData.CreateMesh();
                hasMesh = true;

                updateCallback();
            }

            public void RequestMesh(MapData mapData)
            {
                hasRequestedMesh = true;
                _mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
            }
        }

        [Serializable]
        public struct LODInfo
        {
            public int lod;
            public float visibleDstThreshold;
        }
    }
}