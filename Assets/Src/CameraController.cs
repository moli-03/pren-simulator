using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 6.0f;  // Speed at which the camera moves
    public float sensitivity = 4.0f; // Mouse sensitivity for looking around
    private float rotationY = 0.0f;  // Store the vertical rotatio

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Handle keyboard movement
        MoveCamera();

        // Handle mouse look
        LookAround();
    }

    private void MoveCamera()
    {
        // Get input from WASD keys
        float moveHorizontal = Input.GetAxis("Horizontal"); // A and D keys
        float moveVertical = Input.GetAxis("Vertical");     // W and S keys

        // Create a direction vector based on input
        Vector3 moveDirection = new Vector3(moveHorizontal, 0, moveVertical);
        moveDirection = transform.TransformDirection(moveDirection); // Convert to world space

        // Move the camera based on input
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void LookAround()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity;

        // Apply mouse movement to camera rotation
        rotationY -= mouseY;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Clamp vertical rotation to avoid flipping

        // Rotate the camera
        transform.localRotation = Quaternion.Euler(rotationY, transform.localEulerAngles.y + mouseX, 0);
    }
}
