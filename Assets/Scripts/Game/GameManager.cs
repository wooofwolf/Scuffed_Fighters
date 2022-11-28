using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public bool IsHost { get; set; }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
}
