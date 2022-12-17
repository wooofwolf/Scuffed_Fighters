using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerHurtbox : NetworkBehaviour
{
    [SerializeField] private float _damageModifier = 1.0f;

    private PlayerNetwork _playerNetwork;

    private void Start()
    {
        _playerNetwork = transform.root.GetComponent<PlayerNetwork>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        PlayerHitbox hitbox = other.GetComponent<PlayerHitbox>();
        _playerNetwork.GetHit(hitbox.GetDamage() * _damageModifier);
    }
}
