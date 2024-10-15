using TMPro;
using UnityEngine;

public class Path : MonoBehaviour
{
    public Node StartNode { get; private set; } = null;
    public Node EndNode { get; private set; } = null;

    public GameObject Line;

    public TMP_Text Label;

    public Path From(Node node)
    {
        this.StartNode = node;
        return this;
    }

    public Path To(Node node)
    {
        this.EndNode = node;
        return this;
    }

    public void UpdatePosition()
    {
        // Calcualte the vector between the nodes
        Vector3 connectionVector = this.EndNode.transform.position - this.StartNode.transform.position;

        // Apply the scale based on the calculated vector
        this.Line.transform.localScale = new Vector3(connectionVector.magnitude, 0.001f, 0.03f);

        // Start from Node 1
        this.transform.localPosition = this.StartNode.transform.position + 0.5f * connectionVector;

        // Calculate the angle on the y-axis for the connection
        float angle = Mathf.Atan2(connectionVector.z, connectionVector.x) * Mathf.Rad2Deg;

        // Create a new rotation based on that angle (rotation around Y-axis)
        Quaternion rotation = Quaternion.Euler(0, -angle, 0);

        // Set rotation
        this.transform.localRotation = rotation;

        // Update the label
        this.SetLabel(connectionVector.magnitude.ToString("F2") + "m");
    }


    public void SetLabel(string label)
    {
        this.Label.text = label;
    }
}
