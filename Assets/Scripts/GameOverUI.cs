using System;
using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultTextMesh;
    [SerializeField] private Color winColor;
    [SerializeField] private Color loseColor;

    private void Start()
    {
        GameManager.Instance.OnGameWin += OnGameWin;
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
