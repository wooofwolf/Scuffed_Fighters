using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] private int _damage;

    public int GetDamage()
    {
        return _damage;
    }
}
