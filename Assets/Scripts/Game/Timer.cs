using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Timer : NetworkBehaviour
{
    [SerializeField] private GameplayManager _gameplayManager;

    private TextMeshProUGUI _timerText;
    private List<PlayerNetwork> _players = new();

    private float _timeLeft;
    private bool _timerRunning;

    public override void OnNetworkSpawn()
    {
        _timerText = GetComponent<TextMeshProUGUI>();
        _timeLeft = 10;
        _timerRunning = false;
    }

    private void Update()
    {
        if (!_timerRunning) return;

        if (_timeLeft > 0) _timeLeft -= Time.deltaTime;
        if (_timeLeft <= 0)
        {
            EndGameServerRpc();
        }
    }

    private void OnGUI()
    {
        _timerText.text = ((int)_timeLeft).ToString();
    }

    public int GetTimeLeft()
    {
        return (int)_timeLeft;
    }

    public void SetTimerRunning(bool timerRunning)
    {
        _timerRunning = timerRunning;
    }

    public void AddPlayer(PlayerNetwork p)
    {
        _players.Add(p);
    }

    [ServerRpc]
    private void EndGameServerRpc()
    {
        EndGameClientRpc();
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        SetTimerRunning(false);
        if (_players[0].GetHealth() > _players[1].GetHealth()) StartCoroutine(_gameplayManager.EndGame(GameManager.Instance.GetCharactersId(_players[0].OwnerClientId)));
        else if (_players[0].GetHealth() < _players[1].GetHealth()) StartCoroutine(_gameplayManager.EndGame(GameManager.Instance.GetCharactersId(_players[1].OwnerClientId)));
        else StartCoroutine(_gameplayManager.EndGame(0));
    }
}
