using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _playerPrefabList;

    public override void OnNetworkSpawn()
    {
        if (GameManager.Instance.IsHost) SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.Instance.CharacterOne - 1);
        else SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.Instance.CharacterTwo - 1);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId, int charId)
    {
        GameObject newPlayer = Instantiate(_playerPrefabList[charId]);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
