using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    public void UpdateHealthbar(float updatedHp)
    {
        StartCoroutine(LerpHealthbar(updatedHp, 0.05f));
    }

    private IEnumerator LerpHealthbar(float updatedHp, float duration)
    {
        float t = 0;
        float startValue = _slider.value;
        while (t < duration)
        {
            _slider.value = Mathf.Lerp(startValue, updatedHp, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        _slider.value = updatedHp;
    }
}
