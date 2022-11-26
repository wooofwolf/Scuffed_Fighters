using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton Pattern
    public static GameManager Instance { get; private set; }

    public enum GameState
    {
        SPLASH_SCREEN = 0,
        LOBBY,
        COUNTDOWN,
        IN_GAME
    }

    public GameState CurrentGameState { get; set; }
    public LocalLobby Lobby { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
}
