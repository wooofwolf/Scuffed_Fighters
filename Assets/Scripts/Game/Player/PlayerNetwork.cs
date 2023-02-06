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
    [SerializeField] private GameObject _hitEffect;
    [SerializeField] private GameObject _shieldEffect;

    [SerializeField] private List<AudioClip> _audioClips;
    [SerializeField] private List<float> _audioDelays;

    private GameplayManager _gameplayManager;
    private PlayerInputActions _actions;
    private Rigidbody _rb;
    private Animator _animator;
    private Healthbar _healthbar;
    private ComboCounter _comboCounter;
    private TimeScaleController _timeScaleController;
    private CameraController _cameraController;
    private Timer _timer;
    private SoundManager _soundManager;
    private PlayerNetwork _enemyNetwork;
    private GameObject _projectile;

    private bool _gameStarted = false;

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
    private readonly NetworkVariable<int> _wins = new(0,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    private bool _isOwner;

    private void Start()
    {
        StartCoroutine(Init());
    }

    public ulong GetOwnerClientId()
    {
        return OwnerClientId;
    }    

    public void SetGameStart(bool gameStarted)
    {
        _gameStarted = gameStarted;
    }

    public IEnumerator Init()
    {
        yield return new WaitForSecondsRealtime(1f);

        _gameplayManager = GameObject.Find("GameplayManager").GetComponent<GameplayManager>();
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        _timeScaleController = TimeScaleController.Instance;
        _cameraController = CameraController.Instance;
        _timer = _gameplayManager.GetTimer();
        _soundManager = SoundManager.Instance;

        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (go.GetComponent<PlayerNetwork>() != this) _enemyNetwork = go.GetComponent<PlayerNetwork>();
        }

        _actions = new();
        _actions.Enable();
        _actions.Player.Crouch.performed += CrouchPerformed;
        _actions.Player.Light.performed += LightPerformed;

        _isOwner = IsOwner;

        InitHealthbars(IsHost, _isOwner);
        InitComboCounters(IsHost, _isOwner);

        InitPlayerServerRpc(IsHost);
    }

    public void ResetVariables()
    {
        if (!_isOwner) return;

        _health.Value = 100f;
        _normalizedInputX.Value = default;
        _currentInputX.Value = default;
        _isDead.Value = false;
        _isCrouching.Value = false;
        _isAttacking.Value = false;
        _allowCombo.Value = false;
        _allowHits.Value = true;
        _isHit.Value = false;
        _canBlock.Value = false;
        _isBlocking.Value = false;
        ResetVariablesServerRpc();
        UpdateHealthbarServerRpc(_health.Value);

        InitPlayerServerRpc(IsHost);
    }

    [ServerRpc]
    private void ResetVariablesServerRpc()
    {
        ResetAnimatorClientRpc();
        ResetComboClientRpc("");
    }

    [ClientRpc]
    private void ResetAnimatorClientRpc()
    {
        _animator.SetBool("isCrouching", false);
        _animator.SetFloat("xInput", 0);
        PlayAnimationServerRpc("HorizontalMovement", 0f);
    }

    public int GetWins()
    {
        return _wins.Value;
    }

    [ServerRpc (RequireOwnership = false)]
    public void SetWinsServerRpc(int wins)
    {
        Debug.Log("Setting wins");
        SetWinsClientRpc(wins);
    }

    [ClientRpc]
    public void SetWinsClientRpc(int wins)
    {
        Debug.Log("wins set from " + _wins.Value + " to " + wins);
        _wins.Value = wins;
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

    private void InitComboCounters(bool isHost, bool isOwner)
    {
        ComboCounter[] comboCounters = FindObjectsOfType<ComboCounter>();

        if (isHost)
        {
            if (isOwner) _comboCounter = comboCounters[1];
            else _comboCounter = comboCounters[0];
        }
        else
        {
            if (isOwner) _comboCounter = comboCounters[0];
            else _comboCounter = comboCounters[1];
        }
    }

    [ServerRpc]
    private void InitPlayerServerRpc(bool isHost)
    {
        InitPlayerPosClientRpc(isHost);
        InitCameraClientRpc();
        InitTimerClientRpc();
    }

    [ClientRpc]
    private void InitTimerClientRpc()
    {
        _timer.AddPlayer(this);
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
        if (!_gameStarted) return;

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
        if (!_gameStarted) return;

        _isCrouching.Value = !_isCrouching.Value;
        _animator.SetBool("isCrouching", _isCrouching.Value);
    }

    private void LightPerformed(InputAction.CallbackContext context)
    {
        if (!_isOwner) return;
        if (_isAttacking.Value && !_allowCombo.Value) return;
        if (_isHit.Value) return;
        if (_isBlocking.Value) return;
        if (!_gameStarted) return;

        _isAttacking.Value = true;
        _allowCombo.Value = false;

        if (_isCrouching.Value)
        {
            PlayAnimationServerRpc("Light_Crouch", 0.05f);
            StartCoroutine(PlaySoundDelayed(_audioDelays[5], 5));
        }
        else
        {
            if (_normalizedInputX.Value == 0)
            {
                PlayAnimationServerRpc("Light_Neutral", 0.05f);
                StartCoroutine(PlaySoundDelayed(_audioDelays[3], 3));
            }
            else
            {
                PlayAnimationServerRpc("Light_Forward", 0.05f);
                StartCoroutine(PlaySoundDelayed(_audioDelays[4], 4));
            }
        }
    }

    private IEnumerator PlaySoundDelayed(float delay, int clipId)
    {
        yield return new WaitForSeconds(delay);

        PlaySoundServerRpc(clipId);
    }

    [ServerRpc]
    private void PlaySoundServerRpc(int clipId)
    {
        PlaySoundClientRpc(clipId);
    }

    [ClientRpc]
    private void PlaySoundClientRpc(int clipId)
    {
        _soundManager.PlaySound(_audioClips[clipId]);
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
        if (_projectile != null) return;

        Object projObj = Instantiate(obj, new Vector3(_hand.transform.position.x, _hand.transform.position.y, 0), Quaternion.Inverse(transform.rotation));
        _projectile = projObj as GameObject;
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

    public void GetHit(float damage, Vector3 pos)
    {
        if (!_isOwner) return;
        if (!_allowHits.Value) return;
        if (_isDead.Value) return;

        if (_canBlock.Value)
        {
            _isBlocking.Value = true;
            _canBlock.Value = false;
            PlayAnimationServerRpc("Blocking", 0.05f);
            _enemyNetwork.ResetComboServerRpc("COUNTER");
            ResetComboServerRpc("");
            ShieldEffectsServerRpc();
            return;
        }

        _health.Value -= damage;
        _isHit.Value = true;
        _isAttacking.Value = false;
        _isBlocking.Value = false;

        UpdateHealthbarServerRpc(_health.Value);
        _enemyNetwork.ResetComboServerRpc("");
        AddComboServerRpc();

        if (_health.Value <= 0.0f)
        {
            _isDead.Value = true;
            PlayAnimationServerRpc("Death", 0.05f);
            _enemyNetwork.SetWinsServerRpc(_enemyNetwork.GetWins() + 1);

            if (IsOwnedByServer) EndGameServerRpc(GameManager.Instance.GetCharacters()[1]);
            else EndGameServerRpc(GameManager.Instance.GetCharacters()[0]);
        }
        else PlayAnimationServerRpc("Impact", 0.05f);

        HitEffectsServerRpc(pos, _isDead.Value);
        StartCoroutine(PlaySoundDelayed(_audioDelays[6], 6));
    }

    [ServerRpc]
    private void AddComboServerRpc()
    {
        AddComboClientRpc();
    }

    [ClientRpc]
    private void AddComboClientRpc()
    {
        _comboCounter.AddToCombo();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetComboServerRpc(string text)
    {
        ResetComboClientRpc(text);
    }

    [ClientRpc]
    private void ResetComboClientRpc(string text)
    {
        _comboCounter.ResetCombo(text);
    }

    [ServerRpc]
    private void ShieldEffectsServerRpc()
    {
        ShieldEffectsClientRpc();
    }

    [ClientRpc]
    private void ShieldEffectsClientRpc()
    {
        GameObject effect = Instantiate(_shieldEffect, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), Quaternion.identity);
        effect.transform.LookAt(Camera.main.transform);

        _soundManager.PlaySound(_audioClips[7]);

        StartCoroutine(DeleteEffectAfterTime(1f, effect));
    }

    [ServerRpc]
    private void HitEffectsServerRpc(Vector3 pos, bool dead)
    {
        HitEffectsClientRpc(pos, dead);
    }

    [ClientRpc]
    private void HitEffectsClientRpc(Vector3 pos, bool dead)
    {
        if (dead)
        {
            StartCoroutine(_timeScaleController.HitStop(0.1f, 3f));
            _cameraController.ShakeCamera(3f, 0.5f);
        }
        else
        {
            StartCoroutine(_timeScaleController.HitStop(0.2f, 0.2f));
            _cameraController.ShakeCamera(0.5f, 0.05f);
        }

        GameObject effect = Instantiate(_hitEffect, pos, Quaternion.identity);
        effect.transform.LookAt(Camera.main.transform);
        StartCoroutine(DeleteEffectAfterTime(0.75f, effect));
    }

    private IEnumerator DeleteEffectAfterTime(float time, GameObject effect)
    {
        yield return new WaitForSeconds(time);

        Destroy(effect);
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
        _timer.SetTimerRunning(false);
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

    public float GetHealth()
    {
        return _health.Value;
    }
}
