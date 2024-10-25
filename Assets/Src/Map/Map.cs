using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour
{
    // Assign in editor
    public GameObject NodePrefab;
    public GameObject PathPrefab;
    public LayerMask GroundLayer;
    public LayerMask GraphLayer;
    private Camera MainCamera;
	public static readonly int NODE_COUNT = 8;

    public List<Node> Nodes = new List<Node>();
	public Path[,] PathMatrix = new Path[NODE_COUNT, NODE_COUNT];

    struct DragInfo
    {
        public Node Target;
        public bool IsDragging;
    }

    private DragInfo? CurrentDrag = null;

	public delegate void MapChanged();
	public event MapChanged OnMapChanged;

	public void TriggerMapChange() {
		this.OnMapChanged?.Invoke();
	}

	private void AddPath(Node from, Node to) {
		int fromIndex = from.Index;
		int toIndex = to.Index;
		Path path = Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(from).To(to);
		this.PathMatrix[fromIndex, toIndex] = path;
		this.PathMatrix[toIndex, fromIndex] = path;
	}


	private void AddNode(Node node) {
		node.SetMap(this);
		this.Nodes.Add(node);
		node.Index = this.Nodes.Count - 1;
	}

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
        this.AddNode(A);
        this.AddNode(B);
        this.AddNode(C);
        this.AddNode(D);
        this.AddNode(E);
        this.AddNode(F);
        this.AddNode(G);
        this.AddNode(H);

        // Add the default connections
        // From A
		this.AddPath(A, B);
		this.AddPath(A, C);
		this.AddPath(A, D);

        // From B
		this.AddPath(B, D);
		this.AddPath(B, E);
		this.AddPath(B, F);

        // From C
		this.AddPath(C, D);
		this.AddPath(C, G);

        // From D
		this.AddPath(D, G);
		this.AddPath(D, F);

        // From E
		this.AddPath(E, F);
		this.AddPath(E, H);

        // From F
		this.AddPath(F, G);
		this.AddPath(F, H);

        // From G
		this.AddPath(G, H);

        // Update the positions for each path
		foreach (Path path in this.PathMatrix) {

			if (path == null) continue;

			path.UpdatePosition();
		}

		this.OnMapChanged?.Invoke();
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
				foreach (Path path in this.PathMatrix) {
					if (path == null) continue;
					path.Line.GetComponent<Renderer>().material.color = Color.white;
				}
            }
            // Not released yet -> update position
            else if (Physics.Raycast(this.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, this.GroundLayer))
            {
                // Get the new position based on the raycasthit
                Vector3 newPosition = new Vector3(hit.point.x, this.CurrentDrag.Value.Target.transform.position.y, hit.point.z);
                
				List<Path> affectedPaths = new List<Path>();
				bool hasInvalidPaths = false;
				for (int i = 0; i < this.PathMatrix.GetLength(1); i++) {

					// Get the path we want to check
					Path path = this.PathMatrix[this.CurrentDrag.Value.Target.Index, i];

					if (path == null) {
						continue;
					}

					affectedPaths.Add(path);

					// Get the other node
					Node otherNode = path.StartNode == this.CurrentDrag.Value.Target ? path.EndNode : path.StartNode;

                    // Calculate the new distance
                    Vector3 distance = otherNode.transform.position - newPosition;

                    // Check range
                    bool invalidDistance = distance.magnitude < 0.5f || distance.magnitude > 2f;

                    // Update material color if its correct size or nah
                    path.Line.GetComponent<Renderer>().material.color = invalidDistance ? Color.red : Color.green;

                    if (invalidDistance) {
						hasInvalidPaths = true;
					}
				}

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
		Node start = path.StartNode;
		Node end = path.EndNode;
		this.PathMatrix[start.Index, end.Index] = null;
		this.PathMatrix[end.Index, start.Index] = null;
		Destroy(path.gameObject);
		this.OnMapChanged?.Invoke();
	}
}
