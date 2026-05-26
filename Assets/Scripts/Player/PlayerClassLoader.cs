using UnityEngine;

/// <summary>关卡加载时从 GlobalData 恢复玩家职业动画控制器。</summary>
[RequireComponent(typeof(Animator))]
public class PlayerClassLoader : MonoBehaviour
{
    // 应用上次祭坛选择的 Animator Controller。
    private void Start()
    {
        // 从全局库读取玩家上一次选择的职业动画控制器，并应用到自身 Animator。
        if (GlobalData.chosenAnimatorController == null)
        {
            return;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = GlobalData.chosenAnimatorController;
        }
    }
}
