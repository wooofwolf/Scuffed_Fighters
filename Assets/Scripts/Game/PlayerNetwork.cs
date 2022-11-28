using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float _movementSpeed;

    private Rigidbody _rigidbody;
    private PlayerInputActions _playerInputActions;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        _playerInputActions = new ();
        _playerInputActions.Enable();
        _playerInputActions.Player.Neutral.performed += NeutralPerformed;
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 inputVector = _playerInputActions.Player.Movement.ReadValue<Vector2>();
        _rigidbody.MovePosition(transform.position + _movementSpeed * Time.deltaTime * new Vector3(inputVector.x, 0, 0));
    }

    private void NeutralPerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Debug.Log("Neutral attack");
    }
}
