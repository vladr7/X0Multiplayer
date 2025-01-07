using System;
using UnityEngine;

public class GridPosition : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    
    private void OnMouseDown()
    {
        Debug.Log("GridPosition: " + x + ", " + y);
        GameManager.Instance.ClickedOnGridPosition(x, y);
    }
}
