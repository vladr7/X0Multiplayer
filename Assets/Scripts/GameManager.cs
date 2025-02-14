using System;
using System.Collections.Generic;
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
    public event EventHandler<OnGameWinEventArgs> OnGameWin;
    public event EventHandler OnRematch;
    public event EventHandler OnGameTied;
    public event EventHandler OnScoreChanged;
    public event EventHandler OnPlacedObject;

    public class OnGameWinEventArgs : EventArgs
    {
        public Line line;
        public PlayerType winPlayerType;
    }

    public enum PlayerType
    {
        None,
        Cross,
        Circle
    }

    public enum Orientation
    {
        Horizontal,
        Vertical,
        DiagonalA,
        DiagonalB
    }


    public struct Line
    {
        public List<Vector2Int> gridVector2IntList;
        public Vector2Int centerGridPosition;
        public Orientation orientation;
    }

    private PlayerType localPlayerType;
    public PlayerType LocalPlayerType => localPlayerType;
    private PlayerType[,] playerTypeArray;
    private List<Line> lineList;
    
    private NetworkVariable<int> playerCrossScore = new NetworkVariable<int>(0);
    private NetworkVariable<int> playerCircleScore = new NetworkVariable<int>(0);

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

        lineList = new List<Line>
        {
            // Horizontal lines
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(2, 0)
                },
                centerGridPosition = new Vector2Int(1, 0),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 1),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 1)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Horizontal
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 2),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 2),
                orientation = Orientation.Horizontal
            },
            // Vertical lines
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, 2)
                },
                centerGridPosition = new Vector2Int(0, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(1, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(1, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.Vertical
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(2, 0),
                    new Vector2Int(2, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(2, 1),
                orientation = Orientation.Vertical
            },
            // Diagonal lines
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 0),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 2)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalA
            },
            new Line
            {
                gridVector2IntList = new List<Vector2Int>
                {
                    new Vector2Int(0, 2),
                    new Vector2Int(1, 1),
                    new Vector2Int(2, 0)
                },
                centerGridPosition = new Vector2Int(1, 1),
                orientation = Orientation.DiagonalB
            }
        };
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
        
        playerCrossScore.OnValueChanged += (previous, current) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
        };
        
        playerCircleScore.OnValueChanged += (previous, current) =>
        {
            OnScoreChanged?.Invoke(this, EventArgs.Empty);
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

        if (playerTypeArray[x, y] != PlayerType.None)
        {
            // Already occupied
            return;
        }

        playerTypeArray[x, y] = playerType;
        TriggerOnPlacedObjectRpc();
        
        Debug.Log("GameManager: " + x + ", " + y);
        OnClickedOnGridPosition?.Invoke(this,
            new ClickedOnGridPositionEventArgs { x = x, y = y, playerType = playerType });

        currentPlayablePlayerType.Value =
            currentPlayablePlayerType.Value == PlayerType.Cross ? PlayerType.Circle : PlayerType.Cross;

        TestWinner();
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnPlacedObjectRpc()
    {
        OnPlacedObject?.Invoke(this, EventArgs.Empty);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameWinRpc(int lineIndex, PlayerType winPlayerType)
    {
        Line line = lineList[lineIndex];
        OnGameWin?.Invoke(this, new OnGameWinEventArgs
        {
            line = line,
            winPlayerType = winPlayerType
        });
    }

    public PlayerType GetCurrentPlayablePlayerType()
    {
        return currentPlayablePlayerType.Value;
    }

    private bool TestWinnerLine(Line line)
    {
        return TestWinnerLine
        (playerTypeArray[line.gridVector2IntList[0].x, line.gridVector2IntList[0].y],
            playerTypeArray[line.gridVector2IntList[1].x, line.gridVector2IntList[1].y],
            playerTypeArray[line.gridVector2IntList[2].x, line.gridVector2IntList[2].y]
        );
    }

    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType)
    {
        return aPlayerType != PlayerType.None && aPlayerType == bPlayerType && aPlayerType == cPlayerType;
    }

    private void TestWinner()
    {
        for (int i = 0; i < lineList.Count; i++)
        {
            Line line = lineList[i];
            if (TestWinnerLine(line))
            {
                Debug.Log("Winner: " + i);
                currentPlayablePlayerType.Value = PlayerType.None;
                PlayerType winPlayerType = playerTypeArray[line.centerGridPosition.x, line.centerGridPosition.y];
                if (winPlayerType == PlayerType.Cross)
                {
                    playerCrossScore.Value++;
                }
                else
                {
                    playerCircleScore.Value++;
                }
                TriggerOnGameWinRpc(i, winPlayerType);
                return;
            }
        }
        
        bool hasTie = true;
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                if (playerTypeArray[x, y] == PlayerType.None)
                {
                    hasTie = false;
                    break;
                }
            }
        }
        
        if (hasTie)
        {
            TriggerOnGameTiedRpc();
        }
    }
    
    [Rpc(SendTo.Server)]
    public void RematchRpc()
    {
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                playerTypeArray[x, y] = PlayerType.None;
            }
        }

        currentPlayablePlayerType.Value = PlayerType.Cross;
        TriggerOnRematchRpc();
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnRematchRpc()
    {
        OnRematch?.Invoke(this, EventArgs.Empty);
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    private void TriggerOnGameTiedRpc()
    {
        OnGameTied?.Invoke(this, EventArgs.Empty);
    }
    
    public void GetScores(out int crossScore, out int circleScore)
    {
        crossScore = playerCrossScore.Value;
        circleScore = playerCircleScore.Value;
    }
}