using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboCounter : MonoBehaviour
{
    private TextMeshProUGUI _comboText;

    private int _comboCount = 0;
    private float _stayTimer = 0;

    private void Start()
    {
        _comboText = GetComponent<TextMeshProUGUI>();
        _comboText.SetText("");
    }

    private void Update()
    {
        if (_stayTimer >= 0) _stayTimer -= Time.deltaTime;

        if (_stayTimer < 0 && _comboText.alpha > 0)
        {
            _comboText.alpha -= Time.deltaTime;
        }
    }

    public void AddToCombo()
    {
        _comboCount++;
        _comboText.alpha = 1;
        _stayTimer = 2f;
        _comboText.SetText(_comboCount + " HIT");
    }

    public void ResetCombo(string text)
    {
        _comboCount = 0;
        _comboText.alpha = 1;
        _stayTimer = 2f;
        _comboText.SetText(text);
    }
}
