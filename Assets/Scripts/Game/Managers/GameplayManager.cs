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

public class GameplayManager : NetworkBehaviour
{
    [SerializeField] private Transform _winloseGui;
    [SerializeField] private TextMeshProUGUI _winloseText;
    [SerializeField] private Timer _timer;
    [SerializeField] private CharacterSpawner _characterSpawner;

    public CameraManager CameraManager { get; private set; }

    public enum GameplayState
    {
        START = 0,
        IN_PROGRESS,
        END
    }

    private GameplayState _gameplayState = GameplayState.START;

    private void Start()
    {
        _winloseGui.gameObject.SetActive(false);

        CameraManager = GameObject.Find("CM vcam1").GetComponent<CameraManager>();
    }

    public GameplayState GetGameplayState()
    {
        return _gameplayState;
    }

    public IEnumerator EndGame(int charId)
    {
        _winloseGui.gameObject.SetActive(true);

        StringBuilder sb = new(" wins!", 20);
        
        switch (charId)
        {
            case 0:
                sb = new("Draw!", 5);
                break;
            case 1:
                sb.Insert(0, "Maria");
                break;
            case 2:
                sb.Insert(0, "Abe");
                break;
        }
        _winloseText.SetText(sb.ToString());

        yield return new WaitForSeconds(3);

        SceneTransitionHandler.Instance.SwitchScene("MainMenu", SceneTransitionHandler.SceneStates.LOBBY);
    }

    public Timer GetTimer()
    {
        return _timer;
    }
}
