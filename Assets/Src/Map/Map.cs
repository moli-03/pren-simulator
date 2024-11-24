using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Map : MonoBehaviour
{
    // Assign in editor
    public GameObject NodePrefab;
    public GameObject PathPrefab;
    public GameObject BarrierPrefab;
    public GameObject ConePrefab;
    public GameObject VehiclePrefab;
    public LayerMask GroundLayer;
    public LayerMask GraphLayer;
    private Camera MainCamera;
    public static readonly int NODE_COUNT = 8;
    private int maxBarricadeCount = 3;
    private int maxConeCount = 2;

    private int maxRemovePathCount = 3;
    private Node endNode;


    public List<Node> Nodes = new List<Node>();
    public Path[,] PathMatrix = new Path[NODE_COUNT + 1, NODE_COUNT + 1];

    struct DragInfo
    {
        public Node Target;
        public bool IsDragging;
    }

    private DragInfo? CurrentDrag = null;

    private void AddPath(Node from, Node to)
    {
        int fromIndex = from.Index;
        int toIndex = to.Index;
        Path path = Instantiate(this.PathPrefab).GetComponent<Path>().SetMap(this).From(from).To(to);
        this.PathMatrix[fromIndex, toIndex] = path;
        this.PathMatrix[toIndex, fromIndex] = path;
    }


    private void AddNode(Node node)
    {
        this.Nodes.Add(node);
        node.Index = this.Nodes.Count - 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.MainCamera = Camera.main;

        // Create all node instances on their default positions
        Node A = Instantiate(NodePrefab, new Vector3(2, 0, 0.1f), Quaternion.identity).GetComponent<Node>().SetLabel("G");
        Node B = Instantiate(NodePrefab, new Vector3(0.5f, 0, 0.5f), Quaternion.identity).GetComponent<Node>();
        Node C = Instantiate(NodePrefab, new Vector3(3.5f, 0, 0.5f), Quaternion.identity).GetComponent<Node>();
        Node D = Instantiate(NodePrefab, new Vector3(1.5f, 0, 1f), Quaternion.identity).GetComponent<Node>();
        Node E = Instantiate(NodePrefab, new Vector3(0.5f, 0, 1.5f), Quaternion.identity).GetComponent<Node>().SetLabel("A");
        Node F = Instantiate(NodePrefab, new Vector3(1.5f, 0, 1.5f), Quaternion.identity).GetComponent<Node>();
        Node G = Instantiate(NodePrefab, new Vector3(3.5f, 0, 1.5f), Quaternion.identity).GetComponent<Node>().SetLabel("C");
        Node H = Instantiate(NodePrefab, new Vector3(2f, 0, 2f), Quaternion.identity).GetComponent<Node>().SetLabel("B");

        // Add to node list
        this.AddNode(A);
        this.AddNode(B);
        this.AddNode(C);
        this.AddNode(D);
        this.AddNode(E);
        this.AddNode(F);
        this.AddNode(G);
        this.AddNode(H);

        // Create start
        Node start = Instantiate(NodePrefab, new Vector3(2, 0, -0.4f), Quaternion.identity).GetComponent<Node>().SetLabel("S");
        this.AddNode(start);

        // Create a path to the first node
        this.AddPath(start, A);

        // Add vehicle
        GameObject vehicle = Instantiate(VehiclePrefab, start.transform.position + new Vector3(0, 0.15f, 0), Quaternion.identity);

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

        List<Node> potentialEndNodes = new List<Node> { E, H, G };
        endNode = potentialEndNodes[Random.Range(0, potentialEndNodes.Count)];
		UIController.Instance.UpdateTarget(endNode);


        this.removeRandomPaths();
        this.addRandomCones();
        this.addRandomBarricade();
        // Update the positions for each path
        foreach (Path path in this.PathMatrix)
        {
            if (path == null) continue;
            path.UpdatePosition();
        }
    }


    // Update is called once per frame
    void Update()
    {
        // Check for mouse press
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(this.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit initialHit, Mathf.Infinity, this.GraphLayer))
            {
				if (initialHit.collider.TryGetComponent(out Node node)) {
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
                foreach (Path path in this.PathMatrix)
                {
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
                for (int i = 0; i < this.PathMatrix.GetLength(1); i++)
                {

                    // Get the path we want to check
                    Path path = this.PathMatrix[this.CurrentDrag.Value.Target.Index, i];

                    if (path == null)
                    {
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

                    if (invalidDistance)
                    {
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

    private void addRandomBarricade()
    {
        // Place barricades on random paths
        int barricadeCount = Random.Range(1, maxBarricadeCount);
        List<Path> allPaths = new List<Path>();
        foreach (Path path in this.PathMatrix)
        {
            if (path != null)
                allPaths.Add(path);
        }
        List<Path> pathsWithBarricades = allPaths.OrderBy(_ => Random.value).Take(barricadeCount).ToList();

        foreach (Path path in pathsWithBarricades)
        {
            // Calculate midpoint
            Vector3 midPoint = (path.StartNode.transform.position + path.EndNode.transform.position) / 2;

            // Calculate direction vector from start to end node
            Vector3 direction = (path.EndNode.transform.position - path.StartNode.transform.position).normalized;

            // Calculate a perpendicular rotation
            Quaternion perpendicularRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(-90, 0, 0);

            // Instantiate barrier with calculated position and rotation
            GameObject barrier = Instantiate(BarrierPrefab, midPoint + new Vector3(0.1f, 0, -0.1f), perpendicularRotation);
        }
    }

    private bool IsPathAvailable(Node start, Node end)
    {
        HashSet<Node> visited = new HashSet<Node>();
        return DepthFirstSearch(start, end, visited);
    }

    private bool DepthFirstSearch(Node current, Node target, HashSet<Node> visited)
    {
        if (current == target) return true;
        visited.Add(current);

        for (int i = 0; i < this.PathMatrix.GetLength(1); i++)
        {
            Path path = this.PathMatrix[current.Index, i];
            if (path == null) continue;

            Node neighbor = path.StartNode == current ? path.EndNode : path.StartNode;
            if (!visited.Contains(neighbor) && DepthFirstSearch(neighbor, target, visited))
            {
                return true;
            }
        }
        return false;
    }


    private void addRandomCones()
    {
        // Define the desired scale for the cone
        Vector3 coneScale = new Vector3(20, 20, 20);

        // Identify nodes to exclude
        Node robotStartNode = Nodes.FirstOrDefault(n => n.GetLabel() == "G");
        Node startNode = Nodes.FirstOrDefault(n => n.GetLabel() == "S");
        List<Node> excludeNodes = new List<Node> { startNode };
        excludeNodes.Add(robotStartNode);



        // Collect eligible nodes excluding the start node (S)
        List<Node> eligibleNodes = Nodes.Except(excludeNodes).ToList();

        // Randomly select nodes for cone placement
        int coneCount = Random.Range(1, maxConeCount + 1);
        List<Node> nodesWithCones = eligibleNodes.OrderBy(_ => Random.value).Take(coneCount).ToList();

        // Ensure at least one of E, H, or G is not covered
        List<Node> criticalNodes = new List<Node> {
        Nodes.FirstOrDefault(n => n.GetLabel() == "A"),
        Nodes.FirstOrDefault(n => n.GetLabel() == "B"),
        Nodes.FirstOrDefault(n => n.GetLabel() == "C")
    };

        bool allCriticalCovered = criticalNodes.All(node => nodesWithCones.Contains(node));
        if (allCriticalCovered)
        {
            // Remove one critical node randomly to keep it uncovered
            Node nodeToRemove = criticalNodes[Random.Range(0, criticalNodes.Count)];
            nodesWithCones.Remove(nodeToRemove);
        }

        // Place cones on selected nodes
        foreach (Node node in nodesWithCones)
        {
            GameObject cone = Instantiate(ConePrefab, node.transform.position + new Vector3(0, 0, 0), Quaternion.identity);
            cone.transform.localScale = coneScale;
            cone.transform.rotation = Quaternion.Euler(-90, 0, 0); // Rotate by -90 degrees on the X-axis
        }
    }

    private void removeRandomPaths()
    {
        // Get a list of all existing paths
        List<Path> allPaths = new List<Path>();
        foreach (Path path in this.PathMatrix)
        {
            if (path != null)
                allPaths.Add(path);
        }

        // Determine how many paths to remove (1-3)
        int pathsToRemoveCount = Random.Range(1, maxRemovePathCount);
        List<Path> pathsToRemove = allPaths.OrderBy(_ => Random.value).ToList();

        // Identify the start and first nodes
        Node startNode = Nodes.FirstOrDefault(n => n.GetLabel() == "S");
        Node firstNode = Nodes.FirstOrDefault(n => n.GetLabel() == "G"); 


        int removedCount = 0;

        foreach (Path path in pathsToRemove)
        {
            if ((path.StartNode == startNode && path.EndNode == firstNode) || (path.StartNode == firstNode && path.EndNode == startNode))
            {
                continue;
            }
            // Temporarily remove the path
            RemovePath(path);

            // Check if there's still a path from the start node to the end node

            removedCount++;

            // Stop if we've removed the required number of paths
            if (removedCount >= pathsToRemoveCount)
                break;
        }
    }


    public void RemovePath(Path path)
    {
        Node start = path.StartNode;
        Node end = path.EndNode;
        this.PathMatrix[start.Index, end.Index] = null;
        this.PathMatrix[end.Index, start.Index] = null;
        Destroy(path.gameObject);
    }
}
