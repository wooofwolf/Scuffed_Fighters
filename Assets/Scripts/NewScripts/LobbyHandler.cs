using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;
using Unity.Services.Authentication;

public class LobbyHandler : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI _joinCodeText;

    [SerializeField] private TextMeshProUGUI _playersReadyText;

    [SerializeField] private Image _playerOneImage;
    [SerializeField] private Image _playerTwoImage;
    [SerializeField] private List<Sprite> _spriteList;

    private readonly NetworkManager _networkManager = NetworkManager.Singleton;
    private Dictionary<ulong, bool> _clientsReady;
    private Dictionary<ulong, int> _characters;
    private int _playersReady;
    private bool _gameStarting = false;

    public override void OnNetworkSpawn()
    {
        _clientsReady = new Dictionary<ulong, bool>
        {
            { NetworkManager.LocalClientId, false }
        };

        _characters = new Dictionary<ulong, int>
        {
            { NetworkManager.LocalClientId, 0 }
        };

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            SceneTransitionHandler.Instance.OnClientLoadedScene += ClientLoadedScene;
        }
    }

    private void OnGUI()
    {
        if (_gameStarting) return;

        // Players ready
        _playersReady = 0;
        foreach (var clientReadyPair in _clientsReady)
        {
            if (clientReadyPair.Value) ++_playersReady;
        }

        if (!_playersReadyText.text.StartsWith(_playersReady.ToString()))
        {
            StringBuilder sb = new(" / 2", 5);
            sb.Insert(0, _playersReady);
            _playersReadyText.SetText(sb);
        }

        // Characters
        foreach (var character in _characters)
        {
            if (character.Value == 0) continue;

            if (character.Key == 0)
            {
                _playerOneImage.sprite = _spriteList[character.Value - 1];
                _playerOneImage.color = Color.white;
            }
            else
            {
                _playerTwoImage.sprite = _spriteList[character.Value - 1];
                _playerTwoImage.color = Color.white;
            }
        }
    }

    public void SetJoinCode(string joinCode)
    {
        StringBuilder sb = new("Lobby Code: ", 18);
        sb.Append(joinCode);
        _joinCodeText.SetText(sb);
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (IsServer)
        {
            if (!_clientsReady.ContainsKey(clientId)) _clientsReady.Add(clientId, false);
        }
    }

    private void ClientLoadedScene(ulong clientId)
    {
        if (IsServer)
        {
            if (!_clientsReady.ContainsKey(clientId))
            {
                _clientsReady.Add(clientId, false);
            }
            if (!_characters.ContainsKey(clientId))
            {
                _characters.Add(clientId, 0);
            }
        }
    }

    public void GetReady() // 1
    {
        if (IsHost)
        {
            if (_characters[0] == 0) return;
        }
        else
        {
            if (_characters[1] == 0) return;
        }

        _clientsReady[_networkManager.LocalClientId] = true;

        if (IsServer) UpdateAndCheckPlayersInLobby();
        else OnClientIsReadyServerRpc(_networkManager.LocalClientId);
    }

    private void UpdateAndCheckPlayersInLobby() // 3
    {
        foreach (var clientLobbyStatus in _clientsReady)
        {
            SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key, clientLobbyStatus.Value);
        }

        CheckForAllPlayersReady();
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnClientIsReadyServerRpc(ulong clientid) // 2
    {
        if (_clientsReady.ContainsKey(clientid))
        {
            _clientsReady[clientid] = true;
            UpdateAndCheckPlayersInLobby();
        }
    }

    [ClientRpc]
    private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady) // 4
    {
        if (!IsServer)
        {
            if (!_clientsReady.ContainsKey(clientId))
                _clientsReady.Add(clientId, isReady);
            else
                _clientsReady[clientId] = isReady;
        }
    }

    public void ChangeCharacter(int charId) // 1
    {
        _characters[_networkManager.LocalClientId] = charId;

        if (IsServer) UpdateCharacters();
        else OnCharacterChangeServerRpc(_networkManager.LocalClientId, charId);
    }

    private void UpdateCharacters()
    {
        foreach (var characters in _characters)
        {
            SendCharacterUpdatesClientRpc(characters.Key, characters.Value);
        }
    }

    [ClientRpc]
    private void SendCharacterUpdatesClientRpc(ulong clientId, int charId) // 4
    {
        if (!IsServer)
        {
            if (!_characters.ContainsKey(clientId))
                _characters.Add(clientId, charId);
            else
                _characters[clientId] = charId;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void OnCharacterChangeServerRpc(ulong clientid, int charId) // 2
    {
        if (_characters.ContainsKey(clientid))
        {
            _characters[clientid] = charId;
            UpdateCharacterInLobby();
        }
    }

    private void UpdateCharacterInLobby() // 3
    {
        foreach (var characterStatus in _characters)
        {
            SendCharacterUpdatesClientRpc(characterStatus.Key, characterStatus.Value);
        }
    }

    private void CheckForAllPlayersReady()
    {
        var allPlayersAreReady = true;

        foreach (var clientLobbyStatus in _clientsReady)
            if (!clientLobbyStatus.Value)

                //If some clients are still loading into the lobby scene then this is false
                allPlayersAreReady = false;

        //Only if all players are ready
        if (allPlayersAreReady)
        {
            StartCountdownClientRpc();
        }
    }

    [ClientRpc]
    private void StartCountdownClientRpc()
    {
        StartCoroutine(CountdownToGameplay());
    }

    private IEnumerator CountdownToGameplay()
    {
        _gameStarting = true;

        float counter = 5;
        while (counter > 0)
        {
            StringBuilder sb = new("Starting in ", 13);
            sb.Append(counter);
            _playersReadyText.SetText(sb);

            yield return new WaitForSecondsRealtime(1);

            counter--;
        }

        GameManager.Instance.SetCharacters(_characters);

        if (IsServer)
        {
            //Remove our client connected callback
            _networkManager.OnClientConnectedCallback -= OnClientConnectedCallback;

            //Remove our scene loaded callback
            SceneTransitionHandler.Instance.OnClientLoadedScene -= ClientLoadedScene;

            //Transition to the ingame scene
            SceneTransitionHandler.Instance.SwitchScene("Gameplay", SceneTransitionHandler.SceneStates.IN_GAME);
        }
    }
}
