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

    // Stores the previous orbit center position so satellites follow moving centers smoothly.
    private Vector3 lastCenterPosition;

    private void Start()
    {
        if (orbitCenter != null)
        {
            lastCenterPosition = orbitCenter.position;
        }
    }

    private void Update()
    {
        if (orbitCenter != null)
        {
            // Move with the orbit center first so moons are not left behind.
            Vector3 centerMovement = orbitCenter.position - lastCenterPosition;
            transform.position += centerMovement;

            // Rotate around the current center position after applying center movement.
            transform.RotateAround(orbitCenter.position, orbitAxis, orbitSpeed * Time.deltaTime);

            // Save the center position for the next frame's movement delta.
            lastCenterPosition = orbitCenter.position;
        }

        // Apply local self-rotation independently from orbital motion.
        transform.Rotate(selfRotationAxis, selfRotationSpeed * Time.deltaTime, Space.Self);
    }
}
