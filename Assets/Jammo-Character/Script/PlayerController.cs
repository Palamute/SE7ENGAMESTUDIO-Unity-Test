using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    [Header("Field Bounds")]
    public float fieldMinX = -20f, fieldMaxX = 20f;
    public float fieldMinZ = -12f, fieldMaxZ = 12f;

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveDir = new Vector3(h, 0f, v).normalized;

        if (moveDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            Vector3 newPos = transform.position + moveDir * moveSpeed * Time.deltaTime;
            newPos.x = Mathf.Clamp(newPos.x, fieldMinX, fieldMaxX);
            newPos.z = Mathf.Clamp(newPos.z, fieldMinZ, fieldMaxZ);
            transform.position = newPos;

            animator.SetFloat("speed", 1f);
        }
        else
        {
            animator.SetFloat("speed", 0f);
        }
    }
}