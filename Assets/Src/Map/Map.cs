using System.Collections.Generic;
using System.Linq;
using Assets.Src.Util;
using Unity.VisualScripting;
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
    private int maxBarricadeCount = 4;
    private int minBarricadeCount = 2;
    private int maxConeCount = 3;
    private int minConeCount = 1;
    private int maxRemovePathCount = 4;
    private int minRemovePathCount = 0;
    private Vector3 coneScale = new Vector3(13.5f, 13.5f, 13.5f);
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
        Node A = Instantiate(NodePrefab, new Vector3(2, 0, 0.1f), Quaternion.identity).GetComponent<Node>();
        Node B = Instantiate(NodePrefab, new Vector3(0.5f, 0, 0.5f), Quaternion.identity).GetComponent<Node>();
        Node C = Instantiate(NodePrefab, new Vector3(3.5f, 0, 0.5f), Quaternion.identity).GetComponent<Node>();
        Node D = Instantiate(NodePrefab, new Vector3(1.75f, 0, 0.75f), Quaternion.identity).GetComponent<Node>();
        Node E = Instantiate(NodePrefab, new Vector3(0.5f, 0, 1.5f), Quaternion.identity).GetComponent<Node>().SetLabel("A");
        Node F = Instantiate(NodePrefab, new Vector3(1.5f, 0, 1.5f), Quaternion.identity).GetComponent<Node>();
        Node G = Instantiate(NodePrefab, new Vector3(3.25f, 0, 1.5f), Quaternion.identity).GetComponent<Node>().SetLabel("C");
        Node H = Instantiate(NodePrefab, new Vector3(2f, 0, 2.75f), Quaternion.identity).GetComponent<Node>().SetLabel("B");

        // Create start
        Node start = Instantiate(NodePrefab, new Vector3(2, 0, -0.4f), Quaternion.identity).GetComponent<Node>();
        this.AddNode(start);

        // Add to node list
        this.AddNode(A);
        this.AddNode(B);
        this.AddNode(C);
        this.AddNode(D);
        this.AddNode(E);
        this.AddNode(F);
        this.AddNode(G);
        this.AddNode(H);

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

        // Choose a random target
        List<Node> potentialEndNodes = new List<Node> { E, H, G };
        endNode = potentialEndNodes[Random.Range(0, potentialEndNodes.Count)];
        endNode.IsEndpoint = true;
        VehicleController vehicleController = vehicle.GetComponent<VehicleController>();

        // Set the endNode in the VehicleController
        if (vehicleController != null)
        {
            vehicleController.SetEndNode(endNode);
        }
        UIController.Instance.UpdateTarget(endNode);

        this.RandomizePath();

        // this.removeRandomPaths();
        // this.addRandomCones();
        // this.addRandomBarricade();
        // Update the positions for each path
        foreach (Path path in this.PathMatrix)
        {
            if (path == null) continue;
            path.UpdatePosition();
        }
        DisplaySignedAngles();
    }


    // Update is called once per frame
    void Update()
    {
        // Check for mouse press
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(this.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit initialHit, Mathf.Infinity))
            {
                if (initialHit.collider.TryGetComponent(out Node node))
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

    private void DisplaySignedAngles()
    {
        Vector2 referenceDirection = Vector2.down; // Downward direction as reference (0, -1)

        foreach (Node node in Nodes)
        {
            Debug.Log($"Node {node.GetLabel()}:");

            // Find all connected paths
            for (int i = 0; i < PathMatrix.GetLength(1); i++)
            {
                Path path = PathMatrix[node.Index, i];
                if (path == null) continue;

                // Get the other node to calculate the direction vector
                Node otherNode = path.StartNode == node ? path.EndNode : path.StartNode;
                Vector3 direction3D = otherNode.transform.position - node.transform.position;

                // Convert to 2D (XZ plane) for angle calculation
                Vector2 direction2D = new Vector2(direction3D.x, direction3D.z).normalized;

                // Calculate the signed angle
                float angle = Vector2.SignedAngle(referenceDirection, direction2D);

                // Normalize to go from 0 to 680 to sort them by direction
                float normalizedAngle = angle < 0 ?  360 - (angle * -1) + 360 : angle;

                // Log the angle
                Debug.Log($"  Path to Node {otherNode.GetLabel()} - Signed Angle: {angle}°, Normalized Angle: {normalizedAngle}°");

                // Place the text at the position where the path starts at the node
                Vector3 textPosition = node.transform.position + direction3D.normalized * 0.1f; // Offset slightly from the node

                // Create a text object
                GameObject angleTextObject = new GameObject("AngleText");
                angleTextObject.transform.position = textPosition; // Slightly above ground for better visibility
                angleTextObject.transform.rotation = Quaternion.Euler(90, 0, 0); // Rotate to make it readable from above
                angleTextObject.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Scale down for better fit
                TextMesh textMesh = angleTextObject.AddComponent<TextMesh>();
                textMesh.text = $"{normalizedAngle:F1}°"; // Format angle to 1 decimal place
                textMesh.characterSize = 0.1f;
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.alignment = TextAlignment.Center;
                textMesh.color = Color.black;
            }
        }
    }



    private void RandomizePath()
    {

        List<List<Node>> allPaths = new List<List<Node>>();
        List<Node> currentPath = new List<Node>();

        bool[] visitedNodes = new bool[this.Nodes.Count];
        FindAllPaths(this.Nodes.First(), this.endNode, visitedNodes, currentPath, allPaths);

        // Choose a random path
        List<Node> randomPath = allPaths[Random.Range(0, allPaths.Count - 1)];

        // The actual path
        List<Node> path = new List<Node>();

        // We need to backtrack now to make sure we don't make too complicated paths leading
        // to less positions we can place cones
        int i = randomPath.Count - 1;
        while (i >= 0)
        {

            Node current = randomPath[i];
            path.Add(current);

            bool updatedIndex = false;
            for (int j = 0; j < this.PathMatrix.GetLength(0); j++)
            {

                // No path?
                if (this.PathMatrix[current.Index, j] == null || j == current.Index)
                {
                    continue;
                }

                Node other = this.Nodes[j];
                int firstIndex = randomPath.IndexOf(other);

                // Check if the time we visit the node is the previous node. If not -> update path
                if (firstIndex >= 0 && firstIndex < i)
                {
                    updatedIndex = true;
                    i = firstIndex;
                    continue;
                }
            }

            if (!updatedIndex)
            {
                i--;
            }
        }

        // We filled it up end -> front so we have to reverse it
        path.Reverse();

        // Visualize it
        this.DrawDebugPath(path, Color.magenta, 0.1f);

        // Get all nodes that are not on the path
        List<Node> nodesNotOnPath = this.Nodes.Skip(1).Where(node => !path.Contains(node)).OrderBy(_ => Random.value).ToList();

        // Place some cones on them
        List<Node> nodesWithCones = this.PlaceConesRandom(nodesNotOnPath);

        // Remove some paths
        this.RemoveRandomPaths(path);

        // Place some random barriers
        this.PlaceBarriersRandom(nodesWithCones);
    }


    private void RemoveRandomPaths(List<Node> path)
    {

        // Get all paths that are required to make the path because we cant delete those
        List<Path> requiredPaths = new List<Path>();
        Node prev = path[0];

        for (int i = 1; i < path.Count; i++)
        {
            requiredPaths.Add(this.PathMatrix[prev.Index, path[i].Index]);
            prev = path[i];
        }

        // Get all the paths we can delete
        List<Path> deletablePaths = new List<Path>();
        for (int i = 0; i < this.PathMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < this.PathMatrix.GetLength(1); j++)
            {

                if (this.PathMatrix[i, j] == null)
                {
                    continue;
                }

                if (!requiredPaths.Contains(this.PathMatrix[i, j]))
                {
                    deletablePaths.Add(this.PathMatrix[i, j]);
                }
            }
        }

        // Randomize the list
        deletablePaths = deletablePaths.OrderBy(_ => Random.value).ToList();

        // Random amount of paths we delete
        int deletePathCount = Random.Range(this.minRemovePathCount, this.maxRemovePathCount);
        deletePathCount = Mathf.Clamp(deletePathCount, 0, deletablePaths.Count - 1);

        Debug.Log("Removing " + deletePathCount + " paths");

        for (int i = 0; i < deletePathCount; i++)
        {
            this.RemovePath(deletablePaths[i]);
        }
    }


    private void PlaceBarriersRandom(List<Node> nodesWithCones)
    {

        List<Node> blockedNodes = new List<Node>(nodesWithCones);
        blockedNodes.Add(this.Nodes[0]); // Add the start node

        // Get all paths on which we could place a barrier
        List<Path> possiblePaths = new List<Path>();
        for (int i = 0; i < this.PathMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < this.PathMatrix.GetLength(1); j++)
            {

                Path current = this.PathMatrix[i, j];

                if (current == null)
                {
                    continue;
                }

                // Don't add twice
                if (possiblePaths.Contains(current))
                {
                    continue;
                }

                // Add to possible paths if the start and end node are not blocked
                if (!blockedNodes.Contains(current.StartNode) && !blockedNodes.Contains(current.EndNode))
                {
                    possiblePaths.Add(current);
                }
            }
        }

        int barrierCount = Random.Range(this.minBarricadeCount, this.maxBarricadeCount);
        barrierCount = Mathf.Clamp(barrierCount, 0, possiblePaths.Count - 1);

        // Randomize paths
        possiblePaths = possiblePaths.OrderBy(_ => Random.value).ToList();

        // Place the barriers
        for (int i = 0; i < barrierCount; i++)
        {

            Path path = possiblePaths[i];

            // Calculate midpoint
            Vector3 direction = path.EndNode.transform.position - path.StartNode.transform.position;
            Quaternion pathRotation = Quaternion.LookRotation(direction);
            float distanceFromStart = Mathf.Clamp(direction.magnitude * Random.Range(0f, 1f), Constants.BARRIER_MIN_DISTANCE_FROM_NODE, direction.magnitude - Constants.BARRIER_MIN_DISTANCE_FROM_NODE);
            Vector3 position = path.StartNode.transform.position + direction.normalized * distanceFromStart;

            // We have to spin it a little
            Quaternion rotation = Quaternion.Euler(new Vector3(-90f, pathRotation.eulerAngles.y, 0));

            // The middle of the prefab is on the left of the barrier
            position -= pathRotation * new Vector3(0.076f, 0, 0);

            // Instantiate barrier with calculated position and rotation
            GameObject barrier = Instantiate(BarrierPrefab, position, rotation);
            barrier.AddComponent<Moveable>();
        }
    }


    private List<Node> PlaceConesRandom(List<Node> nodes)
    {

        // Take a random amount of cones
        int coneCount = Random.Range(this.minConeCount, this.maxConeCount);

        // Make sure we dont want to place more cones then there are empty nodes
        coneCount = Mathf.Clamp(coneCount, 0, nodes.Count);

        // Store all blocked nodes
        List<Node> nodesWithCone = new List<Node>();

        for (int i = 0; i < coneCount; i++)
        {
            nodesWithCone.Add(nodes[i]);

            GameObject cone = Instantiate(ConePrefab, nodes[i].transform.position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            cone.transform.localScale = coneScale;
            cone.transform.rotation = Quaternion.Euler(-90, 0, 0); // Rotate by -90 degrees on the X-axis
            cone.AddComponent<Moveable>();
            cone.AddComponent<MeshCollider>();

        }

        return nodesWithCone;
    }


    private void DrawDebugPath(List<Node> path, Color col, float height)
    {

        // Debug only
        for (int i = 0; i < path.Count; i++)
        {
            Node node = path[i];
            Vector3 position = new Vector3(node.transform.position.x, height, node.transform.position.z);
            Draw.DrawCircle(position, col);

            // Draw path to previous node
            if (i > 0)
            {
                Node prev = path[i - 1];
                Vector3 direction = prev.transform.position - node.transform.position;
                direction = new Vector3(direction.x, 0, direction.z);

                Draw.DrawLine(position, direction, col);
            }
        }
    }


    private void FindAllPaths(Node current, Node end, bool[] visitedNodes, List<Node> currentPath, List<List<Node>> allPaths)
    {

        // Add current node to path
        visitedNodes[current.Index] = true;
        currentPath.Add(current);

        // If reached the end node, save the current path
        if (current == end)
        {
            allPaths.Add(new List<Node>(currentPath));
        }
        else
        {
            for (int i = 0; i < this.PathMatrix.GetLength(1); i++)
            {

                Path path = this.PathMatrix[current.Index, i];
                if (path == null || visitedNodes[i]) continue;

                Node other = path.StartNode == current ? path.EndNode : path.StartNode;
                FindAllPaths(other, end, visitedNodes, currentPath, allPaths);
            }
        }

        // Backtrack: remove the current node
        visitedNodes[current.Index] = false;
        currentPath.RemoveAt(currentPath.Count - 1);
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
            Vector3 direction = path.EndNode.transform.position - path.StartNode.transform.position;
            Quaternion pathRotation = Quaternion.LookRotation(direction);
            float distanceFromStart = Mathf.Clamp(direction.magnitude * Random.Range(0f, 1f), Constants.BARRIER_MIN_DISTANCE_FROM_NODE, direction.magnitude - Constants.BARRIER_MIN_DISTANCE_FROM_NODE);
            Vector3 position = path.StartNode.transform.position + direction.normalized * distanceFromStart;

            // We have to spin it a little
            Quaternion rotation = Quaternion.Euler(new Vector3(-90f, pathRotation.eulerAngles.y, 0));

            // The middle of the prefab is on the right of the barrier
            position -= pathRotation * new Vector3(-0.089f, 0, 0);

            // Instantiate barrier with calculated position and rotation
            GameObject barrier = Instantiate(BarrierPrefab, position, rotation);
            barrier.AddComponent<Moveable>();
            barrier.AddComponent<MeshCollider>();
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
        Vector3 coneScale = new Vector3(16.5f, 16.5f, 16.5f);

        // Identify nodes to exclude
        Node robotStartNode = Nodes.FirstOrDefault(n => n.GetLabel() == "G");
        Node startNode = Nodes.FirstOrDefault(n => n.GetLabel() == "S");
        List<Node> excludeNodes = new List<Node>
        {
            startNode,
            robotStartNode
        };

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
            GameObject cone = Instantiate(ConePrefab, node.transform.position, Quaternion.identity);
            cone.transform.localScale = coneScale;
            cone.transform.rotation = Quaternion.Euler(-90, 0, 0); // Rotate by -90 degrees on the X-axis
            cone.AddComponent<Moveable>();
            cone.AddComponent<MeshCollider>();
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
