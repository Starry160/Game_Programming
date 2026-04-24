using UnityEngine;

public class SolarCameraController : MonoBehaviour
{
    [Header("Main Camera")]
    public Camera targetCamera;

    [Header("Focus Settings")]
    public Vector3 focusOffset = new Vector3(0f, 1.5f, -4f);
    public float moveLerpSpeed = 4f;
    public float rotateLerpSpeed = 6f;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;
    private Transform currentTarget;
    private bool returningToDefault;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera != null)
        {
            defaultPosition = targetCamera.transform.position;
            defaultRotation = targetCamera.transform.rotation;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        if (currentTarget != null)
        {
            Vector3 targetPos = currentTarget.position + focusOffset;
            Quaternion targetRot = Quaternion.LookRotation(currentTarget.position - targetPos, Vector3.up);

            targetCamera.transform.position = Vector3.Lerp(
                targetCamera.transform.position,
                targetPos,
                moveLerpSpeed * Time.deltaTime
            );
            targetCamera.transform.rotation = Quaternion.Slerp(
                targetCamera.transform.rotation,
                targetRot,
                rotateLerpSpeed * Time.deltaTime
            );
            returningToDefault = false;
        }
        else if (returningToDefault)
        {
            targetCamera.transform.position = Vector3.Lerp(
                targetCamera.transform.position,
                defaultPosition,
                moveLerpSpeed * Time.deltaTime
            );
            targetCamera.transform.rotation = Quaternion.Slerp(
                targetCamera.transform.rotation,
                defaultRotation,
                rotateLerpSpeed * Time.deltaTime
            );
        }
    }

    public void FocusOn(Transform target)
    {
        currentTarget = target;
        returningToDefault = false;
    }

    public void ReturnToMainView()
    {
        currentTarget = null;
        returningToDefault = true;
    }
}
