using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton Pattern
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public enum GameState
    {
        SPLASH_SCREEN = 0,
        LOBBY,
        COUNTDOWN,
        IN_GAME
    }

    public LobbyManager LobbyManager { get; private set; }
    public RelayManager RelayManager { get; private set; }

    public GameState CurrentGameState { get; set; } = GameState.SPLASH_SCREEN;
    public bool IsHost { get; set; }

    public int CharacterOne { get; set; }
    public int CharacterTwo { get; set; }

    private void Start()
    {
        Application.targetFrameRate = 60;

        LobbyManager = gameObject.GetComponent<LobbyManager>();
        RelayManager = gameObject.GetComponent<RelayManager>();
    }
}
