using UnityEngine;

public class Moveable : MonoBehaviour
{
	private CameraController cameraController;

	private bool IsDragged = false;

	private LayerMask GroundLayer;

    // Start is called before the first frame update
    void Start()
    {
        this.cameraController = Camera.main.GetComponent<CameraController>();
		this.GroundLayer = LayerMask.NameToLayer("Ground");
    }

    // Update is called once per frame
    void Update()
    {
		// Check mouse button down
        if (Input.GetMouseButtonDown(0) && !this.IsDragged) {
			
			// Check what we are looking at
			RaycastHit? currentHit = this.cameraController.GetRaycastHit();

			// Are we dragging this object?
			if (currentHit.HasValue && currentHit.Value.collider.gameObject == this.gameObject) {
				this.IsDragged = true;
			}
		}
		// Check mouse button up
		else if (Input.GetMouseButtonUp(0) && this.IsDragged) {
			this.IsDragged = false;
		}
		// Handle dragging
		else if (this.IsDragged) {
			RaycastHit? currentHit = this.cameraController.GetRaycastHit(this.GroundLayer);
			if (currentHit.HasValue) {
				this.transform.position = new Vector3(currentHit.Value.point.x, this.transform.position.y, currentHit.Value.point.z);
			}
		}
    }
}
