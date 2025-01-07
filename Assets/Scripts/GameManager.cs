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
    public PlayerType LocalPlayerType => localPlayerType;
    
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

    private PlayerType currentPlayablePlayerType;
    
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

        if (IsServer)
        {
            currentPlayablePlayerType = PlayerType.Cross;
        }
    }
    
    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType)
        {
            return;
        }
        
        Debug.Log("GameManager: " + x + ", " + y);
        OnClickedOnGridPosition?.Invoke(this, new ClickedOnGridPositionEventArgs { x = x, y = y, playerType = playerType});
        
        currentPlayablePlayerType = currentPlayablePlayerType == PlayerType.Cross ? PlayerType.Circle : PlayerType.Cross;
    }
    
}
