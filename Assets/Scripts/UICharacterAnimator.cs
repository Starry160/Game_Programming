using UnityEngine;
using UnityEngine.UI;

/// <summary>UI 角色辅助：保证 Image 以原始像素尺寸显示。</summary>
[RequireComponent(typeof(Image))]
public class UICharacterAnimator : MonoBehaviour
{
    private Image _image;

    private void Awake()
    {
        _image = GetComponent<Image>();
        RefreshNativeSize();
    }

    /// <summary>手动刷新 Image 的 Native Size。</summary>
    public void RefreshNativeSize()
    {
        if (_image == null)
        {
            _image = GetComponent<Image>();
        }

        if (_image != null)
        {
            _image.SetNativeSize();
        }
    }
}
