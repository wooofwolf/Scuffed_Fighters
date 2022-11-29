using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float _movementSpeed;

    private Rigidbody _rigidbody;
    private Animator _animator;
    private NetworkManager _networkManager;
    private PlayerInputActions _playerInputActions;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _networkManager = NetworkManager.Singleton;

        _playerInputActions = new ();
        _playerInputActions.Enable();
        _playerInputActions.Player.Neutral.performed += NeutralPerformed;

        if (!IsOwner) return;

        if (_networkManager.IsHost) transform.SetPositionAndRotation(new Vector3(-2, 0, 0), Quaternion.Euler(0, 90, 0));
        else transform.SetPositionAndRotation(new Vector3(2, 0, 0), Quaternion.Euler(0, -90, 0));
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        float inputX = _playerInputActions.Player.Movement.ReadValue<float>();
        _rigidbody.MovePosition(transform.position + _movementSpeed * Time.deltaTime * new Vector3(inputX, 0, 0));

        Debug.Log(inputX);

        _animator.SetFloat("xInput", inputX);
    }

    private void NeutralPerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Debug.Log("Neutral attack");
    }
}
