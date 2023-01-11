using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeScaleController : MonoBehaviour
{
    // Singleton Pattern
    public static TimeScaleController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(this);
        else Instance = this;
    }

    public IEnumerator HitStop(float timer)
    {
        SetTimeScale(0.1f);

        yield return new WaitForSecondsRealtime(timer);

        SetTimeScale(1);
    }

    private void SetTimeScale(float timeScale)
    {
        Debug.Log("Time scale set to " + timeScale);
        Time.timeScale = timeScale;
    }
}
