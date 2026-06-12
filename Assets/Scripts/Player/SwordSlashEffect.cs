using UnityEngine;

/// <summary>Short-lived visual-only crescent slash for the knight sword swing.</summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SwordSlashEffect : MonoBehaviour
{
    private const int TextureSize = 64;
    private const float PixelsPerUnit = 64f;

    private static Sprite slashSprite;

    private SpriteRenderer spriteRenderer;
    private Color startColor;
    private Vector3 startScale;
    private Vector3 endScale;
    private float lifetime = 0.12f;
    private float elapsed;

    public void Initialize(
        bool facingRight,
        float duration,
        Vector2 scale,
        int sortingLayerId,
        int sortingOrder)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = GetOrCreateSlashSprite();
        spriteRenderer.sortingLayerID = sortingLayerId;
        spriteRenderer.sortingOrder = sortingOrder;
        spriteRenderer.color = new Color(0.45f, 0.9f, 1f, 0.95f);
        spriteRenderer.flipX = !facingRight;

        lifetime = Mathf.Max(0.01f, duration);
        startColor = spriteRenderer.color;

        startScale = new Vector3(scale.x, scale.y, 1f);
        endScale = new Vector3(scale.x * 1.18f, scale.y * 1.08f, 1f);
        transform.localScale = startScale;
        transform.rotation = Quaternion.Euler(0f, 0f, facingRight ? -18f : 18f);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);
        float eased = 1f - Mathf.Pow(1f - t, 2f);

        transform.localScale = Vector3.Lerp(startScale, endScale, eased);

        if (spriteRenderer != null)
        {
            Color color = startColor;
            color.a = Mathf.Lerp(startColor.a, 0f, eased);
            spriteRenderer.color = color;
        }

        if (elapsed >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private static Sprite GetOrCreateSlashSprite()
    {
        if (slashSprite != null)
        {
            return slashSprite;
        }

        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(0f, 0f, 0f, 0f);
        Color edge = new Color(0.25f, 0.75f, 1f, 0.9f);
        Color core = new Color(0.85f, 1f, 1f, 1f);

        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float nx = ((x + 0.5f) / TextureSize) * 2f - 1f;
                float ny = ((y + 0.5f) / TextureSize) * 2f - 1f;
                float radius = Mathf.Sqrt(nx * nx + ny * ny);
                float angle = Mathf.Atan2(ny, nx) * Mathf.Rad2Deg;

                bool insideArc = radius >= 0.54f
                    && radius <= 0.94f
                    && angle >= -72f
                    && angle <= 68f;

                if (!insideArc)
                {
                    texture.SetPixel(x, y, clear);
                    continue;
                }

                float centerLine = Mathf.InverseLerp(0.54f, 0.94f, radius);
                float edgeBlend = Mathf.Abs(centerLine - 0.55f);
                Color color = Color.Lerp(core, edge, Mathf.Clamp01(edgeBlend * 2.2f));

                if (radius < 0.6f || radius > 0.88f || angle < -62f || angle > 58f)
                {
                    color.a *= 0.65f;
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        slashSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, TextureSize, TextureSize),
            new Vector2(0.5f, 0.5f),
            PixelsPerUnit);
        slashSprite.name = "RuntimeSwordSlashBlue";
        return slashSprite;
    }
}
