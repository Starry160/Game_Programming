using UnityEngine;

/// <summary>一次性特效：播放后延时自动销毁 GameObject。</summary>
public class AutoDestroy : MonoBehaviour
{
    // 启动时注册销毁计时。
    private void Start()
    {
        // 爆炸/命中等一次性特效播放 0.3 秒后自动销毁，避免场景里残留空物体。
        Destroy(gameObject, 0.3f);
    }
}
