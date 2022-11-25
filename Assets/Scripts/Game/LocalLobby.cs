using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

[System.Serializable]
public class LocalLobby : MonoBehaviour
{
    public enum LobbyState
    {
        LOBBY = 0,
        COUNTDOWN,
        IN_GAME
    }

    [SerializeField] private TextMeshProUGUI _lobbyCodeUI;

    public LobbyState CurrentLobbyState { get; set; }
    public string LobbyId { get; set; }
    public string LobbyCode { get; set; }
    public string HostId { get; set; }
    public int AvailableSlots { get; set; }

    private const int MAX_PLAYER_COUNT = 2;

    public void InitLobby(Lobby lobby)
    {
        CurrentLobbyState = LobbyState.LOBBY;
        LobbyId = lobby.Id;
        LobbyCode = lobby.LobbyCode;
        HostId = lobby.HostId;
        AvailableSlots = lobby.AvailableSlots;
        Debug.Log("Local lobby initialized!");

        UpdateGUI();
    }

    private void UpdateGUI()
    {
        StringBuilder sb = new("Lobby Code: ", 18);
        sb.Append(LobbyCode);
        _lobbyCodeUI.SetText(sb);
        Debug.Log("Local lobby GUI updated!");
    }

    public void UpdateLobby(Lobby lobby)
    {
        if (lobby.Id != LobbyId) LobbyId = lobby.Id;
        if (lobby.LobbyCode != LobbyCode) LobbyCode = lobby.LobbyCode;
        if (lobby.HostId != HostId) HostId = lobby.HostId;
        if (lobby.AvailableSlots != AvailableSlots) AvailableSlots = lobby.AvailableSlots;

        Debug.Log("Lobby updated.");
    }
}
