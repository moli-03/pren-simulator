using System.Runtime.InteropServices;
using Assets.Src.Util;
using UnityEngine;

public class DistanceSensor : MonoBehaviour
{
    public float RayMaxDistance = 0.5f;    // Distance the laser will shoot

	private LineRenderer lineRenderer;
	private bool drawDebugLine = false;

	void Start() {
		this.lineRenderer = this.gameObject.AddComponent<LineRenderer>();
		this.lineRenderer.receiveShadows = false;
		this.lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		this.lineRenderer.useWorldSpace = true;
        this.lineRenderer.startWidth = 0.004f;
        this.lineRenderer.endWidth = 0.004f;
        this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        this.lineRenderer.startColor = Color.red;
        this.lineRenderer.endColor = Color.red;

		this.DrawDebugLine();
	}

	public void DrawDebugLine() {
		this.drawDebugLine = true;
	}

	public void RemoveDebugLine() {
		this.drawDebugLine = false;
	}

	void Update() {

		if (this.drawDebugLine) {
        	Vector3 start = transform.position;
			bool hitSomething = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, RayMaxDistance);
        	Vector3 end = hitSomething ? hit.point : transform.position + this.transform.forward * this.RayMaxDistance;
        	lineRenderer.SetPosition(0, start);
        	lineRenderer.SetPosition(1, end);
		}
	}


	public GameObject GetHitGameObject() {
			  
        // Define the ray starting position as the current position of the object (origin)
        Vector3 origin = transform.position;

        // Define the direction of the ray, which is along the object's local Z-axis
        Vector3 direction = transform.forward;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, RayMaxDistance))
        {
            return null;
        }

        return hit.collider.gameObject;
	}

    public float GetDistance()
    {
        // Define the ray starting position as the current position of the object (origin)
        Vector3 origin = transform.position;

        // Define the direction of the ray, which is along the object's local Z-axis
        Vector3 direction = transform.forward;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, RayMaxDistance))
        {
            return -1;
        }

        return hit.distance;
    }
}
