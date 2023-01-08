using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float _movementSpeed = 1.0f;
    [SerializeField] private float _smoothInputSpeed = 0.2f;

    [SerializeField] private GameObject _hand;

    private GameplayManager _gameplayManager;
    private PlayerInputActions _actions;
    private Rigidbody _rb;
    private Animator _animator;
    private Healthbar _healthbar;

    private readonly NetworkVariable<float> _health = new(100.0f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<float> _normalizedInputX = new(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<float> _currentInputX = new(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _isFacingRight = new(default,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _isCrouching = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _isAttacking = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _allowCombo = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _allowHits = new(true,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _isHit = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _canBlock = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _isBlocking = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private readonly NetworkVariable<bool> _isDead = new(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private bool _isOwner;

    private void Awake()
    {
        _gameplayManager = GameObject.Find("GameplayManager").GetComponent<GameplayManager>();
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();

        _actions = new();
        _actions.Enable();
        _actions.Player.Crouch.performed += CrouchPerformed;
        _actions.Player.Light.performed += LightPerformed;
    }

    public override void OnNetworkSpawn()
    {
        _isOwner = IsOwner;

        InitHealthbars(IsHost, _isOwner);

        if (!_isOwner) return;

        InitPlayerServerRpc(IsHost);
    }

    private void InitHealthbars(bool isHost, bool isOwner)
    {
        Healthbar[] healthbars = FindObjectsOfType<Healthbar>();

        if (isHost)
        {
            if (isOwner) _healthbar = healthbars[1];
            else _healthbar = healthbars[0];
        }
        else
        {
            if (isOwner) _healthbar = healthbars[0];
            else _healthbar = healthbars[1];
        }
    }

    [ServerRpc]
    private void InitPlayerServerRpc(bool isHost)
    {
        InitPlayerPosClientRpc(isHost);
        InitCameraClientRpc();
    }

    [ClientRpc]
    private void InitPlayerPosClientRpc(bool isHost)
    {
        if (isHost)
        {
            transform.SetPositionAndRotation(new Vector3(-2, 0, 0), Quaternion.Euler(0, 90, 0));
            _isFacingRight.Value = true;
        }
        else
        {
            transform.SetPositionAndRotation(new Vector3(2, 0, 0), Quaternion.Euler(0, -90, 0));
            _isFacingRight.Value = false;
        }
    }

    [ClientRpc]
    private void InitCameraClientRpc()
    {
        _gameplayManager.CameraManager.TargetAllPlayers();
    }

    private void Update()
    {
        if (!_isOwner) return;

        _normalizedInputX.Value = _actions.Player.Movement.ReadValue<float>();
    }

    private void FixedUpdate()
    {
        if (!_isOwner) return;
        if (_isCrouching.Value) return;
        if (_isAttacking.Value) return;
        if (_isHit.Value) return;
        if (_isBlocking.Value) return;
        if (_isDead.Value) return;

        _currentInputX.Value = Mathf.Lerp(_currentInputX.Value, _normalizedInputX.Value, _smoothInputSpeed);
        _rb.MovePosition(transform.position + _movementSpeed * Time.deltaTime * new Vector3(_currentInputX.Value, 0, 0));
        
        if (_isFacingRight.Value)
        {
            _animator.SetFloat("xInput", _currentInputX.Value);
            if (_currentInputX.Value < -0.1f) _canBlock.Value = true;
            else if (_canBlock.Value) _canBlock.Value = false;
        }
        else
        {
            _animator.SetFloat("xInput", -_currentInputX.Value);
            if (_currentInputX.Value > 0.1f) _canBlock.Value = true;
            else if (_canBlock.Value) _canBlock.Value = false;
        }
    }

    private void CrouchPerformed(InputAction.CallbackContext context)
    {
        if (!_isOwner) return;

        _isCrouching.Value = !_isCrouching.Value;
        _animator.SetBool("isCrouching", _isCrouching.Value);
    }

    private void LightPerformed(InputAction.CallbackContext context)
    {
        if (!_isOwner) return;
        if (_isAttacking.Value && !_allowCombo.Value) return;
        if (_isHit.Value) return;
        if (_isBlocking.Value) return;

        _isAttacking.Value = true;
        _allowCombo.Value = false;

        if (_isCrouching.Value) PlayAnimationServerRpc("Light_Crouch", 0.05f);
        else
        {
            if (_normalizedInputX.Value == 0) PlayAnimationServerRpc("Light_Neutral", 0.05f);
            else PlayAnimationServerRpc("Light_Forward", 0.05f);
        }
    }

    [ServerRpc]
    private void PlayAnimationServerRpc(string animationName, float transitionDuration)
    {
        PlayAnimationClientRpc(animationName, transitionDuration);
    }

    [ClientRpc]
    private void PlayAnimationClientRpc(string animationName, float transitionDuration)
    {
        _animator.CrossFadeInFixedTime(animationName, transitionDuration);
    }

    public void SpawnProjectile(Object obj)
    {
        Object projObj = Instantiate(obj, new Vector3(_hand.transform.position.x, _hand.transform.position.y, 0), Quaternion.Inverse(transform.rotation));
        Projectile proj = (projObj as GameObject).GetComponent<Projectile>();
        proj.SetOwner(this);
    }

    public void AllowCombo()
    {
        if (!_isOwner) return;

        _allowCombo.Value = true;
    }

    public void FinishAttack()
    {
        if (!_isOwner) return;

        _isAttacking.Value = false;
        _allowCombo.Value = false;
    }

    public void GetHit(float damage)
    {
        if (!_isOwner) return;
        if (!_allowHits.Value) return;
        if (_isDead.Value) return;

        if (_canBlock.Value)
        {
            _isBlocking.Value = true;
            _canBlock.Value = false;
            PlayAnimationServerRpc("Blocking", 0.05f);
            return;
        }

        _health.Value -= damage;
        _isHit.Value = true;
        _isAttacking.Value = false;
        _isBlocking.Value = false;

        UpdateHealthbarServerRpc(_health.Value);

        if (_health.Value <= 0.0f)
        {
            _isDead.Value = true;
            PlayAnimationServerRpc("Death", 0.05f);

            if (GameManager.Instance.IsHost) EndGameServerRpc(GameManager.Instance.CharacterOne);
            else EndGameServerRpc(GameManager.Instance.CharacterTwo);
        }
        else PlayAnimationServerRpc("Impact", 0.05f);
    }

    [ServerRpc]
    private void UpdateHealthbarServerRpc(float updatedHealth)
    {
        UpdateHealthbarClientRpc(updatedHealth);
    }

    [ClientRpc]
    private void UpdateHealthbarClientRpc(float updatedHealth)
    {
        _healthbar.UpdateHealthbar(updatedHealth);
    }

    [ServerRpc]
    private void EndGameServerRpc(int charId)
    {
        EndGameClientRpc(charId);
    }

    [ClientRpc]
    private void EndGameClientRpc(int charId)
    {
        StartCoroutine(_gameplayManager.EndGame(charId));
    }

    public void AllowHits()
    {
        if (!_isOwner) return;

        _allowHits.Value = true;
    }

    public void StopHits()
    {
        if (!_isOwner) return;

        _allowHits.Value = false;
    }

    public void ResetHit()
    {
        if (!_isOwner) return;

        _isHit.Value = false;
    }

    public void StopBlocking()
    {
        if (!_isOwner) return;

        _isBlocking.Value = false;
    }
}
