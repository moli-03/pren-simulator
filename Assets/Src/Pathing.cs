using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pathing : MonoBehaviour
{
	private Map map;

	private Node StartNode;

	private Node EndNode;

	private Color PathColor = Color.magenta;

    // Start is called before the first frame update
    void Start()
    {
        this.map = this.GetComponent<Map>();
		this.map.OnMapChanged += this.HandleMapChange;
    }

	private void HandleMapChange() {

		this.StartNode = this.map.Nodes[0]; // A is always the start
		this.EndNode = this.map.Nodes[this.map.Nodes.Count - 2]; // Just a random last one (change this to be selectable)

		var optimalPath = this.GetOptimalPath();

		// Reset node colors
		this.map.Nodes.ForEach(node => {
			node.SetColor(Color.white);
		});

		// Reset path colors
		foreach (Path path in this.map.PathMatrix) {
			if (path != null && path.GetColor() == this.PathColor) {
				path.SetColor(Color.white);
			}
		}

		// Show new colors
		for (int i = 0; i < optimalPath.Count; i++) {
			optimalPath[i].SetColor(this.PathColor);
			// Color path to next node
			if (i < optimalPath.Count - 1) {
				this.map.PathMatrix[optimalPath[i].Index, optimalPath[i + 1].Index].SetColor(this.PathColor);
			}
		}
	}

	
	public List<Node>? GetOptimalPath() {
		List<List<Node>> allPaths = new List<List<Node>>();
		List<Node> currentPath = new List<Node>();
		bool[] visitedNodes = new bool[Map.NODE_COUNT];

		FindAllPaths(this.StartNode, this.EndNode, visitedNodes, currentPath, allPaths);

		List<Node> optimalPath = null;
		foreach (List<Node> potentialPath in allPaths) {

			// Check path length
			if (potentialPath.Count < 4) {
				continue;
			}

			if (optimalPath == null || potentialPath.Count < optimalPath.Count) {
				optimalPath = potentialPath;
			}
		}

		return optimalPath;
	}

	private void FindAllPaths(Node current, Node end, bool[] visitedNodes, List<Node> currentPath, List<List<Node>> allPaths) {
        // Add current node to path
		visitedNodes[current.Index] = true;
        currentPath.Add(current);

		if (!current.HasCone()) {
        	// If reached the end node, save the current path
        	if (current == end) {
            	allPaths.Add(new List<Node>(currentPath));
        	}
        	else {
				for (int i = 0; i < this.map.PathMatrix.GetLength(1); i++) {

					Path path = this.map.PathMatrix[current.Index, i];
					if (path == null || visitedNodes[i]) continue;

					Node other = path.StartNode == current ? path.EndNode : path.StartNode;
					FindAllPaths(other, end, visitedNodes, currentPath, allPaths);
				}
        	}
		}

        // Backtrack: remove the current node
		visitedNodes[current.Index] = false;
        currentPath.RemoveAt(currentPath.Count - 1);
    }
}
