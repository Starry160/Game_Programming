using System.Collections;
using UnityEngine;

/// <summary>
/// Final Boss 两阶段主控：负责阶段切换、技能调度、移动窗口与死亡收尾。
/// 技能发射仍由动画事件 -> Launcher 执行，保持解耦。
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class FinalBossController : MonoBehaviour
{
    private enum BossPhase
    {
        Phase1,
        Transition,
        Phase2,
        Defeated
    }

    private enum BossAction
    {
        CastArm,
        CastLaser
    }

    [System.Serializable]
    private struct PhaseTuning
    {
        public float moveWindowMin;
        public float moveWindowMax;
        public float recoverWindowMin;
        public float recoverWindowMax;
        public float moveSpeed;
        public float armWeight;
        public float laserWeight;
        public int armComboMin;
        public int armComboMax;
    }

    [Header("Health / Phase")]
    [SerializeField] private float maxHealth = 40f;
    [SerializeField] private float phase2ThresholdRatio = 0.5f;
    [SerializeField] private float phaseTransitionDuration = 1.2f;

    [Header("Arena Movement")]
    [SerializeField] private float orbitRadius = 2.8f;
    [SerializeField] private float retargetInterval = 0.7f;
    [SerializeField] private float stopDistance = 0.18f;

    [Header("Action Timing")]
    [SerializeField] private float castArmAnimDuration = 0.85f;
    [SerializeField] private float castLaserAnimDuration = 0.75f;
    [SerializeField] private float armComboGap = 0.3f;
    [SerializeField] private float minTelegraphTime = 0.25f;

    [Header("Phase Tuning")]
    [SerializeField] private PhaseTuning phase1 = new PhaseTuning
    {
        moveWindowMin = 2.0f,
        moveWindowMax = 3.0f,
        recoverWindowMin = 0.7f,
        recoverWindowMax = 1.0f,
        moveSpeed = 2.0f,
        armWeight = 0.6f,
        laserWeight = 0.4f,
        armComboMin = 1,
        armComboMax = 1
    };

    [SerializeField] private PhaseTuning phase2 = new PhaseTuning
    {
        moveWindowMin = 1.2f,
        moveWindowMax = 2.0f,
        recoverWindowMin = 0.45f,
        recoverWindowMax = 0.7f,
        moveSpeed = 2.8f,
        armWeight = 0.65f,
        laserWeight = 0.35f,
        armComboMin = 2,
        armComboMax = 3
    };

    [Header("Phase 2 Immune Show")]
    [SerializeField] private int immuneShowEveryMinActions = 2;
    [SerializeField] private int immuneShowEveryMaxActions = 3;
    [SerializeField] private float immuneShowDurationMin = 0.5f;
    [SerializeField] private float immuneShowDurationMax = 0.8f;

    [Header("References (Optional)")]
    [SerializeField] private Transform moveAnchor;
    [SerializeField] private FinalBossArmLauncher armLauncher;
    [SerializeField] private FinalBossLaserLauncher laserLauncher;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private Transform _playerTransform;

    private float _currentHealth;
    private BossPhase _phase = BossPhase.Phase1;
    private bool _isImmune;
    private bool _isBusy;
    private bool _movementActive;
    private Vector2 _moveTarget;
    private float _nextRetargetTime;

    private BossAction _lastAction;
    private int _sameActionStreak;
    private int _actionsSinceImmuneShow;
    private int _nextImmuneShowCount;
    private bool _hasIsRunningParam;

    private static readonly int IsRunningHash = Animator.StringToHash("isRunning");

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDefeated => _phase == BossPhase.Defeated;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

        if (moveAnchor == null)
        {
            moveAnchor = transform;
        }

        if (armLauncher == null)
        {
            armLauncher = GetComponent<FinalBossArmLauncher>();
        }

        if (laserLauncher == null)
        {
            laserLauncher = GetComponent<FinalBossLaserLauncher>();
        }

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        _hasIsRunningParam = false;
        if (_animator != null)
        {
            AnimatorControllerParameter[] parameters = _animator.parameters;
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter parameter = parameters[i];
                if (parameter == null)
                {
                    continue;
                }

                if (parameter.type == AnimatorControllerParameterType.Bool &&
                    parameter.nameHash == IsRunningHash)
                {
                    _hasIsRunningParam = true;
                    break;
                }
            }
        }
    }

    private void Start()
    {
        _currentHealth = Mathf.Max(1f, maxHealth);
        _playerTransform = GameObject.FindWithTag("Player")?.transform;
        _nextImmuneShowCount = Random.Range(
            Mathf.Max(1, immuneShowEveryMinActions),
            Mathf.Max(immuneShowEveryMinActions, immuneShowEveryMaxActions) + 1);

        PlayState("Idle");
        StartCoroutine(BossLoop());
    }

    private void FixedUpdate()
    {
        if (!_movementActive || _phase == BossPhase.Defeated)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        Vector2 currentPos = _rb.position;
        Vector2 delta = _moveTarget - currentPos;
        if (delta.sqrMagnitude <= stopDistance * stopDistance)
        {
            _rb.velocity = Vector2.zero;
            return;
        }

        float speed = GetPhaseTuning().moveSpeed;
        Vector2 velocity = delta.normalized * speed;
        _rb.velocity = velocity;
        UpdateFacing(velocity.x);
    }

    public void TakeDamage(float amount)
    {
        if (_phase == BossPhase.Defeated || _isImmune || amount <= 0f)
        {
            return;
        }

        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        if (_currentHealth <= 0f)
        {
            HandleDefeated();
            return;
        }

        if (_phase == BossPhase.Phase1 && _currentHealth <= maxHealth * Mathf.Clamp01(phase2ThresholdRatio))
        {
            StartCoroutine(PhaseTransitionRoutine());
        }
    }

    private IEnumerator BossLoop()
    {
        while (_phase != BossPhase.Defeated)
        {
            if (_phase == BossPhase.Transition || _isBusy)
            {
                yield return null;
                continue;
            }

            if (_phase == BossPhase.Phase2 && _actionsSinceImmuneShow >= _nextImmuneShowCount)
            {
                yield return PlayImmuneShowRoutine();
            }

            yield return MoveWindowRoutine();

            BossAction action = SelectAction();
            yield return ExecuteActionRoutine(action);

            float recover = Random.Range(GetPhaseTuning().recoverWindowMin, GetPhaseTuning().recoverWindowMax);
            yield return new WaitForSeconds(recover);
        }
    }

    private IEnumerator MoveWindowRoutine()
    {
        _movementActive = true;
        _isBusy = false;
        PlayState("Moving");
        SetAnimatorRunning(true);

        float duration = Random.Range(GetPhaseTuning().moveWindowMin, GetPhaseTuning().moveWindowMax);
        float timer = 0f;
        _nextRetargetTime = 0f;

        while (timer < duration && _phase != BossPhase.Defeated && _phase != BossPhase.Transition)
        {
            timer += Time.deltaTime;
            if (Time.time >= _nextRetargetTime)
            {
                RetargetAroundPlayer();
                _nextRetargetTime = Time.time + Mathf.Max(0.1f, retargetInterval);
            }

            yield return null;
        }

        _movementActive = false;
        _rb.velocity = Vector2.zero;
        SetAnimatorRunning(false);
        PlayState("Idle");
    }

    private IEnumerator ExecuteActionRoutine(BossAction action)
    {
        _isBusy = true;
        _movementActive = false;
        _rb.velocity = Vector2.zero;
        SetAnimatorRunning(false);

        if (action == BossAction.CastArm)
        {
            int combo = Random.Range(GetPhaseTuning().armComboMin, GetPhaseTuning().armComboMax + 1);
            for (int i = 0; i < combo; i++)
            {
                PlayState("Casting");
                yield return new WaitForSeconds(Mathf.Max(minTelegraphTime, castArmAnimDuration));

                if (i < combo - 1)
                {
                    yield return new WaitForSeconds(Mathf.Max(0.05f, armComboGap));
                }
            }
        }
        else
        {
            PlayState("LaserBeam");
            yield return new WaitForSeconds(Mathf.Max(minTelegraphTime, castLaserAnimDuration));
        }

        RegisterAction(action);
        PlayState("Idle");
        _isBusy = false;
    }

    private IEnumerator PlayImmuneShowRoutine()
    {
        _isBusy = true;
        _isImmune = true;
        _movementActive = false;
        _rb.velocity = Vector2.zero;
        SetAnimatorRunning(false);

        PlayState("Immune");
        float showTime = Random.Range(immuneShowDurationMin, immuneShowDurationMax);
        yield return new WaitForSeconds(showTime);

        _isImmune = false;
        _actionsSinceImmuneShow = 0;
        _nextImmuneShowCount = Random.Range(
            Mathf.Max(1, immuneShowEveryMinActions),
            Mathf.Max(immuneShowEveryMinActions, immuneShowEveryMaxActions) + 1);

        PlayState("Idle");
        _isBusy = false;
    }

    private IEnumerator PhaseTransitionRoutine()
    {
        if (_phase != BossPhase.Phase1)
        {
            yield break;
        }

        _phase = BossPhase.Transition;
        _isBusy = true;
        _isImmune = true;
        _movementActive = false;
        _rb.velocity = Vector2.zero;
        SetAnimatorRunning(false);

        PlayState("Glowing");
        yield return new WaitForSeconds(Mathf.Max(0.1f, phaseTransitionDuration));

        _phase = BossPhase.Phase2;
        _isImmune = false;
        _isBusy = false;
        PlayState("Idle");
    }

    private void HandleDefeated()
    {
        if (_phase == BossPhase.Defeated)
        {
            return;
        }

        _phase = BossPhase.Defeated;
        _isImmune = true;
        _isBusy = true;
        _movementActive = false;
        _rb.velocity = Vector2.zero;
        SetAnimatorRunning(false);

        if (laserLauncher != null)
        {
            laserLauncher.StopLaser();
        }

        PlayState("Defeated");
    }

    private BossAction SelectAction()
    {
        float armWeight = Mathf.Max(0.01f, GetPhaseTuning().armWeight);
        float laserWeight = Mathf.Max(0.01f, GetPhaseTuning().laserWeight);

        if (_sameActionStreak >= 2)
        {
            return _lastAction == BossAction.CastArm ? BossAction.CastLaser : BossAction.CastArm;
        }

        float sum = armWeight + laserWeight;
        float roll = Random.Range(0f, sum);
        return roll <= armWeight ? BossAction.CastArm : BossAction.CastLaser;
    }

    private void RegisterAction(BossAction action)
    {
        if (_lastAction == action)
        {
            _sameActionStreak++;
        }
        else
        {
            _lastAction = action;
            _sameActionStreak = 1;
        }

        _actionsSinceImmuneShow++;
    }

    private void RetargetAroundPlayer()
    {
        if (_playerTransform == null)
        {
            _playerTransform = GameObject.FindWithTag("Player")?.transform;
            if (_playerTransform == null)
            {
                _moveTarget = _rb.position;
                return;
            }
        }

        Vector2 playerPos = _playerTransform.position;
        Vector2 randomOnCircle = Random.insideUnitCircle.normalized;
        if (randomOnCircle.sqrMagnitude < 0.0001f)
        {
            randomOnCircle = Vector2.right;
        }

        _moveTarget = playerPos + randomOnCircle * orbitRadius;
    }

    private void UpdateFacing(float velocityX)
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        if (velocityX > 0.01f)
        {
            _spriteRenderer.flipX = false;
        }
        else if (velocityX < -0.01f)
        {
            _spriteRenderer.flipX = true;
        }
    }

    private void SetAnimatorRunning(bool value)
    {
        if (_animator == null)
        {
            return;
        }

        if (!_hasIsRunningParam)
        {
            return;
        }

        _animator.SetBool(IsRunningHash, value);
    }

    private void PlayState(string stateName)
    {
        if (_animator == null || string.IsNullOrEmpty(stateName))
        {
            return;
        }

        _animator.Play(stateName, 0, 0f);
    }

    private PhaseTuning GetPhaseTuning()
    {
        return _phase == BossPhase.Phase2 ? phase2 : phase1;
    }
}
