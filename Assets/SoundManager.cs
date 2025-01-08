using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSfxPrefab;
    [SerializeField] private Transform winSfxPrefab;
    [SerializeField] private Transform loseSfxPrefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += OnPlacedObject;
        GameManager.Instance.OnGameWin += OnGameWin;
    }

    private void OnGameWin(object sender, GameManager.OnGameWinEventArgs e)
    {
        Transform sfx =
            Instantiate(e.winPlayerType == GameManager.Instance.LocalPlayerType ? winSfxPrefab : loseSfxPrefab,
                transform.position, Quaternion.identity);
        Destroy(sfx.gameObject, 5f);
    }

    private void OnPlacedObject(object sender, EventArgs e)
    {
        Transform sfx = Instantiate(placeSfxPrefab, transform.position, Quaternion.identity);
        Destroy(sfx.gameObject, 5f);
    }
}