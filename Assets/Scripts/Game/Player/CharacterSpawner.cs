using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private List<GameObject> _playerPrefabList;

    public override void OnNetworkSpawn()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, GameManager.Instance.GetCharactersId(NetworkManager.Singleton.LocalClientId) - 1);
        //if (!IsServer) return;

        //Dictionary<ulong, int> characters = GameManager.Instance.GetCharacters();

        //foreach (var character in characters)
        //{
        //    GameObject newPlayer = Instantiate(_playerPrefabList[character.Value - 1]);
        //    NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        //    newPlayer.SetActive(true);
        //    netObj.SpawnAsPlayerObject(character.Key, true);
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    public void SpawnPlayerServerRpc(ulong clientId, int charId)
    {
        Debug.Log(clientId + " is spawning player");

        GameObject newPlayer = Instantiate(_playerPrefabList[charId]);
        NetworkObject netObj = newPlayer.GetComponent<NetworkObject>();
        newPlayer.SetActive(true);
        netObj.SpawnAsPlayerObject(clientId, true);
    }
}
