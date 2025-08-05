using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float mouseSens = 10f;

    private bool isInputActive = false;
    private float mouseVerticalInput;
    private float mouseHorizontalInput;
    private Camera playerCam;
    private Rigidbody rb;
    private Vector3 currentPosition;
    private Vector3 previousPosition;

    private const float maxVerticalAngle = 90f;
    private const float minVerticalAngle = -90f;

    void Awake()
    {
        playerCam = Camera.main;
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        currentPosition = playerCam.transform.position;
        previousPosition = currentPosition;
    }

    void Update()
    {
        CheckOnWindowFocus();
        HandleMouseLook();
    }

    void FixedUpdate()
    {
        HandlePlayerMovement();
    }

    #region PUBLIC METHODS

    public Vector3 GetPreviousPos() => previousPosition;

    #endregion

    private Vector3 GetMoveInput()
    {
        Vector3 direction = new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        // Clamp vector magnitude to reduce drifting when input is released
        direction = Vector3.ClampMagnitude(direction, 1);
        return direction;
    }

    private float GetMouseVerticalInput()
    {
        float input = Input.GetAxis("Mouse Y");
        input *= mouseSens;
        return -input;
    }

    private float GetMouseHorizontalInput()
    {
        float input = Input.GetAxis("Mouse X");
        input *= mouseSens;
        return input;
    }

    private void CheckOnWindowFocus()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isInputActive = false;
            Cursor.lockState = CursorLockMode.None;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isInputActive = true;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void HandleMouseLook()
    {
        if (isInputActive == false)
            return;

        // Horizontal rotation (rotate the player body)
        mouseHorizontalInput = GetMouseHorizontalInput() * rotateSpeed;
        transform.Rotate(0f, mouseHorizontalInput, 0f, Space.Self);

        // Vertical rotation (rotate the camera only)
        // Adding the vertical rotation in order to clamp it and 
        // prevent the camera rotate passes its min and max angles
        mouseVerticalInput += GetMouseVerticalInput() * rotateSpeed;
        mouseVerticalInput = Mathf.Clamp(mouseVerticalInput, minVerticalAngle, maxVerticalAngle);
        playerCam.transform.localEulerAngles = new(mouseVerticalInput, 0, 0);
    }

    private void HandlePlayerMovement()
    {
        if (isInputActive == false)
            return;

        // Ensure the moving direction is relative to the player's orientation
        // e.g. if the player rotate 90 degrees, the moving direction will follows
        Vector3 targetPosition = transform.TransformDirection(GetMoveInput()) * moveSpeed;
        rb.MovePosition(transform.position + targetPosition * Time.fixedDeltaTime);

        // Update player camera current position in world space
        previousPosition = currentPosition;
        currentPosition = playerCam.transform.position;
    }
}
