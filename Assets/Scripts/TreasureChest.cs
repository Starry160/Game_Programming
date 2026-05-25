using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class TreasureChest : MonoBehaviour
{
    private BoxCollider2D _triggerCollider;
    private Animator _animator;
    private bool _isOpened = false;

    private void Start()
    {
        _triggerCollider = GetComponent<BoxCollider2D>();
        _animator = GetComponent<Animator>();

        if (_triggerCollider != null)
        {
            _triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_isOpened)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isOpened = true;

        if (_animator != null)
        {
            _animator.SetTrigger("open");
        }
    }
}
