using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    private void Start()
    {
        // 爆炸/命中等一次性特效播放 0.3 秒后自动销毁，避免场景里残留空物体。
        Destroy(gameObject, 0.3f);
    }
}
