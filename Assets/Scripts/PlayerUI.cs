using System;
using TMPro;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] private GameObject crossArrowGameObject;
    [SerializeField] private GameObject circleArrowGameObject;
    [SerializeField] private GameObject crossYouTextGameObject;
    [SerializeField] private GameObject circleYouTextGameObject;
    [SerializeField] private TextMeshProUGUI playerCrossScoreText;
    [SerializeField] private TextMeshProUGUI playerCircleScoreText;

    private void Awake()
    {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossYouTextGameObject.SetActive(false);
        circleYouTextGameObject.SetActive(false);

        playerCrossScoreText.text = "";
        playerCircleScoreText.text = "";
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += OnGameStarted;
        GameManager.Instance.OnCurrentPlayablePlayerTypeChanged += OnCurrentPlayablePlayerTypeChanged;
        GameManager.Instance.OnScoreChanged += OnScoreChanged;
    }

    private void OnScoreChanged(object sender, EventArgs e)
    {
        GameManager.Instance.GetScores(out int playerCrossScore, out int playerCircleScore);

        playerCrossScoreText.text = playerCrossScore.ToString();
        playerCircleScoreText.text = playerCircleScore.ToString();
    }

    private void OnCurrentPlayablePlayerTypeChanged(object sender, EventArgs e)
    {
        UpdateCurrentArrow();
    }

    private void OnGameStarted(object sender, EventArgs e)
    {
        if (GameManager.Instance.LocalPlayerType == GameManager.PlayerType.Cross)
        {
            crossYouTextGameObject.SetActive(true);
        }
        else
        {
            circleYouTextGameObject.SetActive(true);
        }

        playerCrossScoreText.text = "0";
        playerCircleScoreText.text = "0";

        UpdateCurrentArrow();
    }

    private void UpdateCurrentArrow()
    {
        if (GameManager.Instance.GetCurrentPlayablePlayerType() == GameManager.PlayerType.Cross)
        {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        }
        else
        {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(true);
        }
    }
}