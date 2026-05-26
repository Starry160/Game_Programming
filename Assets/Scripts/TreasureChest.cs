using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Animator))]
public class TreasureChest : MonoBehaviour
{
    [FormerlySerializedAs("_potionPrefab")]
    [SerializeField] private GameObject _dropPrefab;
    [SerializeField] private float _moveDistance = 0.8f;
    [SerializeField] private float _spawnSideOffset = 0.25f;
    [SerializeField] private float _spawnVerticalOffset = 0f;
    [SerializeField] private float _randomXJitter = 0.12f;
    [SerializeField] private float _randomYJitter = 0.08f;

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
        OpenChest();
    }

    private void OpenChest()
    {
        if (_animator != null)
        {
            _animator.SetTrigger("open");
        }

        SpawnDrop();
    }

    private void SpawnDrop()
    {
        if (_dropPrefab == null)
        {
            Debug.LogWarning("[TreasureChest] _dropPrefab 未绑定，无法弹出掉落物。", this);
            return;
        }

        float directionX = (Random.value > 0.5f) ? 1f : -1f;
        // 严格保持宝箱同一水平线（同Y）生成，向左右平射。
        Vector3 spawnPosition = transform.position + new Vector3(directionX * _spawnSideOffset, _spawnVerticalOffset, 0f);
        GameObject dropObj = Instantiate(_dropPrefab, spawnPosition, Quaternion.identity);
        ChestDropItem dropItem = dropObj.GetComponent<ChestDropItem>();
        if (dropItem == null)
        {
            dropItem = dropObj.GetComponentInChildren<ChestDropItem>();
        }

        if (dropItem == null)
        {
            Debug.LogWarning($"[TreasureChest] 生成的掉落物对象 {dropObj.name} 未挂 ChestDropItem。", dropObj);
            return;
        }

        float randomX = Random.Range(-_randomXJitter, _randomXJitter);
        float randomY = Random.Range(-_randomYJitter, _randomYJitter);
        float finalMoveX = directionX * _moveDistance + randomX;
        if (Mathf.Sign(finalMoveX) != directionX)
        {
            finalMoveX = directionX * 0.05f;
        }

        Vector2 moveVector = new Vector2(finalMoveX, randomY);
        dropItem.PopOut(moveVector);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position + new Vector3(0f, _spawnVerticalOffset, 0f);
        Vector3 leftSpawn = center + Vector3.left * _spawnSideOffset;
        Vector3 rightSpawn = center + Vector3.right * _spawnSideOffset;

        // Chest horizontal reference line.
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center + Vector3.left * (_spawnSideOffset + 0.4f), center + Vector3.right * (_spawnSideOffset + 0.4f));

        // Left / right spawn points.
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(leftSpawn, 0.05f);
        Gizmos.DrawSphere(rightSpawn, 0.05f);

        // Horizontal move direction guides.
        float guideLen = Mathf.Max(0.2f, _moveDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawLine(leftSpawn, leftSpawn + Vector3.left * guideLen);
        Gizmos.DrawLine(rightSpawn, rightSpawn + Vector3.right * guideLen);
    }
}
