using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;

public class TerrainGenerator : MonoBehaviour {

	const float viewerMoveThresholdForChunkUpdate = 25f;
	const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;


	public int colliderLODIndex;
	public LODInfo[] detailLevels;

	public MeshSettings meshSettings;
	public HeightMapSettings heightMapSettings;
	public TextureData textureSettings;

	private Transform _viewer;
	private bool _hasSetViewer;
	public Material mapMaterial;

	Vector2 _viewerPosition;
	Vector2 _viewerPositionOld;

	float _meshWorldSize;
	int _chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

	void Start() {

		textureSettings.ApplyToMaterial (mapMaterial);
		textureSettings.UpdateMeshHeights (mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

		float maxViewDst = detailLevels [detailLevels.Length - 1].visibleDstThreshold;
		_meshWorldSize = meshSettings.meshWorldSize;
		_chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / _meshWorldSize);

		MultiplayerTest.Instance.OnThisPlayerSpawned += OnPlayerSpawned;
		//UpdateVisibleChunks ();
	}

	private void OnPlayerSpawned(Transform obj)
	{
		_viewer = obj;
		_hasSetViewer = true;
		_viewerPosition = new Vector2 (_viewer.position.x, _viewer.position.z);
		_viewerPositionOld = _viewerPosition;
		foreach (var chunk in terrainChunkDictionary.Values)
		{
			chunk.SetViewer(_viewer);
		}
		UpdateVisibleChunks();
	}

	void Update() {
		if(!_hasSetViewer) return;
		_viewerPosition = new Vector2 (_viewer.position.x, _viewer.position.z);
		visibleTerrainChunks.ForEach (chunk => chunk.UpdateGrass());

		if ((_viewerPositionOld - _viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
			_viewerPositionOld = _viewerPosition;
			UpdateVisibleChunks ();
		}
	}
		
	void UpdateVisibleChunks() {
		HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2> ();
		for (int i = visibleTerrainChunks.Count-1; i >= 0; i--) {
			alreadyUpdatedChunkCoords.Add (visibleTerrainChunks [i].coord);
			visibleTerrainChunks [i].UpdateTerrainChunk ();
		}
			
		int currentChunkCoordX = Mathf.RoundToInt (_viewerPosition.x / _meshWorldSize);
		int currentChunkCoordY = Mathf.RoundToInt (_viewerPosition.y / _meshWorldSize);

		for (int yOffset = -_chunksVisibleInViewDst; yOffset <= _chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -_chunksVisibleInViewDst; xOffset <= _chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
				if (!alreadyUpdatedChunkCoords.Contains (viewedChunkCoord)) {
					if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
						terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
					} else {
						TerrainChunk newChunk = new TerrainChunk (viewedChunkCoord,heightMapSettings,meshSettings, detailLevels, colliderLODIndex, transform, _viewer, mapMaterial);
						terrainChunkDictionary.Add (viewedChunkCoord, newChunk);
						newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
						newChunk.Load ();
					}
				}

			}
		}
	}

	void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible) {
		if (isVisible) {
			visibleTerrainChunks.Add (chunk);
		} else {
			visibleTerrainChunks.Remove (chunk);
		}
	}

}

[System.Serializable]
public struct LODInfo {
	[Range(0,MeshSettings.numSupportedLODs-1)]
	public int lod;
	public float visibleDstThreshold;


	public float sqrVisibleDstThreshold {
		get {
			return visibleDstThreshold * visibleDstThreshold;
		}
	}
}
