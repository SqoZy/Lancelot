using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraMovement : MonoBehaviour
{
    [Header("Camera Movement")]
    [SerializeField] private float mouseSensitivity = 0.5f;
    private float sensMultiplier = 0.0075f;

    [SerializeField] private Transform playerTransform;

    private float xRotation;
    private float yRotation;
    private Vector2 mouseInput;

    void Update()
    {
        CameraMovement();
    }

    private void CameraMovement()
    {
        float mouseX = mouseInput.x * mouseSensitivity * sensMultiplier;
        float mouseY = mouseInput.y * mouseSensitivity * sensMultiplier;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        yRotation += mouseX;

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerTransform.localRotation = Quaternion.Euler(0f, yRotation, 0f);
    }

    // On is called by the input action asset.
    private void OnLook(InputValue inputValue) => mouseInput = inputValue.Get<Vector2>();
}
