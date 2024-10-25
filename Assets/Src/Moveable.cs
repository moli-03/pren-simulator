using UnityEngine;

public class Moveable : MonoBehaviour
{
	private CameraController cameraController;

	private bool IsSelected = false;

	private bool Rotate = false;

	private float RotationSpeed = 100f;

	private LayerMask GroundLayer;
	private LayerMask GraphLayer;
	private int Mask;

    // Start is called before the first frame update
    void Start()
    {
        this.cameraController = Camera.main.GetComponent<CameraController>();
		this.GroundLayer = LayerMask.NameToLayer("Ground");
		this.GraphLayer = LayerMask.NameToLayer("Graph");
		this.Mask = (1 << this.GroundLayer) | (1 << this.GraphLayer);
    }

    // Update is called once per frame
    void Update()
    {
		// Check mouse button down
        if (Input.GetMouseButtonDown(0) && !this.IsSelected) {
			// Check what we are looking at
			RaycastHit? currentHit = this.cameraController.GetRaycastHit();

			// Are we dragging this object?
			if (currentHit.HasValue && currentHit.Value.collider.gameObject == this.gameObject) {
				this.IsSelected = true;
			}
		}
		// Check mouse button up
		else if (Input.GetMouseButtonUp(0) && this.IsSelected) {
			this.IsSelected = false;
		}
		// Handle dragging
		else if (this.IsSelected) {
			RaycastHit? currentHit = this.cameraController.GetRaycastHit(this.Mask);

			// Hit anything?
			if (currentHit.HasValue) {

				// Check if we hit the graph
				if (currentHit.Value.collider.gameObject.layer == this.GraphLayer.value) {

					// Check node hit
					if (currentHit.Value.collider.gameObject.name.StartsWith("Node") && this.name.StartsWith("Cone")) {
						Node node = currentHit.Value.collider.GetComponent<Node>();
						node.SetCone(this.gameObject);
					} 
					// Check path hit
					else if (currentHit.Value.collider.transform.parent.name.StartsWith("Path") && this.name.StartsWith("Barrier")) {
						Path path = currentHit.Value.collider.GetComponentInParent<Path>();
						path.SetBarrier(this.gameObject);
					}
				}
				else {
					// Ground it -> update position
					this.transform.position = new Vector3(currentHit.Value.point.x, this.transform.position.y, currentHit.Value.point.z);
				}
			}
		}


		// Start rotation
		if (this.IsSelected && Input.GetMouseButtonDown(1)) {
			this.Rotate = true;
			this.cameraController.DisableMovement();
		}

		// Handle rotation
		if (this.Rotate) {
			if (Input.GetKey(KeyCode.A)) {
				this.transform.Rotate(new Vector3(0, 0, this.RotationSpeed * Time.deltaTime));
			}

			if (Input.GetMouseButtonUp(1)) {
				this.Rotate = false;
				this.cameraController.EnableMovement();
			}
		}

    }
}
