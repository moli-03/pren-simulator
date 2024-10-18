using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 6.0f;  // Speed at which the camera moves
    public float sensitivity = 4.0f; // Mouse sensitivity for looking around
    private float rotationY = 0.0f;  // Store the vertical rotation
	private Dictionary<int, RaycastHit?> currentRaycastHits = new Dictionary<int, RaycastHit?>();
	private bool movementEnabled = true;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
		// Reset all current raycast hits
		this.currentRaycastHits.Clear();

        // Handle keyboard movement
		if (this.movementEnabled) {
        	MoveCamera();
		}

        // Handle mouse look
        LookAround();
    }


	public void DisableMovement() {
		this.movementEnabled = false;
	}

	public void EnableMovement() {
		this.movementEnabled = true;
	}


	/// <summary>
	/// Performs a raycast with the main camera and returns the result
	/// </summary>
	/// <returns></returns>
	public RaycastHit? GetRaycastHit() {
		if (this.currentRaycastHits.ContainsKey(-1)) {
			return this.currentRaycastHits[-1];
		}
	
        if (!Physics.Raycast(this.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) {
			this.currentRaycastHits[-1] = null;
			return null;
		}

		this.currentRaycastHits[-1] = hit;
		return hit;
	}


	/// <summary>
	/// Returns the current raycast hit on a layer
	/// </summary>
	/// <param name="layer"></param>
	/// <returns></returns>
	public RaycastHit? GetRaycastHit(LayerMask layer) {
		if (this.currentRaycastHits.ContainsKey(layer.value)) {
			return this.currentRaycastHits[layer.value];
		}

        if (!Physics.Raycast(this.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layer)) {
			this.currentRaycastHits[layer.value] = null;
			return null;
		}

		this.currentRaycastHits[layer.value] = hit;
		return hit;
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
