using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Timer : MonoBehaviour
{
    private TextMeshProUGUI ui;
    private float _timeLeft = 10;
    private bool _timerStarted = false;

    private void Start()
    {
        ui = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 2 && !_timerStarted)
        {
            _timerStarted = true;
        }

        if (!_timerStarted) return;

        ui.text = ((int)_timeLeft).ToString();
        _timeLeft -= Time.deltaTime;
    }

    public int GetTimeLeft()
    {
        return (int)_timeLeft;
    }
}
