using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    private readonly GameManager _gameManager = GameManager.Instance;

    private NetworkManager _networkManager;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _networkManager = NetworkManager.Singleton;

        if (_gameManager.IsHost) _networkManager.StartHost();
        else _networkManager.StartClient();
    }
}
