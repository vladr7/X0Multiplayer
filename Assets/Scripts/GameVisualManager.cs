using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameVisualManager : NetworkBehaviour
{
    private const float GRID_SIZE = 3.1f;

    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;
    [SerializeField] private Transform lineCompletePrefab;
    
    private List<GameObject> visualGameObjectList = new List<GameObject>();

    private void Awake()
    {
        visualGameObjectList = new List<GameObject>();
    }

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += OnClickedOnGridPosition;
        GameManager.Instance.OnGameWin += OnGameWin;
        GameManager.Instance.OnRematch += OnRematch;
    }
    
    private void OnRematch(object sender, EventArgs e)
    {
        if(!IsServer)
        {
            return;
        }
        
        foreach (GameObject visualGameObject in visualGameObjectList)
        {
            Destroy(visualGameObject);
        }
        
        visualGameObjectList.Clear();
    }

    private void OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if(!IsServer)
        {
            return;
        }
        
        float eulerZ = 0;
        switch (e.line.orientation)
        {
            case GameManager.Orientation.Horizontal:
                eulerZ = 0f;
                break;
            case GameManager.Orientation.Vertical:
                eulerZ = 90f;
                break;
            case GameManager.Orientation.DiagonalA:
                eulerZ = 45f;
                break;
            case GameManager.Orientation.DiagonalB:
                eulerZ = -45f;
                break;
        }

        Transform spawnedLineCompleteTransform = Instantiate(lineCompletePrefab,
            GetGridWorldPosition(e.line.centerGridPosition.x, e.line.centerGridPosition.y),
            Quaternion.Euler(0, 0, eulerZ));
        spawnedLineCompleteTransform.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
        
        visualGameObjectList.Add(spawnedLineCompleteTransform.gameObject);
    }

    private void OnClickedOnGridPosition(object sender, GameManager.ClickedOnGridPositionEventArgs e)
    {
        SpawnObjectRpc(e.x, e.y, e.playerType);
    }

    [Rpc(SendTo.Server)]
    private void SpawnObjectRpc(int x, int y, GameManager.PlayerType playerType)
    {
        Debug.Log("SpawnObject: " + x + ", " + y);
        Transform prefab = playerType == GameManager.PlayerType.Cross ? crossPrefab : circlePrefab;

        Transform spawnedCrossTransform = Instantiate(prefab, GetGridWorldPosition(x, y), Quaternion.identity);
        spawnedCrossTransform.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
        
        visualGameObjectList.Add(spawnedCrossTransform.gameObject);
    }

    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
}