using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.Netcode;

public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance { get; internal set; }

    [HideInInspector]
    public delegate void ClientLoadedSceneDelegateHandler(ulong clientId);
    [HideInInspector]
    public event ClientLoadedSceneDelegateHandler OnClientLoadedScene;
    [HideInInspector]
    public delegate void SceneStateChangedDelegateHandler(SceneStates newState);
    [HideInInspector]
    public event SceneStateChangedDelegateHandler OnSceneStateChanged;

    public enum SceneStates
    {
        SPLASH_SCREEN = 0,
        LOBBY,
        IN_GAME
    }

    private SceneStates _sceneState;
    private int _clientsLoaded;

    private void Awake()
    {
        if (Instance != this && Instance != null) Destroy(Instance.gameObject);

        Instance = this;
        SetSceneState(SceneStates.SPLASH_SCREEN);
        DontDestroyOnLoad(this);
    }

    public void SetSceneState(SceneStates sceneState)
    {
        _sceneState = sceneState;

        if (OnSceneStateChanged != null) OnSceneStateChanged.Invoke(_sceneState);
    }

    public SceneStates GetCurrentSceneState()
    {
        return _sceneState;
    }

    public void RegisterCallbacks()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnLoadComplete;
    }

    public void SwitchScene(string sceneName, SceneStates sceneState)
    {
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.Log("NetworkManager is listening");
            _clientsLoaded = 0;
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else SceneManager.LoadSceneAsync(sceneName);

        _sceneState = sceneState;
        Debug.Log("Scene successfully switched!");
    }

    private void OnLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        _clientsLoaded += 1;
        OnClientLoadedScene?.Invoke(clientId);
    }

    public bool AllClientsAreLoaded()
    {
        return _clientsLoaded == NetworkManager.Singleton.ConnectedClients.Count;
    }
}
