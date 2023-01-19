using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.UI;

public class TestHandler : MonoBehaviour
{
    [SerializeField] private Button spawnMariaButton;
    [SerializeField] private Button spawnAbeButton;

    [SerializeField] private List<GameObject> _playerPrefabList;


    private void Awake()
    {
        UnityServices.InitializeAsync();

        spawnMariaButton.onClick.AddListener(() =>
        {
            SpawnCharacterServerRpc(0);
        });
        spawnAbeButton.onClick.AddListener(() =>
        {
            SpawnCharacterServerRpc(1);
        });
    }

    private void Start()
    {
        NetworkManager.Singleton.StartHost();
    }

    [ServerRpc]
    public void SpawnCharacterServerRpc(int charId)
    {
        GameObject newPlayer = Instantiate(_playerPrefabList[charId]);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
    }
}
