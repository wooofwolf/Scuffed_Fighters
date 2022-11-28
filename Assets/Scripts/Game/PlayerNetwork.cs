using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private float _movementSpeed;

    private Rigidbody _rigidbody;
    private NetworkObject _networkObject;
    private NetworkManager _networkManager;
    private PlayerInputActions _playerInputActions;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _networkObject = GetComponent<NetworkObject>();

        _networkManager = NetworkManager.Singleton;

        _playerInputActions = new ();
        _playerInputActions.Enable();
        _playerInputActions.Player.Neutral.performed += NeutralPerformed;

        if (_networkManager.LocalClientId == _networkObject.OwnerClientId) InitTransformServerRpc();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        Vector2 inputVector = _playerInputActions.Player.Movement.ReadValue<Vector2>();
        _rigidbody.MovePosition(transform.position + _movementSpeed * Time.deltaTime * new Vector3(inputVector.x, 0, 0));
    }

    [ServerRpc (RequireOwnership = false)]
    private void InitTransformServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (_networkManager.IsHost)
        {
            transform.SetPositionAndRotation(new Vector3(-2, 0, 0), Quaternion.Euler(0, 90, 0));
        }
        else
        {
            transform.SetPositionAndRotation(new Vector3(2, 0, 0), Quaternion.Euler(0, -90, 0));
        }
    }

    private void NeutralPerformed(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;

        Debug.Log("Neutral attack");
    }
}
