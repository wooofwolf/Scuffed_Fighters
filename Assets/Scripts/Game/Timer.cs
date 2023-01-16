using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Timer : NetworkBehaviour
{
    private TextMeshProUGUI _timerText;
    private float _timeLeft;

    public override void OnNetworkSpawn()
    {
        _timerText = GetComponent<TextMeshProUGUI>();
        _timeLeft = 99;
    }

    private void Update()
    {
        if (_timeLeft > 0) _timeLeft -= Time.deltaTime;
    }

    private void OnGUI()
    {
        _timerText.text = ((int)_timeLeft).ToString();
    }

    public int GetTimeLeft()
    {
        return (int)_timeLeft;
    }
}
