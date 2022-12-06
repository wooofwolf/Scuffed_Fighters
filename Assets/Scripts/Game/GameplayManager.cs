using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    private readonly GameManager _gameManager = GameManager.Instance;
    private readonly UnityTransport _unityTransport;

    private NetworkManager _networkManager;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _networkManager = NetworkManager.Singleton;

        if (_gameManager.IsHost)
        {
            StartHost();
        }
        else _networkManager.StartClient();
    }

    private async void StartHost()
    {
        Allocation a = await RelayService.Instance.CreateAllocationAsync(2);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

        _unityTransport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>()
                {
                    {
                        "RelayJoinKey", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: joinCode)
                    }
                };

            await LobbyService.Instance.UpdateLobbyAsync()
        }

        _networkManager.StartHost();
    }
}
