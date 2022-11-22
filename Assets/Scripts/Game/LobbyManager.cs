using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyCode;
    [SerializeField] private TextMeshProUGUI _codeInput;

    private string _playerID;

    private void Start()
    {
        _playerID = AuthenticationService.Instance.PlayerId;
    }

    public async void HostLobby()
    {
        try
        {
            CreateLobbyOptions lobbyOptions = new()
            {
                IsPrivate = false
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_playerID, 2, lobbyOptions);
            Debug.Log("Lobby successfully created!");

            StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
            Debug.Log("Started heartbeat lobby coroutine.");

            StringBuilder sb = new("Lobby Code: ", 18);
            sb.Append(lobby.LobbyCode);
            _lobbyCode.SetText(sb.ToString());
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async void JoinLobby()
    {
        try
        {
            Debug.Log(_codeInput.text);
            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(_codeInput.text);
            Debug.Log("Lobby joined!");

            StringBuilder sb = new("Lobby Code: ", 18);
            sb.Append(lobby.LobbyCode);
            _lobbyCode.SetText(sb.ToString());
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }
}
