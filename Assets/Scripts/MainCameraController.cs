using UnityEngine;

public class MainCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float fastMoveMultiplier = 2f;
    [SerializeField] private float verticalMoveSpeed = 6f;
    [SerializeField] private float scrollSpeedStep = 2f;
    [SerializeField] private float minMoveSpeed = 2f;
    [SerializeField] private float maxMoveSpeed = 25f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 2f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 currentRotation = transform.eulerAngles;
        yaw = currentRotation.y;
        pitch = currentRotation.x;

        if (pitch > 180f)
        {
            pitch -= 360f;
        }
    }

    private void Update()
    {
        UpdateLook();
        UpdateMovement();
        UpdateSpeedFromScroll();
    }

    private void UpdateLook()
    {
        if (!Input.GetMouseButton(1))
        {
            return;
        }

        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateMovement()
    {
        float currentMoveSpeed = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentMoveSpeed *= fastMoveMultiplier;
        }

        Vector3 moveDirection = Vector3.zero;
        moveDirection += transform.forward * Input.GetAxisRaw("Vertical");
        moveDirection += transform.right * Input.GetAxisRaw("Horizontal");

        if (Input.GetKey(KeyCode.E))
        {
            moveDirection += Vector3.up * verticalMoveSpeed;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            moveDirection += Vector3.down * verticalMoveSpeed;
        }

        transform.position += moveDirection * currentMoveSpeed * Time.deltaTime;
    }

    private void UpdateSpeedFromScroll()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Approximately(scroll, 0f))
        {
            return;
        }

        moveSpeed = Mathf.Clamp(moveSpeed + scroll * scrollSpeedStep, minMoveSpeed, maxMoveSpeed);
    }
}
