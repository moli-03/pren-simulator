using System;
using System.Linq;
using Assets.Src.Util;
using Assets.Src.Vehicle.Graph;
using UnityEngine;

namespace Assets.Src.Vehicle.States {

	public class NodeReached : VehicleState
	{
		public override string Name => "NodeReached";

		private static LayerMask GraphLayerMask = 1 << LayerMask.NameToLayer("Graph");

		public NodeReached(VehicleController vehicle) : base(vehicle) { }

		private bool isOnEndPoint() {
			if (!Physics.Raycast(this.Vehicle.transform.position, -this.Vehicle.transform.up, out RaycastHit hit, 0.5f, GraphLayerMask)) {
				return false;
			}
			
			if (!hit.collider.TryGetComponent(out Node node)) {
				return false;
			}

			return node.IsEndpoint;
		}

		public override void Start()
		{

			// Check if we have reached th end point
			if (this.isOnEndPoint()) {
				this.Vehicle.SetState(new EndpointReached(this.Vehicle));
				return;
			}

			
			// Check if we have already found a node on that position
			MapNode node = this.Vehicle.Map.GetNodeAt(this.Vehicle.Position);

			MapNode previousVisitedNode = this.Vehicle.NodeStack.LastOrDefault();

			bool previouslyFound = node != null;

			// Found for the first time
			if (!previouslyFound) {
				// Add the new node to the map
				node = this.Vehicle.Map.AddNodeAt(this.Vehicle.Position);
			}

			// Add to node stack (if its not already at the top)
			if (this.Vehicle.NodeStack.Count == 0 || previousVisitedNode != node) {
				this.Vehicle.NodeStack.Add(node);
			}

			// Connect the two nodes
			if (previousVisitedNode != null && previousVisitedNode != node) {
				this.Vehicle.Map.AddPathBetween(previousVisitedNode, node);
			}

			if (previouslyFound) {
				this.Vehicle.SetState(new ChooseNextPath(this.Vehicle));
			}
			else {
				this.Vehicle.SetState(new FindPathsOfNode(this.Vehicle));
			}
		}
	}

}