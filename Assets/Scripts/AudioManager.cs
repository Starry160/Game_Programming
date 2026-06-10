using UnityEngine;

/// <summary>Provides shared hit sound playback for player and enemy feedback.</summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>Provides shared hit sound playback for player and enemy feedback.</summary>
    public static AudioManager Instance;

    [Header("Hit Sounds")]
    [SerializeField] private AudioClip _playerHitSound;
    [SerializeField] private AudioClip _enemyHitSound;

    [Header("Playback")]
    [SerializeField, Range(0f, 1f)] private float _playerHitVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _enemyHitVolume = 1f;

    private AudioSource _audioSource;

    // Builds the singleton audio service that other gameplay scripts call for shared sound effects.
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
    }

    // Plays the player hit sound effect.
    public void PlayPlayerHit()
    {
        if (_audioSource == null || _playerHitSound == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_playerHitSound, _playerHitVolume);
    }

    // Plays the enemy hit sound effect.
    public void PlayEnemyHit()
    {
        if (_audioSource == null || _enemyHitSound == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_enemyHitSound, _enemyHitVolume);
    }
}
