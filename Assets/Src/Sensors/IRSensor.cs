using System.Runtime.InteropServices;
using Assets.Src.Util;
using UnityEngine;

public class IRSensor : MonoBehaviour
{
    public float RayMaxDistance = 1f;    // Distance the laser will shoot

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

			this.GetComponent<Renderer>().material.color = Pathing.IsOnLine(this) ? Color.green : Color.black;
		}
	}

    public float GetReflectedLight()
    {
        // Define the ray starting position as the current position of the object (origin)
        Vector3 origin = transform.position;

        // Define the direction of the ray, which is along the object's local Z-axis
        Vector3 direction = transform.forward;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, RayMaxDistance))
        {
            return 0;
        }

        // Try to get the Renderer component of the hit object
        Renderer renderer = hit.collider.GetComponent<Renderer>();

        Color color;

        // Color of gameObjects with a texture
        if (renderer != null && renderer.material.mainTexture != null)
        {
            // Get the texture from the object
            Texture2D texture = renderer.material.mainTexture as Texture2D;

            // Get the UV coordinates of the hit point
            Vector2 pixelUV = hit.textureCoord;

            // Convert UV coordinates to texture pixel coordinates
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;

            // Get the color at the pixel coordinates
            color = texture.GetPixel((int)pixelUV.x, (int)pixelUV.y);
        }
        // Color of gameObjects without a texture (lines etc.)
        else if (renderer != null)
        {
            color = renderer.material.color;
        }
        else
        {
            return 0f;
        }
		
		// Return the grayscale value of the color
		return color.grayscale;
    }
}
