using System;
using UnityEngine;

public class GameVisualManager : MonoBehaviour
{

    private const float GRID_SIZE = 3.1f;
    
    [SerializeField] private Transform crossPrefab;
    [SerializeField] private Transform circlePrefab;

    private void Start()
    {
        GameManager.Instance.OnClickedOnGridPosition += OnClickedOnGridPosition;
    }
    
    private void OnClickedOnGridPosition(object sender, GameManager.ClickedOnGridPositionEventArgs e)
    {
        Instantiate(crossPrefab, GetGridWorldPosition(e.x, e.y), Quaternion.identity);
    }
    
    private Vector2 GetGridWorldPosition(int x, int y)
    {
        return new Vector2(-GRID_SIZE + x * GRID_SIZE, -GRID_SIZE + y * GRID_SIZE);
    }
    
}
