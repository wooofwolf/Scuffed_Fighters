using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public Lobby CurrentLobby { get; private set; }

    private IEnumerator _heartbeatCoroutine;
    private IEnumerator _pollLobbyCoroutine;

    private readonly string _lobbyName = Guid.NewGuid().ToString();
    private readonly int _rateLimitGetLobby = 1;

    public async Task<Lobby> HostLobby()
    {
        try
        {
            CreateLobbyOptions lobbyOptions = new()
            {
                IsPrivate = false,
                Player = new(
                    id: AuthenticationService.Instance.PlayerId,
                    data: new Dictionary<string, PlayerDataObject>()
                    {
                        {
                            "Ready", new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: "false")
                        },
                        {
                            "Character", new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: "0")
                        }
                    }),
                Data = new Dictionary<string, DataObject>
                {
                    {
                        "RelayKey", new DataObject(DataObject.VisibilityOptions.Member, "0")
                    }
                }
            };

            CurrentLobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, 2, lobbyOptions);
            Debug.Log("Lobby successfully created!");

            _heartbeatCoroutine = HeartbeatLobbyCoroutine(CurrentLobby.Id, 15);
            StartCoroutine(_heartbeatCoroutine);
            Debug.Log("Started heartbeat lobby coroutine.");

            _pollLobbyCoroutine = PollLobbyCoroutine(_rateLimitGetLobby);
            StartCoroutine(_pollLobbyCoroutine);
            Debug.Log("Started poll lobby coroutine.");

            return CurrentLobby;
        }
        catch (Exception e)
        {
            Debug.LogException(e);

            return null;
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

    public async Task<Lobby> JoinLobby(string lobbyCode)
    {
        try
        {
            // TODO: Check if player is owner of the lobby before deleting it, else leave lobby

            // Delete current lobby
            await LobbyService.Instance.DeleteLobbyAsync(CurrentLobby.Id);
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
                        },
                        {
                            "Character", new PlayerDataObject(
                                visibility: PlayerDataObject.VisibilityOptions.Public,
                                value: "0")
                        }
                    })
            };

            CurrentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, lobbyOptions);
            Debug.Log("Lobby joined!");

            StopCoroutine(_pollLobbyCoroutine);
            await Task.Delay(_rateLimitGetLobby * 1000);

            _pollLobbyCoroutine = PollLobbyCoroutine(_rateLimitGetLobby);
            StartCoroutine(_pollLobbyCoroutine);
            Debug.Log("Started poll lobby coroutine.");

            return CurrentLobby;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);

            return null;
        }
    }

    IEnumerator PollLobbyCoroutine(float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            var t = Task.Run(async () => await LobbyService.Instance.GetLobbyAsync(CurrentLobby.Id));
            yield return new WaitUntil(() => t.IsCompleted);
            Debug.Log("Lobby poll complete.");

            CurrentLobby = t.Result;

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

            await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId, options);
            Debug.Log("Player's ready state set to true.");
        }
        catch (LobbyServiceException e)
        { 
            Debug.LogError(e);
        }
    }

    public async void ChangeCharacter(int charId)
    {
        try
        {
            UpdatePlayerOptions options = new()
            {
                Data = new Dictionary<string, PlayerDataObject>()
                {
                    {
                        "Character", new PlayerDataObject(
                            visibility: PlayerDataObject.VisibilityOptions.Public,
                            value: charId.ToString())
                    }
                }
            };

            await LobbyService.Instance.UpdatePlayerAsync(CurrentLobby.Id, AuthenticationService.Instance.PlayerId, options);
            Debug.Log("Player's character updated.");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public async void SetRelayKey(string relayCode)
    {
        try
        {
            UpdateLobbyOptions options = new()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    {
                        "RelayKey", new DataObject(
                            visibility: DataObject.VisibilityOptions.Member,
                            value: relayCode)
                    }
                }
            };

            await LobbyService.Instance.UpdateLobbyAsync(CurrentLobby.Id, options);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    public string GetRelayKey()
    {
        CurrentLobby.Data.TryGetValue("RelayKey", out var key);
        return key.Value;
    }
}
