using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float _lifespan;
    [SerializeField] private float _speed;
    [SerializeField] private float _damage;

    private void Update()
    {
        if (_lifespan <= 0) Destroy(gameObject);

        _lifespan -= Time.deltaTime;

        transform.Translate(_speed * Time.deltaTime * Vector3.back);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHurtbox hurtbox = other.GetComponent<PlayerHurtbox>();
        PlayerNetwork playerNetwork = other.transform.root.GetComponent<PlayerNetwork>();
        playerNetwork.GetHit(_damage * hurtbox.GetDamageModifier());

        Destroy(gameObject);
    }
}
