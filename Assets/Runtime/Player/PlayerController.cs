using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    private bool isSprinting;
    private float moveSpeed;
    private Vector2 moveInput;
    private const float movementForceMultiplier = 10f;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private float groundDrag;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private const float groundCheckDistanceMultiplier = 0.5f;
    private const float groundCheckOffset = 0.2f;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private bool useJumpCooldown;
    [SerializeField] private float airMultiplier;
    private bool canJump;

    [Header("Misc")]
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private Transform orientation;
    private Vector3 moveDirection;
    private Vector3 movementForce;
    private PlayerControls playerControls;


    private void Awake()
    {
        playerControls = new PlayerControls();
    }

    private void Start()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        playerRigidbody.freezeRotation = true;

        canJump = true;
        isSprinting = false;
        moveSpeed = walkSpeed;
        if (!useJumpCooldown) jumpCooldown = 0f;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        SpeedControl();
    }

    private void FixedUpdate()
    {
        isGrounded = IsGrounded();
        MovePlayer();
    }

    private bool IsGrounded() => Physics.Raycast(transform.position, Vector3.down, playerHeight * groundCheckDistanceMultiplier + groundCheckOffset, groundLayer);

    private void MovePlayer()
    {
        if (isSprinting)
            moveSpeed = sprintSpeed;
        else
            moveSpeed = walkSpeed;

        moveDirection = orientation.forward * moveInput.y + orientation.right * moveInput.x;
        movementForce = moveDirection.normalized * moveSpeed * movementForceMultiplier;

        if (isGrounded)
            playerRigidbody.AddForce(movementForce);
        else
            playerRigidbody.AddForce(movementForce * airMultiplier);
    }

    private void SpeedControl()
    {
        Vector3 flatvel = new(playerRigidbody.linearVelocity.x, 0, playerRigidbody.linearVelocity.z);

        if (flatvel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatvel.normalized * moveSpeed;
            playerRigidbody.linearVelocity = new Vector3(limitedVel.x, playerRigidbody.linearVelocity.y, limitedVel.z);
        }
        if (isGrounded) playerRigidbody.linearDamping = groundDrag;
        else playerRigidbody.linearDamping = 0;
    }

    // On is called by the input action asset.

    private void OnMove(InputValue inputvalue) => moveInput = inputvalue.Get<Vector2>();

    private void OnSprint(InputValue inputvalue) => isSprinting = inputvalue.isPressed;

    private void OnJump()
    {
        if (canJump && isGrounded)
        {
            playerRigidbody.linearVelocity = new Vector3(playerRigidbody.linearVelocity.x, 0f, playerRigidbody.linearVelocity.z);
            playerRigidbody.AddForce(Vector2.up * jumpForce, ForceMode.Impulse);
            canJump = false;
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump() => canJump = true;

    private void OnTriggerEnter(Collider other)
    {
    }
}
