using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Smoothly follows the player and can play the boss intro camera sequence.</summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [Header("Follow")]
    [Tooltip("Smooth follow time; lower values make the camera catch up faster.")]
    [SerializeField] private float _smoothTime = 0.15f;

    [Tooltip("Maximum camera follow speed; non-positive values disable the cap.")]
    [SerializeField] private float _maxSpeed = -1f;

    [Tooltip("Local XY offset from the followed target.")]
    [SerializeField] private Vector2 _offset = Vector2.zero;

    [Header("Intro Cinematic")]
    [Tooltip("Play the boss intro camera sequence when entering the configured scene.")]
    [SerializeField] private bool _enableIntroCinematic = true;
    [Tooltip("Scene name where the boss intro camera sequence is enabled.")]
    [SerializeField] private string _introSceneName = "Final Boss";
    [Tooltip("Name of the boss object used as the intro camera target.")]
    [SerializeField] private string _bossObjectName = "FinalBoss";
    [SerializeField] private float _holdOnPlayerDuration = 0.8f;
    [SerializeField] private float _moveToBossDuration = 1.2f;
    [SerializeField] private float _holdOnBossDuration = 1.5f;
    [SerializeField] private float _moveBackToPlayerDuration = 1.0f;
    [SerializeField] private float _cinematicSmoothTime = 0.32f;
    [Tooltip("Temporarily disables player movement and attacks during the intro shot.")]
    [SerializeField] private bool _disablePlayerControlDuringIntro = true;

    private Transform _target;
    private float _lockedZ;
    private Vector3 _currentVelocity;
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    private PlayerFacing _playerFacing;

    // Stores the starting offset between the camera and its follow target.
    private void Awake()
    {
        _lockedZ = transform.position.z;
    }

    // Finds the player automatically when no follow target was assigned in the Inspector.
    private void Start()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            _playerController = player;
            _playerAttack = player.GetComponent<PlayerAttack>();
            _playerFacing = player.GetComponent<PlayerFacing>();
            _target = player.transform;
            TryPlayIntroCinematic(player.transform);
        }
        else
        {
            Debug.LogWarning("[CameraController] PlayerController was not found in the scene. The camera will remain still.");
        }
    }

    // Updates camera or visual follow logic after normal Update movement.
    private void LateUpdate()
    {
        if (_target == null)
        {
            return;
        }

        Vector3 desired = new Vector3(
            _target.position.x + _offset.x,
            _target.position.y + _offset.y,
            _lockedZ);

        float maxSpeed = _maxSpeed > 0f ? _maxSpeed : Mathf.Infinity;
        Vector3 smoothed = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref _currentVelocity,
            _smoothTime,
            maxSpeed);

        smoothed.z = _lockedZ;
        transform.position = smoothed;
    }

    /// <summary>
    /// Changes the transform that the camera follows.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        _target = newTarget;
    }

    // Starts the intro cinematic if the current scene requires it.
    private void TryPlayIntroCinematic(Transform playerTransform)
    {
        if (!_enableIntroCinematic)
        {
            return;
        }

        string activeSceneName = SceneManager.GetActiveScene().name;
        if (!string.Equals(activeSceneName, _introSceneName, System.StringComparison.Ordinal))
        {
            return;
        }

        GameObject bossObject = GameObject.Find(_bossObjectName);
        if (bossObject == null)
        {
            return;
        }

        StartCoroutine(IntroCinematicRoutine(playerTransform, bossObject.transform));
    }

    // Moves the camera from player to boss and back during the intro.
    private IEnumerator IntroCinematicRoutine(Transform playerTransform, Transform bossTransform)
    {
        if (_disablePlayerControlDuringIntro)
        {
            SetPlayerControlEnabled(false);
        }

        float originalSmoothTime = _smoothTime;
        _smoothTime = Mathf.Max(0.01f, _cinematicSmoothTime);

        // Hold on player first to build tension before camera move.
        yield return new WaitForSeconds(Mathf.Max(0f, _holdOnPlayerDuration));

        SetTarget(bossTransform);
        yield return new WaitForSeconds(Mathf.Max(0.05f, _moveToBossDuration));
        yield return new WaitForSeconds(Mathf.Max(0f, _holdOnBossDuration));

        SetTarget(playerTransform);
        yield return new WaitForSeconds(Mathf.Max(0.05f, _moveBackToPlayerDuration));

        _smoothTime = originalSmoothTime;
        if (_disablePlayerControlDuringIntro)
        {
            SetPlayerControlEnabled(true);
        }
    }

    // Enables or disables player movement, facing, and attack scripts.
    private void SetPlayerControlEnabled(bool enabled)
    {
        if (_playerController != null)
        {
            _playerController.enabled = enabled;
            Rigidbody2D rb = _playerController.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
        }

        if (_playerAttack != null)
        {
            _playerAttack.enabled = enabled;
        }

        if (_playerFacing != null)
        {
            _playerFacing.enabled = enabled;
        }
    }
}
