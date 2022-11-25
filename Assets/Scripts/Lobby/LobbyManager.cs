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

    private ServiceRateLimiter _getLobbyCooldown = new ServiceRateLimiter(1, 1f);

    private readonly string _lobbyName = Guid.NewGuid().ToString();

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
                Player = new (id: AuthenticationService.Instance.PlayerId)
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(_lobbyName, 2, lobbyOptions);
            Debug.Log("Lobby successfully created!");

            _heartbeatCoroutine = HeartbeatLobbyCoroutine(lobby.Id, 15);
            StartCoroutine(_heartbeatCoroutine);
            Debug.Log("Started heartbeat lobby coroutine.");

            _localLobby.InitLobby(lobby);

            await PollLobby();

            //_pollLobbyCoroutine = PollLobbyCoroutine(1);
            //StartCoroutine(_pollLobbyCoroutine);
            //Debug.Log("Started poll lobby coroutine.");
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
            // Delete current lobby
            await LobbyService.Instance.DeleteLobbyAsync(_localLobby.LobbyId);
            Debug.Log("Current lobby deleted.");

            StopCoroutine(_heartbeatCoroutine);
            Debug.Log("Stopped heartbeat coroutine.");

            // Join existing lobby
            JoinLobbyByCodeOptions lobbyOptions = new()
            {
                Player = new (id: AuthenticationService.Instance.PlayerId)
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(_codeInput.text, lobbyOptions);
            Debug.Log("Lobby joined!");

            _localLobby.InitLobby(lobby);

            await PollLobby();

            //StopCoroutine(_pollLobbyCoroutine);
            //_pollLobbyCoroutine = PollLobbyCoroutine(1);
            //StartCoroutine(_pollLobbyCoroutine);
            //Debug.Log("Started poll lobby coroutine.");
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

    public async Task PollLobby()
    {
        Lobby updatedLobby = await LobbyService.Instance.GetLobbyAsync(_localLobby.LobbyId);

        if (_getLobbyCooldown.TaskQueued)
            return;
        await _getLobbyCooldown.QueueUntilCooldown();

        _localLobby.UpdateLobby(updatedLobby);
    }
}
