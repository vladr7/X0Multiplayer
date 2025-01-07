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

    public event EventHandler OnGameStarted;
    public event EventHandler OnCurrentPlayablePlayerTypeChanged;

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    private PlayerType localPlayerType;
    public PlayerType LocalPlayerType => localPlayerType;
    private PlayerType[,] playerTypeArray;
    
    private NetworkVariable<PlayerType> currentPlayablePlayerType = new NetworkVariable<PlayerType>(PlayerType.None);

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
        
        playerTypeArray = new PlayerType[3, 3];
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

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        
        currentPlayablePlayerType.OnValueChanged += (previous, current) =>
        {
            OnCurrentPlayablePlayerTypeChanged?.Invoke(this, EventArgs.Empty);
        };
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2)
        {
            currentPlayablePlayerType.Value = PlayerType.Cross;
            TriggerOnGameStartedRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameStartedRpc()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
    }

    [Rpc(SendTo.Server)]
    public void ClickedOnGridPositionRpc(int x, int y, PlayerType playerType)
    {
        if (playerType != currentPlayablePlayerType.Value)
        {
            return;
        }
        
        if(playerTypeArray[x, y] != PlayerType.None)
        {
            // Already occupied
            return;
        }
        
        playerTypeArray[x, y] = playerType;
        

        Debug.Log("GameManager: " + x + ", " + y);
        OnClickedOnGridPosition?.Invoke(this,
            new ClickedOnGridPositionEventArgs { x = x, y = y, playerType = playerType });

        currentPlayablePlayerType.Value =
            currentPlayablePlayerType.Value == PlayerType.Cross ? PlayerType.Circle : PlayerType.Cross;
        
        TestTicTacToeWinner();
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }
    
    private void TestTicTacToeWinner()
    {
        // Horizontal
        for (int y = 0; y < 3; y++)
        {
            if (playerTypeArray[0, y] != PlayerType.None &&
                playerTypeArray[0, y] == playerTypeArray[1, y] &&
                playerTypeArray[0, y] == playerTypeArray[2, y])
            {
                Debug.Log("Winner: " + playerTypeArray[0, y]);
                currentPlayablePlayerType.Value = PlayerType.None;
            }
        }
        
        // Vertical
        for (int x = 0; x < 3; x++)
        {
            if (playerTypeArray[x, 0] != PlayerType.None &&
                playerTypeArray[x, 0] == playerTypeArray[x, 1] &&
                playerTypeArray[x, 0] == playerTypeArray[x, 2])
            {
                Debug.Log("Winner: " + playerTypeArray[x, 0]);
                currentPlayablePlayerType.Value = PlayerType.None;
            }
        }
        
        // Diagonal
        if (playerTypeArray[0, 0] != PlayerType.None &&
            playerTypeArray[0, 0] == playerTypeArray[1, 1] &&
            playerTypeArray[0, 0] == playerTypeArray[2, 2])
        {
            Debug.Log("Winner: " + playerTypeArray[0, 0]);
            currentPlayablePlayerType.Value = PlayerType.None;
        }
        
        if (playerTypeArray[2, 0] != PlayerType.None &&
            playerTypeArray[2, 0] == playerTypeArray[1, 1] &&
            playerTypeArray[2, 0] == playerTypeArray[0, 2])
        {
            Debug.Log("Winner: " + playerTypeArray[2, 0]);
            currentPlayablePlayerType.Value = PlayerType.None;
        }
    }
    
    
}