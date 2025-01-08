using System;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private Transform placeSfxPrefab;

    private void Start()
    {
        GameManager.Instance.OnPlacedObject += OnPlacedObject;
    }
    
    private void OnPlacedObject(object sender, EventArgs e)
    {
        Transform sfx = Instantiate(placeSfxPrefab, transform.position, Quaternion.identity);
        Destroy(sfx.gameObject, 5f);
    }
}
