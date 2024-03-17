using System;
using System.Collections.Generic;
using UnityEngine;
using WorldGeneration;

public class EndlessTerrain : MonoBehaviour
{
    private const float viewerMoveThresholdForChunkUpdate = 25f;

    private const float sqrViewerMoveThresholdForChunkUpdate =
        viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDst;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    private Vector2 viewerPositionOld;
    private static MapGenerator mapGenerator;
    private int chunkSize;
    private int chunksVisibleInViewDst;

    private readonly Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new();
    private static readonly List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    private void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    private void UpdateVisibleChunks()
    {
        for (var i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        terrainChunksVisibleLastUpdate.Clear();

        var currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        var currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (var yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        for (var xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
        {
            var viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

            if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
            else
                terrainChunkDictionary.Add(viewedChunkCoord,
                    new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
        }
    }

    public class TerrainChunk
    {
        private readonly GameObject meshObject;
        private readonly Vector2 position;
        private Bounds bounds;

        private readonly MeshRenderer meshRenderer;
        private readonly MeshFilter meshFilter;
        private readonly MeshCollider meshCollider;

        private readonly LODInfo[] detailLevels;
        private readonly LODMesh[] lodMeshes;
        private readonly LODMesh collisionLODMesh;

        private MapData mapData;
        private bool mapDataReceived;
        private int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            var positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
            SetVisible(false);

            lodMeshes = new LODMesh[detailLevels.Length];
            for (var i = 0; i < detailLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
                if (detailLevels[i].useForCollider) collisionLODMesh = lodMeshes[i];
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        private void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            var texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize,
                MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }


        public void UpdateTerrainChunk()
        {
            if (mapDataReceived)
            {
                var viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                var visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible)
                {
                    var lodIndex = 0;

                    for (var i = 0; i < detailLevels.Length - 1; i++)
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                            lodIndex = i + 1;
                        else
                            break;

                    if (lodIndex != previousLODIndex)
                    {
                        var lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if (lodIndex == 0)
                    {
                        if (collisionLODMesh.hasMesh)
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        else if (!collisionLODMesh.hasRequestedMesh) collisionLODMesh.RequestMesh(mapData);
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
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
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDstThreshold;
        public bool useForCollider;
    }
}