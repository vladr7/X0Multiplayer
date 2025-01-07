using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public event EventHandler<ClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class ClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
    }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.Log("GameManager: " + x + ", " + y);
        OnClickedOnGridPosition?.Invoke(this, new ClickedOnGridPositionEventArgs { x = x, y = y });
    }
}
