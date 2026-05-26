using UnityEngine;

/// <summary>全局音频管理器：统一播放主角/敌人受击音效。</summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>全局单例，供任意脚本直接调用。</summary>
    public static AudioManager Instance;

    [Header("Hit Sounds")]
    [SerializeField] private AudioClip _playerHitSound;
    [SerializeField] private AudioClip _enemyHitSound;

    [Header("Playback")]
    [SerializeField, Range(0f, 1f)] private float _playerHitVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _enemyHitVolume = 1f;

    private AudioSource _audioSource;

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

        // 受击为 2D UI/反馈音，不使用空间衰减。
        _audioSource.spatialBlend = 0f;
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
    }

    public void PlayPlayerHit()
    {
        if (_audioSource == null || _playerHitSound == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_playerHitSound, _playerHitVolume);
    }

    public void PlayEnemyHit()
    {
        if (_audioSource == null || _enemyHitSound == null)
        {
            return;
        }

        _audioSource.PlayOneShot(_enemyHitSound, _enemyHitVolume);
    }
}
