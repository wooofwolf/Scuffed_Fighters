using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup _group;

    private CinemachineVirtualCamera _cam;

    private void Start()
    {
        _cam = GetComponent<CinemachineVirtualCamera>();
    }

    public void AddLookAtTarget(Transform transform)
    {
        _group.AddMember(transform, 1, 1);
    }

    public void SetFollow(Transform transform)
    {
        _cam.Follow = transform;
    }
}
