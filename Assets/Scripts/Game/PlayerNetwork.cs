using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float _movementSpeed = 1.0f;
    [SerializeField] private float _smoothInputSpeed = 0.2f;

    private Rigidbody _rigidbody;
    private Animator _animator;
    private NetworkManager _networkManager;
    private PlayerInputActions _playerInputActions;

    private float _normalizedInputX;
    private float _currentInputX;
    private bool _isFacingRight;
    private bool _isCrouching = false;
    private bool _isAttacking = false;
    private bool _allowCombo = false;

    private int _healthMax = 100;
    private int _health = 100;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _networkManager = NetworkManager.Singleton;

        if (GameObject.Find("Healthbar1").GetComponent<Healthbar>().Owner != this)
        {
            GameObject.Find("Healthbar1").GetComponent<Healthbar>().Owner = this;
        }
        else
        {
            GameObject.Find("Healthbar2").GetComponent<Healthbar>().Owner = this;
        }
    }

    private void Start()
    {
        if (!IsOwner) return;

        if (IsHost)
        {
            transform.SetPositionAndRotation(new Vector3(-2, 0, 0), Quaternion.Euler(0, 90, 0));
            _isFacingRight = true;
        }
        else
        {
            transform.SetPositionAndRotation(new Vector3(2, 0, 0), Quaternion.Euler(0, -90, 0));
            _isFacingRight = false;
        }

        _playerInputActions = new ();
        _playerInputActions.Enable();
        _playerInputActions.Player.Jump.performed += JumpPerformed;
        _playerInputActions.Player.Crouch.performed += CrouchPerformed;
        _playerInputActions.Player.Light.performed += LightPerformed;
        _playerInputActions.Player.Heavy.performed += HeavyPerformed;
        _playerInputActions.Player.Heavy.canceled += HeavyCanceled;
    }

    [ServerRpc(RequireOwnership = false)]
    private void InitPlayerServerRpc(bool isHost, ServerRpcParams serverRpcParams = default)
    {
        _networkManager.ConnectedClients.TryGetValue(serverRpcParams.Receive.SenderClientId, out NetworkClient client);
        Transform player = client.PlayerObject.transform;
        if (isHost)
        {
            Debug.LogWarning("Setting Host pos for client " + serverRpcParams.Receive.SenderClientId);

            player.SetPositionAndRotation(new Vector3(-2, 0, 0), Quaternion.Euler(0, 90, 0));
            _isFacingRight = true;
        }
        else
        {
            Debug.LogWarning("Setting Client pos for client " + serverRpcParams.Receive.SenderClientId);

            player.SetPositionAndRotation(new Vector3(2, 0, 0), Quaternion.Euler(0, -90, 0));
            _isFacingRight = false;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        _normalizedInputX = _playerInputActions.Player.Movement.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;
        if (_isAttacking) return;
        if (_isCrouching) return;

        _currentInputX = Mathf.Lerp(_currentInputX, _normalizedInputX, _smoothInputSpeed);
        _rigidbody.MovePosition(transform.position + _movementSpeed * Time.deltaTime * new Vector3(_currentInputX, 0, 0));

        if (_isFacingRight) _animator.SetFloat("xInput", _currentInputX);
        else _animator.SetFloat("xInput", -_currentInputX);
    }

    private void JumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("Jump");
    }

    private void CrouchPerformed(InputAction.CallbackContext context)
    {
        _isCrouching = !_isCrouching;
        _animator.SetBool("isCrouching", _isCrouching);
    }

    private void LightPerformed(InputAction.CallbackContext context)
    {
        if (_isAttacking && !_allowCombo) return;

        _isAttacking = true;
        _allowCombo = false;

        if (_isCrouching) _animator.CrossFadeInFixedTime("Light_Crouch", 0.1f);
        else
        {
            if (_normalizedInputX == 0) _animator.CrossFadeInFixedTime("Light_Neutral", 0.1f);
            else _animator.CrossFadeInFixedTime("Light_Forward", 0.1f);
        }
    }

    private void HeavyPerformed(InputAction.CallbackContext context)
    {
        if (_isAttacking && !_allowCombo) return;

        _isAttacking = true;
        _allowCombo = false;

        if (_isCrouching)
        {
            // Heavy crouch attack?
        }
        else
        {
            if (_normalizedInputX == 0) _animator.CrossFadeInFixedTime("Heavy_Neutral_Charge", 0.1f);
        }
    }

    private void HeavyCanceled(InputAction.CallbackContext context)
    {
        if (_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name == "Heavy_Neutral_Charge") _animator.CrossFadeInFixedTime("Heavy_Neutral_Attack", 0.1f);
    }

    public void AllowCombo()
    {
        _allowCombo = true;
    }

    public void FinishAttack()
    {
        _isAttacking = false;
        _allowCombo = false;
    }

    public float GetHealthPercent()
    {
        Debug.Log(_health + " / " + _healthMax + " = " + _health / _healthMax);
        return ((float)_health / _healthMax);
    }

    [ClientRpc]
    public void DamageClientRPC(int damage)
    {
        _health -= damage;
        Debug.Log("HP: " + _health);
    }
}
