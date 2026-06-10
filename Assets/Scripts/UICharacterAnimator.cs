using UnityEngine;
using UnityEngine.UI;

/// <summary>Keeps UI character images at their native sprite size.</summary>
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UICharacterAnimator : MonoBehaviour
{
    private Image _image;

    // Starts the looping idle preview when this UI character becomes visible.
    private void OnEnable()
    {
        RefreshNativeSize();
    }

    // Advances the UI character preview by ping-ponging rotation each frame.
    private void Awake()
    {
        _image = GetComponent<Image>();
        RefreshNativeSize();
    }

    // Keeps editor-time references and collider setup consistent.
    private void OnValidate()
    {
        RefreshNativeSize();
    }

    /// <summary>Keeps UI character images at their native sprite size.</summary>
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
