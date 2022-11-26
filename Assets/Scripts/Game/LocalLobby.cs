using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class LocalLobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyCodeUI;
    [SerializeField] private Button readyButtonUI;
    [SerializeField] private TextMeshProUGUI _playersReadyUI;

    public string LobbyId { get; set; }
    public string LobbyCode { get; set; }
    public string HostId { get; set; }
    public int AvailableSlots { get; set; }

    public bool IsPlayerReady { get; set; }

    public int PlayersReady { get; set; }

    private const int MAX_PLAYER_COUNT = 2;

    private readonly GameManager gameManager = GameManager.Instance;

    public void InitLobby(Lobby lobby)
    {
        gameManager.Lobby = this;

        LobbyId = lobby.Id;
        LobbyCode = lobby.LobbyCode;
        HostId = lobby.HostId;
        AvailableSlots = lobby.AvailableSlots;
        IsPlayerReady = false;
        PlayersReady = 0;
        Debug.Log("Local lobby initialized!");

        UpdateGUI();
    }

    private void UpdateGUI()
    {
        StringBuilder sb = new("Lobby Code: ", 18);
        sb.Append(LobbyCode);
        _lobbyCodeUI.SetText(sb);
        Debug.Log("Lobby code updated!");

        if (IsPlayerReady) readyButtonUI.gameObject.SetActive(false);
        else readyButtonUI.gameObject.SetActive(true);
        Debug.Log("Ready button updated!");

        sb = new("/ 2", 5);
        sb.Insert(0, PlayersReady + " ");
        _playersReadyUI.SetText(sb);
        Debug.Log("Players ready updated!");

        Debug.Log("Local lobby GUI updated!");
    }

    public void UpdateLobby(Lobby lobby)
    {
        if (lobby.Id != LobbyId) LobbyId = lobby.Id;
        if (lobby.LobbyCode != LobbyCode) LobbyCode = lobby.LobbyCode;
        if (lobby.HostId != HostId) HostId = lobby.HostId;
        if (lobby.AvailableSlots != AvailableSlots) AvailableSlots = lobby.AvailableSlots;
        
        foreach (Player p in lobby.Players)
        {
            if (p.Id == AuthenticationService.Instance.PlayerId) continue;

            p.Data.TryGetValue("Ready", out var playerData);
            if (playerData.Value.Equals("true"))
            {
                if (IsPlayerReady && gameManager.CurrentGameState == GameManager.GameState.LOBBY)
                {
                    PlayersReady = MAX_PLAYER_COUNT;
                    gameManager.CurrentGameState = GameManager.GameState.COUNTDOWN;
                    StartCoroutine(Countdown(5));
                }
                else PlayersReady = 1;

                if (gameManager.CurrentGameState == GameManager.GameState.LOBBY) UpdateGUI();
            }
        }

        Debug.Log("Lobby updated.");
    }

    public void GetReady()
    {
        IsPlayerReady = true;
        ++PlayersReady;
        UpdateGUI();
    }

    private IEnumerator Countdown(float duration)
    {
        float counter = duration;
        while (counter > 0)
        {
            yield return new WaitForSecondsRealtime(1);

            StringBuilder sb = new("Starting in ", 13);
            sb.Append(counter);
            _playersReadyUI.SetText(sb);
            Debug.Log("Timer updated!");

            --counter;
        }

        gameManager.CurrentGameState = GameManager.GameState.IN_GAME;
        SceneManager.LoadSceneAsync(2);
    }
}
