using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class GameplayManager : NetworkBehaviour
{
    [SerializeField] private Transform _winloseGui;
    [SerializeField] private TextMeshProUGUI _winloseText;
    [SerializeField] private Timer _timer;
    [SerializeField] private CharacterSpawner _characterSpawner;
    [SerializeField] private TextMeshProUGUI _startTimer;

    [SerializeField] private AudioClip _bgmClip;

    [SerializeField] private Animator _leftDoorAnimator;
    [SerializeField] private Animator _rightDoorAnimator;

    [SerializeField] private GameObject _winsLeft;
    [SerializeField] private GameObject _winsRight;
    [SerializeField] private Sprite _winSprite;

    public CameraManager CameraManager { get; private set; }

    private int roundNumber = 0;

    private void Start()
    {
        _winloseGui.gameObject.SetActive(false);

        CameraManager = GameObject.Find("CM vcam1").GetComponent<CameraManager>();
        StartCoroutine(StartGame());
    }
    private IEnumerator StartGame()
    {
        yield return new WaitForSecondsRealtime(2f);

        SoundManager.Instance.PlayMusic(_bgmClip);

        yield return new WaitForSecondsRealtime(1f);

        _leftDoorAnimator.Play("Door_Left_Open");
        _rightDoorAnimator.Play("Door_Right_Open");

        // Countdown
        for (int i = 0; i < 3; ++i)
        {
            _startTimer.SetText((3 - i).ToString());

            yield return new WaitForSecondsRealtime(1f);
        }

        _startTimer.SetText("Go!");
        _timer.SetTimerRunning(true);
        
        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            p.GetComponent<PlayerNetwork>().SetGameStart(true);
        }

        yield return new WaitForSecondsRealtime(1f);

        _startTimer.transform.parent.gameObject.SetActive(false);
    }

    public IEnumerator EndGame(int charId)
    {
        SoundManager.Instance.StopMusic();

        bool hasPlayerWon = false;

        yield return new WaitForSecondsRealtime(0.5f);

        _winloseGui.gameObject.SetActive(true);

        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            p.GetComponent<PlayerNetwork>().SetGameStart(false);

            Debug.Log(p.gameObject.name + " wins: " + p.GetComponent<PlayerNetwork>().GetWins());

            if (p.GetComponent<PlayerNetwork>().GetWins() >= 2)
            {
                Debug.Log("Player has 2 wins or more");
                hasPlayerWon = true;
            }
        }

        if (roundNumber >= 2 || hasPlayerWon)
        {
            Debug.Log("Player has won game");
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
        else
        {
            _winloseText.SetText("Game!");

            yield return new WaitForSeconds(3);

            StartCoroutine(StartNewRound());
        }
    }

    private IEnumerator StartNewRound()
    {
        roundNumber++;

        _leftDoorAnimator.Play("Door_Left_Close");
        _rightDoorAnimator.Play("Door_Right_Close");

        yield return new WaitForSecondsRealtime(1f);

        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerNetwork network = p.GetComponent<PlayerNetwork>();
            network.ResetVariables();

            int wins = network.GetWins();
            // this player is the host
            if (network.GetOwnerClientId() == 0)
            {
                if (wins > 0)
                    _winsLeft.transform.GetChild(1).GetComponent<Image>().sprite = _winSprite;
                if (wins > 1)
                    _winsLeft.transform.GetChild(0).GetComponent<Image>().sprite = _winSprite;
            }
            else
            {
                if (wins > 0)
                    _winsRight.transform.GetChild(0).GetComponent<Image>().sprite = _winSprite;
                if (wins > 1)
                    _winsRight.transform.GetChild(1).GetComponent<Image>().sprite = _winSprite;
            }
        }
        _timer.SetTimerRunning(false);
        _timer.SetTimeLeft(99f);
        _winloseGui.gameObject.SetActive(false);

        SoundManager.Instance.PlayMusic(_bgmClip);

        yield return new WaitForSecondsRealtime(1f);

        _leftDoorAnimator.Play("Door_Left_Open");
        _rightDoorAnimator.Play("Door_Right_Open");

        _startTimer.transform.parent.gameObject.SetActive(true);
        // Countdown
        for (int i = 0; i < 3; ++i)
        {
            _startTimer.SetText((3 - i).ToString());

            yield return new WaitForSecondsRealtime(1f);
        }

        _startTimer.SetText("Go!");
        _timer.SetTimerRunning(true);

        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            p.GetComponent<PlayerNetwork>().SetGameStart(true);
        }

        yield return new WaitForSecondsRealtime(1f);

        _startTimer.transform.parent.gameObject.SetActive(false);
    }

    public Timer GetTimer()
    {
        return _timer;
    }
}
