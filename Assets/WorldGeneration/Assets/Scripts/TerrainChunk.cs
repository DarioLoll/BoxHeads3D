using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using WorldGeneration.Assets.Scripts;
using Random = UnityEngine.Random;

public class TerrainChunk {
	
	const float colliderGenerationDistanceThreshold = 5;
	public event System.Action<TerrainChunk, bool> onVisibilityChanged;
	public Vector2 coord;
	 
	GameObject meshObject;
	Vector2 sampleCentre;
	Bounds bounds;

	MeshRenderer meshRenderer;
	MeshFilter meshFilter;
	MeshCollider meshCollider;

	LODInfo[] detailLevels;
	LODMesh[] lodMeshes;
	int colliderLODIndex;

	HeightMap heightMap;
	public bool HeightMapReceived { get; private set; }
	public int PreviousLODIndex { get; private set; }= -1;
	public bool HasSetCollider { get; private set; }
	float maxViewDst;

	HeightMapSettings heightMapSettings;
	MeshSettings meshSettings;
	Transform viewer;
	public List<List<Matrix4x4>> grassMatrices = new();

	[CanBeNull] public event Action GeneratedCollider;


	public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material) {
		this.coord = coord;
		this.detailLevels = detailLevels;
		this.colliderLODIndex = colliderLODIndex;
		this.heightMapSettings = heightMapSettings;
		this.meshSettings = meshSettings;
		this.viewer = viewer;

		sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
		Vector2 position = coord * meshSettings.meshWorldSize ;
		bounds = new Bounds(position,Vector2.one * meshSettings.meshWorldSize );


		meshObject = new GameObject("Terrain Chunk");
		meshRenderer = meshObject.AddComponent<MeshRenderer>();
		meshFilter = meshObject.AddComponent<MeshFilter>();
		meshCollider = meshObject.AddComponent<MeshCollider>();
		meshRenderer.material = material;

		meshObject.transform.position = new Vector3(position.x,0,position.y);
		meshObject.transform.parent = parent;
		SetVisible(false);

		lodMeshes = new LODMesh[detailLevels.Length];
		for (int i = 0; i < detailLevels.Length; i++) {
			lodMeshes[i] = new LODMesh(detailLevels[i].lod);
			lodMeshes[i].updateCallback += UpdateTerrainChunk;
			if (i == colliderLODIndex) {
				lodMeshes[i].updateCallback += UpdateCollisionMesh;
			}
		}

		maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;

	}

	public void Load() {
		ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap (meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
	}
	
	
	public void SetViewer(Transform newViewer) {
		this.viewer = newViewer;
		UpdateTerrainChunk ();
	}



	void OnHeightMapReceived(object heightMapObject) {
		this.heightMap = (HeightMap)heightMapObject;
		HeightMapReceived = true;
		UpdateTerrainChunk ();
	}

	Vector2 viewerPosition => new(viewer.position.x, viewer.position.z);


	public void UpdateTerrainChunk() {
		if (HeightMapReceived) {
			float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));

			bool wasVisible = IsVisible ();
			bool visible = viewerDstFromNearestEdge <= maxViewDst;

			if (visible) {
				int lodIndex = 0;

				for (int i = 0; i < detailLevels.Length - 1; i++) {
					if (viewerDstFromNearestEdge > detailLevels [i].visibleDstThreshold) {
						lodIndex = i + 1;
					} else {
						break;
					}
				}

				if (lodIndex != PreviousLODIndex) {
					LODMesh lodMesh = lodMeshes [lodIndex];
					if (lodMesh.hasMesh) {
						PreviousLODIndex = lodIndex;
						meshFilter.mesh = lodMesh.mesh;
					} else if (!lodMesh.hasRequestedMesh) {
						lodMesh.RequestMesh (heightMap, meshSettings);
					}
				}

				

			}

			if (wasVisible != visible) {
				
				SetVisible (visible);
				if (onVisibilityChanged != null) {
					onVisibilityChanged (this, visible);
				}
			}
		}
	}

	public void UpdateGrass()
	{
		return;
		if(!IsVisible()) return;
		if (HasSetCollider)
		{
			for (int i = 0; i < grassMatrices.Count; i++)
			{
				Graphics.DrawMeshInstanced(GrassSpawner.Instance.grassMeshes[0], 0, 
					GrassSpawner.Instance.grassMaterials[0], grassMatrices[i]);
			}
		}
	}

	public void UpdateCollisionMesh() {
		if (!HasSetCollider) {
			float sqrDstFromViewerToEdge = bounds.SqrDistance (viewerPosition);

			if (sqrDstFromViewerToEdge < detailLevels [colliderLODIndex].sqrVisibleDstThreshold) {
				if (!lodMeshes [colliderLODIndex].hasRequestedMesh) {
					lodMeshes [colliderLODIndex].RequestMesh (heightMap, meshSettings);
				}
			}

			if (lodMeshes [colliderLODIndex].hasMesh) {
				meshCollider.sharedMesh = lodMeshes [colliderLODIndex].mesh;
				HasSetCollider = true;
				ObjectSpawner.Instance.GenerateOnChunk(meshCollider.bounds.min, meshCollider.bounds.max, coord);
				GenerateGrass();
			}
			GeneratedCollider?.Invoke();
		}
	}
	
	public void GenerateGrass()
	{
		return;
		int currentMatrixListIndex = 0;
		grassMatrices.Add(new List<Matrix4x4>());
		for (var i = 0; i < GrassSpawner.Instance.grass.density; i++)
		{
			var colliderBounds = meshCollider.bounds;
			var sampleX = Random.Range(colliderBounds.min.x, colliderBounds.max.x);
			var sampleY = Random.Range(colliderBounds.min.z, colliderBounds.max.z);
			var rayStart = new Vector3(sampleX, GrassSpawner.Instance.grass.maxHeight, sampleY);

			if (!Physics.Raycast(rayStart, Vector3.down, out var hit, Mathf.Infinity))
				continue;

			if (hit.point.y < GrassSpawner.Instance.grass.minHeight)
				continue;

			if (grassMatrices[currentMatrixListIndex].Count >= 1000)
			{
				currentMatrixListIndex++;
				grassMatrices.Add(new List<Matrix4x4>());
			}
			grassMatrices[currentMatrixListIndex].Add(Matrix4x4.TRS(hit.point, Quaternion.identity, new Vector3(8,3,8)));
		}
	}
	

	public void SetVisible(bool visible) {
		meshObject.SetActive (visible);
	}

	public bool IsVisible() {
		return meshObject.activeSelf;
	}

}

class LODMesh {

	public Mesh mesh;
	public bool hasRequestedMesh;
	public bool hasMesh;
	int lod;
	public event System.Action updateCallback;

	public LODMesh(int lod) {
		this.lod = lod;
	}

	void OnMeshDataReceived(object meshDataObject) {
		mesh = ((MeshData)meshDataObject).CreateMesh ();
		hasMesh = true;

		updateCallback ();
	}

	public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings) {
		hasRequestedMesh = true;
		ThreadedDataRequester.RequestData (() => MeshGenerator.GenerateTerrainMesh (heightMap.values, meshSettings, lod), OnMeshDataReceived);
	}

}