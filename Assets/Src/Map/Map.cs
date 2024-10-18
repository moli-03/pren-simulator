using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Map : MonoBehaviour
{
    // Assign in editor
    public GameObject NodePrefab;
    public GameObject PathPrefab;
    public LayerMask GroundLayer;
    public LayerMask GraphLayer;

    private Camera MainCamera;

    private List<Node> Nodes = new List<Node>();
    private List<Path> Paths = new List<Path>();

    struct DragInfo
    {
        public Node Target;
        public bool IsDragging;
    }

    private DragInfo? CurrentDrag = null;

    // Start is called before the first frame update
    void Start()
    {
        this.MainCamera = Camera.main;

        // Create all node instances on their default positions
        Node A = Instantiate(NodePrefab, new Vector3(1.5f, 0, 0), Quaternion.identity).GetComponent<Node>().SetLabel("A");
        Node B = Instantiate(NodePrefab, new Vector3(0, 0, 0.5f), Quaternion.identity).GetComponent<Node>().SetLabel("B");
        Node C = Instantiate(NodePrefab, new Vector3(3, 0, 0.5f), Quaternion.identity).GetComponent<Node>().SetLabel("C");
        Node D = Instantiate(NodePrefab, new Vector3(1f, 0, 1f), Quaternion.identity).GetComponent<Node>().SetLabel("D");
        Node E = Instantiate(NodePrefab, new Vector3(0, 0, 1.5f), Quaternion.identity).GetComponent<Node>().SetLabel("E");
        Node F = Instantiate(NodePrefab, new Vector3(1f, 0, 1.5f), Quaternion.identity).GetComponent<Node>().SetLabel("F");
        Node G = Instantiate(NodePrefab, new Vector3(3f, 0, 1.5f), Quaternion.identity).GetComponent<Node>().SetLabel("G");
        Node H = Instantiate(NodePrefab, new Vector3(1.5f, 0, 2f), Quaternion.identity).GetComponent<Node>().SetLabel("H");

        // Add to node list
        this.Nodes.Add(A);
        this.Nodes.Add(B);
        this.Nodes.Add(C);
        this.Nodes.Add(D);
        this.Nodes.Add(E);
        this.Nodes.Add(F);
        this.Nodes.Add(G);
        this.Nodes.Add(H);

        // Add the default connections
        // From A
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(A).To(B));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(A).To(B));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(A).To(C));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(A).To(D));

        // From B
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(B).To(D));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(B).To(E));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(B).To(F));

        // From C
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(C).To(D));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(C).To(G));

        // From D
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(D).To(G));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(D).To(F));

        // From E
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(E).To(F));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(E).To(H));

        // From F
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(F).To(G));
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(F).To(H));

        // From G
        this.Paths.Add(Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(G).To(H));

        // Update the positions for each path
        this.Paths.ForEach(path => path.UpdatePosition());
    }

    // Update is called once per frame
    void Update()
    {
        // Check for mouse press
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(this.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit initialHit, Mathf.Infinity, this.GraphLayer))
            {
                // Mark as selected
                initialHit.collider.GetComponent<Renderer>().material.color = Color.cyan;

                // Store drag info
                this.CurrentDrag = new DragInfo()
                {
                    Target = initialHit.collider.gameObject.GetComponent<Node>(),
                    IsDragging = true
                };
            }
        }


        // Handle logic for dragging
        if (this.CurrentDrag.HasValue && this.CurrentDrag.Value.IsDragging)
        {
            // Check for mouse release
            if (Input.GetMouseButtonUp(0))
            {
                // Back to white again
                this.CurrentDrag.Value.Target.GetComponent<Renderer>().material.color = Color.white;
                this.CurrentDrag = null;
                this.Paths.ForEach(path => path.Line.GetComponent<Renderer>().material.color = Color.white);
            }
            // Not released yet -> update position
            else if (Physics.Raycast(this.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, this.GroundLayer))
            {
                // Get the new position based on the raycasthit
                Vector3 newPosition = new Vector3(hit.point.x, this.CurrentDrag.Value.Target.transform.position.y, hit.point.z);
                
                // Get a list of all affected paths
                List<Path> affectedPaths = this.Paths.Where(path => path.StartNode == this.CurrentDrag.Value.Target || path.EndNode == this.CurrentDrag.Value.Target).ToList();

                // Check if all paths are still in the 0.5m - 2m range
                bool hasInvalidPaths = affectedPaths.Any(path =>
                {
                    // Get the other node
                    Node target = path.StartNode == this.CurrentDrag.Value.Target ? path.EndNode : path.StartNode;

                    // Calculate the new distance
                    Vector3 distance = target.transform.position - newPosition;

                    // Check range
                    bool invalidDistance = distance.magnitude < 0.5f || distance.magnitude > 2f;

                    // Update material color if its correct size or nah
                    path.Line.GetComponent<Renderer>().material.color = invalidDistance ? Color.red : Color.green;

                    return invalidDistance;
                });

                // No invalid paths? -> update position of the node
                if (!hasInvalidPaths)
                {
                    this.CurrentDrag.Value.Target.transform.position = newPosition;

                    // Update all related paths
                    affectedPaths.ForEach(path => path.UpdatePosition());
                }
            }
        }
    }

	public void RemovePath(Path path) {
		this.Paths.Remove(path);
		Destroy(path.gameObject);
	}
}
