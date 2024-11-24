using System.Linq;
using Assets.Src.Vehicle.Graph;

namespace Assets.Src.Vehicle.States {

	public class NodeReached : VehicleState
	{
		public override string Name => "NodeReached";

		public NodeReached(VehicleController vehicle) : base(vehicle) { }

		public override void Start()
		{
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
			if (previousVisitedNode != null) {
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