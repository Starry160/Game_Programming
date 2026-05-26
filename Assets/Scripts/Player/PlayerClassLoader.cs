using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerClassLoader : MonoBehaviour
{
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
