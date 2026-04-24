using UnityEngine;

public class OrbitMotion : MonoBehaviour
{
    [Header("Orbit")]
    public Transform orbitCenter;
    public Vector3 orbitAxis = Vector3.up;
    public float orbitSpeed = 10f;

    [Header("Self Rotation")]
    public Vector3 selfRotationAxis = Vector3.up;
    public float selfRotationSpeed = 25f;

    private void Update()
    {
        if (orbitCenter != null)
        {
            transform.RotateAround(
                orbitCenter.position,
                orbitAxis.normalized,
                orbitSpeed * Time.deltaTime
            );
        }

        transform.Rotate(selfRotationAxis.normalized, selfRotationSpeed * Time.deltaTime, Space.Self);
    }
}
