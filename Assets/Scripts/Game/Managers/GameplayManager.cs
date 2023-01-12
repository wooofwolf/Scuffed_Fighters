using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameplayManager : MonoBehaviour
{
    [SerializeField] private Transform _winloseGui;
    [SerializeField] private TextMeshProUGUI _winloseText;
    [SerializeField] private Timer _timer;

    public CameraManager CameraManager { get; private set; }

    private readonly GameManager _gameManager = GameManager.Instance;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _winloseGui.gameObject.SetActive(false);

        CameraManager = GameObject.Find("CM vcam1").GetComponent<CameraManager>();

        if (_gameManager.IsHost)
        {
            string relayCode = await _gameManager.RelayManager.CreateRelay();
            _gameManager.LobbyManager.SetRelayKey(relayCode);
        }
        else
        {
            StartCoroutine(TryJoiningRelay());
        }
    }

    private IEnumerator TryJoiningRelay()
    {
        var delay = new WaitForSecondsRealtime(1);
        string relayCode = "0";

        while (relayCode == "0")
        {
            relayCode = _gameManager.LobbyManager.GetRelayKey();

            yield return delay;
        }

        _gameManager.RelayManager.JoinRelay(relayCode);
    }

    public IEnumerator EndGame(int charId)
    {
        _winloseGui.gameObject.SetActive(true);

        StringBuilder sb = new(" wins!", 20);
        
        switch (charId)
        {
            case 1:
                sb.Insert(0, "Maria");
                break;
            case 2:
                sb.Insert(0, "Abe");
                break;
        }
        _winloseText.SetText(sb.ToString());

        yield return new WaitForSeconds(3);

        _gameManager.CurrentGameState = GameManager.GameState.LOBBY;
        SceneManager.LoadSceneAsync(1);
    }

    public Timer GetTimer()
    {
        return _timer;
    }

    public IEnumerator DrawGame()
    {
        _winloseGui.gameObject.SetActive(true);

        StringBuilder sb = new("DRAW!", 5);
        _winloseText.SetText(sb.ToString());

        yield return new WaitForSeconds(3);

        _gameManager.CurrentGameState = GameManager.GameState.LOBBY;
        SceneManager.LoadSceneAsync(1);
    }
}
