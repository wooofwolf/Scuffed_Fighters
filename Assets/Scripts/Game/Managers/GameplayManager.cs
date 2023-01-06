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

    public void EndGame(PlayerNetwork winner)
    {
        _winloseGui.gameObject.SetActive(true);

        StringBuilder sb = new(" wins!", 20);
        //sb.Insert(0, winner.GetCharacter());
        _winloseText.SetText(sb.ToString());
    }
}
