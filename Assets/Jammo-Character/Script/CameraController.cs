using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform playerTarget;
    public Vector3 offset = new Vector3(0f, 12f, -6f);
    public float followSpeed = 8f;

    private Transform currentTarget;

    void Start()
    {
        currentTarget = playerTarget;
    }

    void LateUpdate()
    {
        if (currentTarget == null)
        {
            currentTarget = playerTarget;
            return;
        }

        Vector3 desiredPos = currentTarget.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, followSpeed * Time.deltaTime);
        transform.LookAt(currentTarget.position);
    }

    public void SwitchToBall(Transform ball)
    {
        currentTarget = ball;
    }

    public void SwitchToPlayer()
    {
        currentTarget = playerTarget;
    }
}