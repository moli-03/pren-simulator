using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Pathing : MonoBehaviour
{
	private Map map;

	private Node StartNode;

	private Node EndNode;

	private Color PathColor = Color.magenta;

	public struct PathInfo {
		public List<Node> Nodes;
		public float Length;
		public int BarrierCount;

		public PathInfo Clone() {
			return new PathInfo {
				Nodes = new List<Node>(Nodes),
				Length = this.Length,
				BarrierCount = this.BarrierCount
			};
		}
	}

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

		if (optimalPath.HasValue) {
			// Show new colors
			for (int i = 0; i < optimalPath.Value.Nodes.Count; i++) {
				optimalPath.Value.Nodes[i].SetColor(this.PathColor);
				// Color path to next node
				if (i < optimalPath.Value.Nodes.Count - 1) {
					this.map.PathMatrix[optimalPath.Value.Nodes[i].Index, optimalPath.Value.Nodes[i + 1].Index].SetColor(this.PathColor);
				}
			}
		}
	}

	
	public PathInfo? GetOptimalPath() {
		List<PathInfo> allPaths = new List<PathInfo>();
		PathInfo currentPath = new PathInfo() {
			Nodes = new List<Node>(),
			BarrierCount = 0,
			Length = 0f
		};
		bool[] visitedNodes = new bool[Map.NODE_COUNT];

		FindAllPaths(this.StartNode, this.EndNode, visitedNodes, currentPath, allPaths);

		PathInfo? optimalPath = null;
		foreach (PathInfo potentialPath in allPaths) {

			// Check path length
			if (potentialPath.Nodes.Count < 4) {
				continue;
			}

			if (
				// No optimal path found yet
				!optimalPath.HasValue
				||
				// Path with less barriers
				(potentialPath.BarrierCount < optimalPath.Value.BarrierCount)
				||
				// Path with same amount of barriers but shorter
				(potentialPath.BarrierCount == optimalPath.Value.BarrierCount && potentialPath.Length < optimalPath.Value.Length)
			) {
				optimalPath = potentialPath;
			}
		}


		Debug.Log(optimalPath);

		return optimalPath;
	}

	private void FindAllPaths(Node current, Node end, bool[] visitedNodes, PathInfo currentPath, List<PathInfo> allPaths) {
			  
        // Add current node to path
		visitedNodes[current.Index] = true;
		currentPath.Nodes.Add(current);

		if (currentPath.Nodes.Count > 1) {
			Node previous = currentPath.Nodes[currentPath.Nodes.Count - 2];
			Path path = this.map.PathMatrix[current.Index, previous.Index];
			currentPath.Length += path.GetLength();
			if (path.HasBarrier()) {
				currentPath.BarrierCount++;
			}
		}

		if (!current.HasCone()) {
        	// If reached the end node, save the current path
        	if (current == end) {
            	allPaths.Add(currentPath.Clone());
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
		if (currentPath.Nodes.Count > 1) {
			Node previous = currentPath.Nodes[currentPath.Nodes.Count - 2];
			Path path = this.map.PathMatrix[current.Index, previous.Index];
			currentPath.Length -= path.GetLength();
			if (path.HasBarrier()) {
				currentPath.BarrierCount--;
			}
		}
		visitedNodes[current.Index] = false;
        currentPath.Nodes.RemoveAt(currentPath.Nodes.Count - 1);
    }
}
