using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IRSensor : MonoBehaviour
{
    public float RayMaxDistance = 100f;    // Distance the laser will shoot

    void Start()
    {

    }


    public float GetReflectedLight()
    {
        // Define the ray starting position as the current position of the object (origin)
        Vector3 origin = transform.position;

        // Define the direction of the ray, which is along the object's local Z-axis
        Vector3 direction = transform.up;

        if (!Physics.Raycast(origin, direction, out RaycastHit hit, RayMaxDistance))
        {
            return 0;
        }

        // Try to get the Renderer component of the hit object
        Renderer renderer = hit.collider.GetComponent<Renderer>();

        Color color;

        // Color of gameobjects with a texture
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
        // Color of gameobjects without a texture (lines etc.)
        else if (renderer != null)
        {
            color = renderer.material.color;
        }
        else
        {
            return 0f;
        }

        // Convert the color to grayscale (luminance)
        // Grayscale value is an approximation of how bright the color is.
        float grayscale = color.r * 0.299f + color.g * 0.587f + color.b * 0.114f;

        // Simulate infrared output: map grayscale (0 = cold/black, 1 = hot/white)
        // You could apply custom scaling or thresholds here based on your sensor's range.
        return grayscale; // This is the "temperature" in infrared terms.
    }
}
