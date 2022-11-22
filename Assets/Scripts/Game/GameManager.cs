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
        MAIN_MENU,
        LOBBY,
        IN_GAME
    }

    private GameState _gameState;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void SetGameState(GameState state)
    {
        _gameState = state;
    }
}
