using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_gameManager.IsHost)
        {
            string relayCode = await _gameManager.RelayManager.CreateRelay();
            _gameManager.LobbyManager.SetRelayKey(relayCode);
        }
        else
        {
            StartCoroutine(TryJoiningRelay());
        }
    }

    private IEnumerator TryJoiningRelay()
    {
        var delay = new WaitForSecondsRealtime(1);
        string relayCode = "0";

        while (relayCode == "0")
        {
            relayCode = _gameManager.LobbyManager.GetRelayKey();

            yield return delay;
        }

        _gameManager.RelayManager.JoinRelay(relayCode);
    }
}
