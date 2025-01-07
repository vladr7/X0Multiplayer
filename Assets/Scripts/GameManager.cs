using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public event EventHandler<ClickedOnGridPositionEventArgs> OnClickedOnGridPosition;
    public class ClickedOnGridPositionEventArgs : EventArgs
    {
        public int x;
        public int y;
        public PlayerType playerType;
    }
    
    public enum PlayerType
    {
        None, 
        Cross,
        Circle
    }
    
    private PlayerType localPlayerType;
    
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
    
    public override void OnNetworkSpawn()
    {
        Debug.Log("OnNetworkSpawn + NetworkManager.Singleton.LocalClientId: " + NetworkManager.Singleton.LocalClientId);
        
        if (NetworkManager.Singleton.LocalClientId == 0)
        {
            localPlayerType = PlayerType.Cross;
        }
        else
        {
            localPlayerType = PlayerType.Circle;
        }
    }
    
    public void ClickedOnGridPosition(int x, int y)
    {
        Debug.Log("GameManager: " + x + ", " + y);
        OnClickedOnGridPosition?.Invoke(this, new ClickedOnGridPositionEventArgs { x = x, y = y, playerType = localPlayerType});
    }
}
