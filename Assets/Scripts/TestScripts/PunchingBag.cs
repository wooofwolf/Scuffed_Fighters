using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBag : MonoBehaviour
{
    [SerializeField] private TimeScaleController timeScaleController;
    [SerializeField] private CameraController cameraController;

    [SerializeField] private GameObject hitEffect;

    private void OnTriggerEnter(Collider other)
    {
        PlayerHitbox hitbox = other.GetComponent<PlayerHitbox>();
        if (hitbox == null) return;
        
        StartCoroutine(timeScaleController.HitStop(0.2f, 0.2f));
        cameraController.ShakeCamera(0.5f, 0.05f);

        GameObject effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
        effect.transform.LookAt(Camera.main.transform);
        StartCoroutine(DeleteEffectAfterTime(0.75f, effect));
    }

    private IEnumerator DeleteEffectAfterTime(float time, GameObject effect)
    {
        yield return new WaitForSeconds(time);

        Destroy(effect);
    }
}
