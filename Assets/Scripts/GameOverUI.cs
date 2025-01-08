using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;
    [SerializeField] private Button rematchButton;

    private void Awake()
    {
        rematchButton.onClick.AddListener(() =>
        {
            GameManager.Instance.RematchRpc();
            Hide();
        });
    }

    private void Start()
    {
        GameManager.Instance.OnGameWin += OnGameWin;
        GameManager.Instance.OnRematch += OnRematch;
        GameManager.Instance.OnGameTied += OnGameTied;
        Hide();
    }
    
    private void OnGameTied(object sender, EventArgs e)
    {
        resultTextMesh.text = "Draw!";
        resultTextMesh.color = Color.gray;
        Show();
    }
    
    private void OnRematch(object sender, EventArgs e)
    {
        Hide();
    }
    
    private void OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        if (e.winPlayerType == GameManager.PlayerType.None)
        {
            resultTextMesh.text = "Draw!";
            resultTextMesh.color = Color.white;
        }
        else if (e.winPlayerType == GameManager.Instance.LocalPlayerType)
        {
            resultTextMesh.text = "You Win!";
            resultTextMesh.color = winColor;
        }
        else
        {
            resultTextMesh.text = "You Lose!";
            resultTextMesh.color = loseColor;
        }
        
        Show();
    }

    private void Show()
    {
        gameObject.SetActive(true);
    }
    
    private void Hide()
    {
        gameObject.SetActive(false);
    }
}
