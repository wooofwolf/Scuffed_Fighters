using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private LocalLobby _localLobby;

    private IEnumerator _heartbeatCoroutine;
    private IEnumerator _pollLobbyCoroutine;

    private readonly string _lobbyName = Guid.NewGuid().ToString();
    private readonly int _rateLimitGetLobby = 1;

    private void Start()
    {
        // Auto host a lobby
        HostLobby();
    }

    private async void HostLobby()
    {
        try
        {
            CreateLobbyOptions lobbyOptions = new()
            {
                IsPrivate = false,
                Player = new (
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>()
                    {
                        {
                            "Ready", new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: "false")
                        }
                    }),
                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "RelayJoinKey", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: "")
                    }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, 2, lobbyOptions);
            Debug.Log("Lobby successfully created!");

            _heartbeatCoroutine = HeartbeatLobbyCoroutine(lobby.Id, 15);
            StartCoroutine(_heartbeatCoroutine);
            Debug.Log("Started heartbeat lobby coroutine.");

            _localLobby.InitLobby(lobby);

            _pollLobbyCoroutine = PollLobbyCoroutine(_rateLimitGetLobby);
            StartCoroutine(_pollLobbyCoroutine);
            Debug.Log("Started poll lobby coroutine.");
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
            // TODO: Check if player is owner of the lobby before deleting it, else leave lobby

            // Delete current lobby
            await LobbyService.Instance.DeleteLobbyAsync(_localLobby.LobbyId);
            Debug.Log("Current lobby deleted.");

            StopCoroutine(_heartbeatCoroutine);
            Debug.Log("Stopped heartbeat coroutine.");

            // Join existing lobby
            JoinLobbyByCodeOptions lobbyOptions = new()
            {
                Player = new (
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>()
                    {
                        {
                            "Ready", new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: "false")
                        }
                    })
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(_codeInput.text, lobbyOptions);
            Debug.Log("Lobby joined!");

            _localLobby.InitLobby(lobby);

            StopCoroutine(_pollLobbyCoroutine);
            await Task.Delay(_rateLimitGetLobby * 1000);

            _pollLobbyCoroutine = PollLobbyCoroutine(_rateLimitGetLobby);
            StartCoroutine(_pollLobbyCoroutine);
            Debug.Log("Started poll lobby coroutine.");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    IEnumerator PollLobbyCoroutine(float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            var t = Task.Run(async () => await LobbyService.Instance.GetLobbyAsync(_localLobby.LobbyId));
            yield return new WaitUntil(() => t.IsCompleted);
            Debug.Log("Lobby task complete.");

            Lobby updatedLobby = t.Result;
            _localLobby.UpdateLobby(updatedLobby);
            yield return delay;
        }
    }

    public async void GetReady()
    {
        try
        {
            UpdatePlayerOptions options = new()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    {
                        "Ready", new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: "true")
                    }
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(_localLobby.LobbyId, AuthenticationService.Instance.PlayerId, options);
            _localLobby.GetReady();
            Debug.Log("Player's ready state set to true.");
        }
        catch (LobbyServiceException e)
        { 
            Debug.LogError(e);
        }
    }
}
