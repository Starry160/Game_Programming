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

    // 用于记录圆心（比如地球）上一帧的位置
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
            // 关键修复：先让物体（月球）跟着圆心（地球）同频移动，抵消圆心乱跑导致的甩飞问题
            Vector3 centerMovement = orbitCenter.position - lastCenterPosition;
            transform.position += centerMovement;

            // 然后再围绕圆心进行公转旋转
            transform.RotateAround(orbitCenter.position, orbitAxis, orbitSpeed * Time.deltaTime);

            // 更新记录的圆心位置
            lastCenterPosition = orbitCenter.position;
        }

        // 自身的自转行为
        transform.Rotate(selfRotationAxis, selfRotationSpeed * Time.deltaTime, Space.Self);
    }
}