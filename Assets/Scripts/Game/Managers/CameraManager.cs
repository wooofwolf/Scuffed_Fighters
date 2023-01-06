using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private CinemachineTargetGroup _group;

    private CinemachineVirtualCamera _cam;
    private readonly List<Transform> _transforms = new();

    private void Start()
    {
        _cam = GetComponent<CinemachineVirtualCamera>();
    }

    public void TargetAllPlayers()
    {
        foreach (PlayerNetwork player in FindObjectsOfType<PlayerNetwork>())
        {
            TryAddMember(player.transform);
        }
    }

    private void TryAddMember(Transform transform)
    {
        if (_transforms.Contains(transform)) return;

        _group.AddMember(transform, 1, 1);
        _transforms.Add(transform);
    }
}
