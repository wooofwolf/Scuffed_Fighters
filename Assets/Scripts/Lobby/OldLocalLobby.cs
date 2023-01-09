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
public class OldLocalLobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _lobbyCodeUI;
    [SerializeField] private Button _readyButtonUI;
    [SerializeField] private TextMeshProUGUI _playersReadyUI;
    [SerializeField] private TMP_InputField _codeInput;
    [SerializeField] private Image _playerOneImage;
    [SerializeField] private Image _playerTwoImage;
    [SerializeField] private List<Sprite> _spriteList;

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
        if (_gameManager.LobbyManager.CurrentLobby == null) HostLocalLobby();
        else
        {
            _gameManager.RelayManager.DisconnectRelay();
            _currentLobby = _gameManager.LobbyManager.CurrentLobby;
            ChangeCharacter(0);
            SetReady("false");
        }
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

        if (_playerOneChar != 0)
        {
            _playerOneImage.color = new Color(255, 255, 255, 255);
            _playerOneImage.sprite = _spriteList[_playerOneChar - 1];
        }
        if (_playerTwoChar != 0)
        {
            _playerTwoImage.color = new Color(255, 255, 255, 255);
            _playerTwoImage.sprite = _spriteList[_playerTwoChar - 1];
        }
        Debug.Log("Player characters updated!");

        sb = new("/ 2", 5);
        sb.Insert(0, _playersReady + " ");
        _playersReadyUI.SetText(sb);
        Debug.Log("Players ready updated!");

        Debug.Log("Local lobby GUI updated!");
    }

    public void SetReady(string ready)
    {
        if (ready == "true")
        {
            if (AuthenticationService.Instance.PlayerId == _hostId && _playerOneChar == 0) return;
            else if (AuthenticationService.Instance.PlayerId != _hostId && _playerTwoChar == 0) return;
        }

        _gameManager.LobbyManager.SetReady(ready);
        _isPlayerReady = bool.Parse(ready);
    }

    public void ChangeCharacter(int charId)
    {
        _gameManager.LobbyManager.ChangeCharacter(charId);

        if (AuthenticationService.Instance.PlayerId == _hostId)
        {
            _playerOneChar = charId;
        }
        else
        {
            _playerTwoChar = charId;
        }

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

        _gameManager.CurrentGameState = GameManager.GameState.IN_GAME;
        _gameManager.IsHost = AuthenticationService.Instance.PlayerId == _hostId;
        _gameManager.CharacterOne = _playerOneChar;
        _gameManager.CharacterTwo = _playerTwoChar;
        SceneManager.LoadSceneAsync(2);
    }
}
