using System.Runtime.InteropServices;
using Assets.Src.Util;
using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for LINQ methods like Skip and Zip



public class IRSensor : MonoBehaviour
{
    public float RayMaxDistance = 1f;    // Distance the laser will shoot

    private LineRenderer lineRenderer;
    private bool drawDebugLine = false;

    private LayerMask GraphLayer;
    private LayerMask GroundLayer;
    
    private Queue<float> recentReadings = new Queue<float>();
        private Queue<float> blackWhiteRecentReadings = new Queue<float>();

    private const int MaxReadings = 10; // Number of frames to store

    void Start()
    {
        this.lineRenderer = this.gameObject.AddComponent<LineRenderer>();
        this.lineRenderer.receiveShadows = false;
        this.lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        this.lineRenderer.useWorldSpace = true;
        this.lineRenderer.startWidth = 0.004f;
        this.lineRenderer.endWidth = 0.004f;
        this.lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        this.lineRenderer.startColor = Color.red;
        this.lineRenderer.endColor = Color.red;
        this.GraphLayer = 1 << LayerMask.NameToLayer("Graph");
        this.GroundLayer = 1 << LayerMask.NameToLayer("Ground");
    }

    public void UpdateBlackWhiteSensorReadings(float newReading)
    {
        blackWhiteRecentReadings.Enqueue(newReading);
        if (blackWhiteRecentReadings.Count > MaxReadings)
        {
            blackWhiteRecentReadings.Dequeue();
        }
    }

   public void UpdateSensorReadings(float newReading)
    {
        recentReadings.Enqueue(newReading);
        if (recentReadings.Count > MaxReadings)
        {
            recentReadings.Dequeue();
        }
    }

    public IEnumerable<float> GetRecentReadings()
    {
        return recentReadings;
    }


    public IEnumerable<float> GetBlackWhiteRecentReadings()
    {
        return blackWhiteRecentReadings;
    }

    public bool IsTransitioning()
    {
        if (recentReadings.Count < MaxReadings) return false; // Not enough data yet

        float first = recentReadings.Peek();
        float last = 0f;
        foreach (float value in recentReadings)
        {
            last = value; // Get the last value
        }

        // Check if the readings show a transition from black to ground or vice versa
        return Mathf.Abs(last - first) > 0.2f; // Adjust the threshold as needed
    }
    public void DrawDebugLine()
    {
        this.drawDebugLine = true;
    }

    public void RemoveDebugLine()
    {
        this.drawDebugLine = false;
    }

    void Update()
    {

        if (this.drawDebugLine)
        {
            Vector3 start = transform.position;
            bool hitSomething = Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, RayMaxDistance);
            Vector3 end = hitSomething ? hit.point : transform.position + this.transform.forward * this.RayMaxDistance;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            this.GetComponent<Renderer>().material.color = Pathing.IsOnLine(this) ? Color.green : Color.black;
        }
    }

    public float GetReflectedLightAndCheckBlackWhite()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, RayMaxDistance, GraphLayer))
        {
            return 0f;
        }

        Renderer renderer = hit.collider.GetComponent<Renderer>();
        if (renderer == null)
        {
            return 0f;
        }

        Color color;

        // Handle objects with a texture
        if (renderer.material.mainTexture != null)
        {
            Texture2D texture = renderer.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;

            if (!texture.isReadable)
            {
                return 0f;
            }

            color = texture.GetPixel((int)pixelUV.x, (int)pixelUV.y);
        }
        // Handle objects without a texture
        else
        {
            color = renderer.material.color;
        }

        // Additional check: Ensure the surface is black on white
        float grayscale = color.grayscale;

        if (grayscale < 0.4f) // Black threshold
        {
            Vector3 surroundingPosition = hit.point + hit.normal * 0.01f;
            if (Physics.Raycast(surroundingPosition, -hit.normal, out RaycastHit surroundingHit, 0.02f))
            {
                Renderer surroundingRenderer = surroundingHit.collider.GetComponent<Renderer>();
                if (surroundingRenderer != null)
                {
                    Color surroundingColor = surroundingRenderer.material.color;

                    if (surroundingColor.grayscale > 0.8f) // White threshold
                    {
                        //Debug.Log("Black on white detected!");
                        return grayscale;
                    }
                }
            }
        }

        return grayscale;
    }


    public float GetReflectedLight()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit hit = new RaycastHit();
        Renderer renderer = null;
        if (!Physics.Raycast(origin, direction, out hit, RayMaxDistance, GraphLayer | GroundLayer))
        {
            Debug.Log("No hit found!");
            return 0f;
        }

        renderer = hit.collider.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.Log("No renderer found!");
            return 0f;
        }

        Color color;

        // Handle objects with textures
        if (renderer.material.mainTexture != null)
        {
            Texture2D texture = renderer.material.mainTexture as Texture2D;
            Vector2 pixelUV = hit.textureCoord;
            pixelUV.x *= texture.width;
            pixelUV.y *= texture.height;

            if (!texture.isReadable)
            {
                Debug.Log("Texture is not readable!");
                return 0f; // Small non-zero value for unreadable textures
            }

            color = texture.GetPixel((int)pixelUV.x, (int)pixelUV.y);
        }
        else
        {
            // Use the material color if no texture is found
            color = renderer.material.color;
        }

        // Return the grayscale value of the color
        return color.grayscale;
    }

}
