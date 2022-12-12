using System.Collections;
using System.Collections.Generic;
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

        Debug.Log("Player gets damaged");
        _playerNetwork.DamageClientRPC(hitbox.GetDamage() * _damageModifier);
    }
}
