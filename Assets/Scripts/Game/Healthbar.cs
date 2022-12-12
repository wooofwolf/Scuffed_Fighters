using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Healthbar : MonoBehaviour
{
    public PlayerNetwork Owner { get; set; }

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }


}
