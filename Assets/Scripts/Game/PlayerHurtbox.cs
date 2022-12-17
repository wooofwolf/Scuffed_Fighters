using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHurtbox : MonoBehaviour
{
    [SerializeField] private float _damageModifier = 1.0f;

    private PlayerNetwork _playerNetwork;

    private void Start()
    {
        _playerNetwork = transform.root.GetComponent<PlayerNetwork>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHitbox hitbox = other.GetComponent<PlayerHitbox>();
        _playerNetwork.GetHit(hitbox.GetDamage() * _damageModifier);
    }
}
