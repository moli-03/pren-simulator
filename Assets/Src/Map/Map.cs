using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Map : MonoBehaviour
{
    private List<Node> Nodes = new List<Node>();
    private List<(Node, Node)> Connections = new List<(Node, Node)>();
    private List<GameObject> ConnectionInstances = new List<GameObject>();
    private Camera MainCamera;
    public LayerMask GroundLayer;
    public LayerMask GraphLayer;
    public GameObject NodePrefab;

    private GameObject DraggingTarget;
    private Vector3 DraggingOffset;
    private bool IsDragging = false;

    // Start is called before the first frame update
    void Start()
    {
        this.MainCamera = Camera.main;

        Node A = Instantiate(NodePrefab, new Vector3(1.5f, 0, 0), Quaternion.identity).GetComponent<Node>();
        A.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "A";
        Node B = Instantiate(NodePrefab, new Vector3(0, 0, 0.5f), Quaternion.identity).GetComponent<Node>();
        B.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "B";
        Node C = Instantiate(NodePrefab, new Vector3(3, 0, 0.5f), Quaternion.identity).GetComponent<Node>();
        C.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "C";
        Node D = Instantiate(NodePrefab, new Vector3(1f, 0, 1f), Quaternion.identity).GetComponent<Node>();
        D.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "D";
        Node E = Instantiate(NodePrefab, new Vector3(0, 0, 1.5f), Quaternion.identity).GetComponent<Node>();
        E.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "E";
        Node F = Instantiate(NodePrefab, new Vector3(1f, 0, 1.5f), Quaternion.identity).GetComponent<Node>();
        F.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "F";
        Node G = Instantiate(NodePrefab, new Vector3(3f, 0, 1.5f), Quaternion.identity).GetComponent<Node>();
        G.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "G";
        Node H = Instantiate(NodePrefab, new Vector3(1.5f, 0, 2f), Quaternion.identity).GetComponent<Node>();
        H.transform.Find("Canvas/Letter").GetComponent<TMP_Text>().text = "H";

        this.Nodes.Add(A);
        this.Nodes.Add(B);
        this.Nodes.Add(C);
        this.Nodes.Add(D);
        this.Nodes.Add(E);
        this.Nodes.Add(F);
        this.Nodes.Add(G);
        this.Nodes.Add(H);

        // From A
        this.Connections.Add((A, B));
        this.Connections.Add((A, C));
        this.Connections.Add((A, D));

        // From B
        this.Connections.Add((B, D));
        this.Connections.Add((B, E));
        this.Connections.Add((B, F));

        // From C
        this.Connections.Add((C, D));
        this.Connections.Add((C, G));

        // From D
        this.Connections.Add((D, G));
        this.Connections.Add((D, F));

        // From E
        this.Connections.Add((E, F));
        this.Connections.Add((E, H));

        // From F
        this.Connections.Add((F, G));
        this.Connections.Add((F, H));

        // From G
        this.Connections.Add((G, H));

        // Initial draw
        this.DrawConnections();
    }

    // Update is called once per frame
    void Update()
    {
        // Check for mouse press
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(this.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit initialHit, Mathf.Infinity, this.GraphLayer))
            {
                initialHit.collider.GetComponent<Renderer>().material.color = Color.cyan;
                this.DraggingTarget = initialHit.collider.gameObject;
                this.DraggingOffset.y = 0;
                this.IsDragging = true;
            }
        }

        if (Input.GetMouseButtonUp(0) && IsDragging)
        {
            this.DraggingTarget.GetComponent<Renderer>().material.color = Color.white;
            this.IsDragging = false;
            this.DraggingTarget = null;
            this.DraggingOffset = Vector3.zero;
        }

        if (IsDragging && Physics.Raycast(this.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, this.GroundLayer))
        {
            // Get the new position of the point
            Vector3 newPosition = new Vector3(hit.point.x, this.DraggingTarget.transform.position.y, hit.point.z);

            // Check if its in the 0.5m - 2m range
            List<(Node, Node)> relevantConnections = this.Connections.Where(connection => connection.Item1 == this.DraggingTarget || connection.Item2 == this.DraggingTarget).ToList();

            bool hasInvalidConnections = this.Connections.Any(connection =>
            {
                Node target = connection.Item1 == this.DraggingTarget ? connection.Item2 : connection.Item1;
                Vector3 distance = target.transform.position - newPosition;
                Debug.Log(distance + ", Magnitude: " + distance.magnitude);
                return distance.magnitude < 0.5f || distance.magnitude > 2f;
            });

            if (!hasInvalidConnections || true)
            {
                this.DraggingTarget.transform.position = newPosition;
                this.WipeConnections();
                this.DrawConnections();
            }
        }
    }

    void WipeConnections()
    {
        this.ConnectionInstances.ForEach(instance => Destroy(instance));
    }

    void DrawConnections()
    {

        // Draw the connections
        this.Connections.ForEach(connection =>
        {
            // Create the plane
            GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // Calcualte the vector between the nodes
            Vector3 connectionVector = connection.Item2.transform.position - connection.Item1.transform.position;

            // Apply the scale based on the calculated vector
            plane.transform.localScale = new Vector3(connectionVector.magnitude, 0.001f, 0.03f);

            // Start from Node 1
            plane.transform.localPosition = connection.Item1.transform.position + 0.5f * connectionVector;

            // Calculate the angle on the Y-axis using Atan2 (XZ plane)
            float angle = Mathf.Atan2(connectionVector.z, connectionVector.x) * Mathf.Rad2Deg;

            // Create a new rotation based on that angle (rotation around Y-axis)
            Quaternion rotation = Quaternion.Euler(0, -angle, 0);

            // Set rotation
            plane.transform.localRotation = rotation;

            this.ConnectionInstances.Add(plane);
        });
    }
}
