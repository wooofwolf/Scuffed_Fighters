using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class LocalLobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyCodeUI;
    [SerializeField] private Button _readyButtonUI;
    [SerializeField] private TextMeshProUGUI _playersReadyUI;
    [SerializeField] private TMP_InputField _codeInput;

    private const int MAX_PLAYER_COUNT = 2;

    private readonly GameManager _gameManager = GameManager.Instance;
    private Lobby _currentLobby;

    private string _lobbyId;
    private string _lobbyCode;
    private string _hostId;
    private int _availableSlots;
    private bool _isPlayerReady;
    private int _playersReady;
    private int _playerOneChar = 0;
    private int _playerTwoChar = 0;

    private void Start()
    {
        HostLocalLobby();
    }

    private void Update()
    {
        if (_currentLobby == null) return;

        if (_currentLobby != _gameManager.LobbyManager.CurrentLobby && _gameManager.CurrentGameState == GameManager.GameState.LOBBY)
        {
            _currentLobby = _gameManager.LobbyManager.CurrentLobby;

            if (_currentLobby.Id != _lobbyId) _lobbyId = _currentLobby.Id;
            if (_currentLobby.LobbyCode != _lobbyCode) _lobbyCode = _currentLobby.LobbyCode;
            if (_currentLobby.HostId != _hostId) _hostId = _currentLobby.HostId;
            if (_currentLobby.AvailableSlots != _availableSlots) _availableSlots = _currentLobby.AvailableSlots;
            
            _playersReady = 0;
            foreach (Player p in _currentLobby.Players)
            {
                p.Data.TryGetValue("Ready", out var playerData);
                if (playerData.Value.Equals("true"))
                {
                    ++_playersReady;
                }

                GetCharacter(p);
            }

            UpdateGUI();

            if (_playersReady == MAX_PLAYER_COUNT)
            {
                _gameManager.CurrentGameState = GameManager.GameState.COUNTDOWN;
                StartCoroutine(Countdown(5));
            }

            Debug.Log("Lobby updated.");
        }
    }

    private void GetCharacter(Player p)
    {
        // Ignore your character because you already know it
        if (p.Id == AuthenticationService.Instance.PlayerId) return;

        p.Data.TryGetValue("Character", out var playerChar);
        if (p.Id == _hostId)    // Host's character
        {
            if (_playerOneChar != int.Parse(playerChar.Value)) _playerOneChar = int.Parse(playerChar.Value);
        }
        else
        {
            if (_playerTwoChar != int.Parse(playerChar.Value)) _playerTwoChar = int.Parse(playerChar.Value);
        }
    }

    private async void HostLocalLobby()
    {
        _currentLobby = await _gameManager.LobbyManager.HostLobby();
    }

    public async void JoinLobby()
    {
        _currentLobby = await _gameManager.LobbyManager.JoinLobby(_codeInput.text);
    }

    private void UpdateGUI()
    {
        StringBuilder sb = new("Lobby Code: ", 18);
        sb.Append(_lobbyCode);
        _lobbyCodeUI.SetText(sb);
        Debug.Log("Lobby code updated!");

        if (_isPlayerReady) _readyButtonUI.gameObject.SetActive(false);
        else _readyButtonUI.gameObject.SetActive(true);
        Debug.Log("Ready button updated!");

        sb = new("/ 2", 5);
        sb.Insert(0, _playersReady + " ");
        _playersReadyUI.SetText(sb);
        Debug.Log("Players ready updated!");

        Debug.Log("Local lobby GUI updated!");
    }

    public void GetReady()
    {
        _gameManager.LobbyManager.GetReady();
        _isPlayerReady = true;
    }

    public void ChangeCharacter(int charId)
    {
        Debug.Log("Changing chaarcter");

        _gameManager.LobbyManager.ChangeCharacter(charId);

        if (AuthenticationService.Instance.PlayerId == _hostId)
        {
            _playerOneChar = charId;
        }
        else
        {
            _playerTwoChar = charId;
        }
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

        _gameManager.CurrentGameState = GameManager.GameState.IN_GAME;
        _gameManager.IsHost = AuthenticationService.Instance.PlayerId == _hostId;
        SceneManager.LoadSceneAsync(2);
    }
}
