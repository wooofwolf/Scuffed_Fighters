using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;

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

    public CameraManager CameraManager { get; private set; }

    private int roundNumber;

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

        _winloseGui.gameObject.SetActive(true);

        foreach (GameObject p in GameObject.FindGameObjectsWithTag("Player"))
        {
            p.GetComponent<PlayerNetwork>().SetGameStart(false);
        }

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
